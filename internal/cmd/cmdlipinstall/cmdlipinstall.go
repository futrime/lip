package cmdlipinstall

import (
	"container/list"
	"flag"
	"fmt"
	"net/url"
	"os"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/install"
	"github.com/lippkg/lip/internal/network"
	"github.com/lippkg/lip/internal/path"

	specifierpkg "github.com/lippkg/lip/internal/specifier"
	"github.com/lippkg/lip/internal/tooth"
	log "github.com/sirupsen/logrus"
)

type FlagDict struct {
	helpFlag           bool
	upgradeFlag        bool
	forceReinstallFlag bool
	yesFlag            bool
	noDependenciesFlag bool
}

const helpMessage = `
Usage:
  lip install [options] <specifier> [...]

Description:
  Install teeth from:

  - tooth repositories. (e.g. "github.com/tooth-hub/llbds3@3.1.0")
  - local tooth archives. (e.g. "./foo.tth")

Options:
  -h, --help                  Show help.
  --upgrade                   Upgrade the specified tooth to the newest available version.
  --force-reinstall           Reinstall the tooth even if they are already up-to-date.
  -y, --yes                   Assume yes to all prompts and run non-interactively.
  --no-dependencies           Do not install dependencies. Also bypass prerequisite checks.

Note:
  Any string ends with .tth is considered as a local tooth archive path.
`

func Run(ctx context.Context, args []string) error {
	var err error

	flagSet := flag.NewFlagSet("install", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.upgradeFlag, "upgrade", false, "")
	flagSet.BoolVar(&flagDict.forceReinstallFlag, "force-reinstall", false, "")
	flagSet.BoolVar(&flagDict.yesFlag, "yes", false, "")
	flagSet.BoolVar(&flagDict.yesFlag, "y", false, "")
	flagSet.BoolVar(&flagDict.noDependenciesFlag, "no-dependencies", false, "")
	err = flagSet.Parse(args)
	if err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		fmt.Print(helpMessage)
		return nil
	}

	// At least one specifier is required.
	if flagSet.NArg() == 0 {
		return fmt.Errorf("at least one specifier is required")
	}

	log.Info("Downloading teeth and resolving dependencies...")

	// 1. Download teeth specified by the user and resolve their dependencies.

	archiveToInstallList, err := parseAndDownloadspecifierpkgtringList(ctx, flagSet.Args())
	if err != nil {
		return fmt.Errorf("failed to parse and download specifier string list: %w", err)
	}

	// 2. Resolve dependencies and check prerequisites.

	if !flagDict.noDependenciesFlag {
		archiveToInstallList, err = resolveDependencies(ctx, archiveToInstallList, flagDict.upgradeFlag,
			flagDict.forceReinstallFlag)

		if err != nil {
			return fmt.Errorf("failed to resolve dependencies: %w", err)
		}

		missingPrerequisiteMap, err := findMissingPrerequisites(ctx, archiveToInstallList)
		if err != nil {
			return fmt.Errorf("failed to find missing prerequisites: %w", err)
		}

		if len(missingPrerequisiteMap) > 0 {
			missingPrerequisiteMsg := "\n"
			for prerequisite := range missingPrerequisiteMap {
				missingPrerequisiteMsg += fmt.Sprintf("  %v\n", prerequisite)
			}

			return fmt.Errorf("missing prerequisites: %v", missingPrerequisiteMsg)
		}
	}

	// 3. Sort teeth.

	archiveToInstallList, err = install.SortToothArchives(archiveToInstallList)
	if err != nil {
		return fmt.Errorf("failed to sort teeth: %w", err)
	}

	// 4. Ask for confirmation.

	if !flagDict.yesFlag {
		err = askForConfirmation(ctx, archiveToInstallList)
		if err != nil {
			return err
		}
	}

	// 5. Install teeth.

	err = installToothArchiveList(ctx, archiveToInstallList, flagDict.forceReinstallFlag,
		flagDict.upgradeFlag)
	if err != nil {
		return fmt.Errorf("failed to install teeth: %w", err)
	}

	log.Info("Done.")

	return nil
}

// ---------------------------------------------------------------------

// askForConfirmation asks for confirmation before installing the tooth.
func askForConfirmation(ctx context.Context,
	archiveList []tooth.Archive) error {

	// Print the list of teeth to be installed.
	log.Info("The following teeth will be installed:")
	for _, archive := range archiveList {
		log.Infof("  %v: %v", archive.Metadata().Tooth(),
			archive.Metadata().Info().Name)
	}

	// Ask for confirmation.
	log.Info("Do you want to continue? [y/N]")
	var ans string
	fmt.Scanln(&ans)
	if ans != "y" && ans != "Y" {
		return fmt.Errorf("aborted")
	}

	return nil
}

// downloadFromAllGoProxies downloads the tooth from all Go proxies and returns
// the path to the downloaded tooth.
func downloadFromAllGoProxies(ctx context.Context, toothRepo string,
	toothVersion semver.Version) (string, error) {

	var err error

	log.Infof("Downloading %v@%v...", toothRepo, toothVersion)

	goModuleProxyURL, err := ctx.GoModuleProxyURL()
	if err != nil {
		return "", fmt.Errorf("failed to get Go module proxy URL: %w", err)
	}

	downloadURL, err := network.GenerateGoModuleZipFileURL(toothRepo, toothVersion, goModuleProxyURL)
	if err != nil {
		return "", fmt.Errorf("failed to generate Go module zip file URL: %w", err)
	}

	cacheDir, err := ctx.CacheDir()
	if err != nil {
		return "", fmt.Errorf("failed to get cache directory: %w", err)
	}

	cacheFileName := url.QueryEscape(downloadURL.String())
	cachePath := cacheDir.Join(path.MustParse(cacheFileName))

	// Skip downloading if the tooth is already in the cache.
	if _, err := os.Stat(cachePath.LocalString()); err == nil {
		return cachePath.LocalString(), nil
	} else if !os.IsNotExist(err) {
		return "", fmt.Errorf("failed to check if file exists: %w", err)
	}

	enableProgressBar := true
	if log.GetLevel() == log.PanicLevel || log.GetLevel() == log.FatalLevel ||
		log.GetLevel() == log.ErrorLevel || log.GetLevel() == log.WarnLevel {
		enableProgressBar = false
	}

	err = network.DownloadFile(downloadURL, cachePath, enableProgressBar)
	if err != nil {
		return "", fmt.Errorf("failed to download file: %w", err)
	}

	return cachePath.LocalString(), nil
}

// downloadSpecifier downloads the tooth specified by the specifier and returns
// the path to the downloaded tooth.
func downloadSpecifier(ctx context.Context,
	specifier specifierpkg.Specifier) (tooth.Archive, error) {
	switch specifier.Kind() {
	case specifierpkg.ToothArchiveKind:
		archivePath, err := specifier.ToothArchivePath()
		if err != nil {
			return tooth.Archive{}, fmt.Errorf("failed to get tooth archive path: %w", err)
		}

		archive, err := tooth.MakeArchive(archivePath)
		if err != nil {
			return tooth.Archive{}, fmt.Errorf("failed to create archive%v: %w", archivePath, err)
		}

		return archive, nil

	case specifierpkg.ToothRepoKind:
		toothRepo, err := specifier.ToothRepoPath()
		if err != nil {
			return tooth.Archive{}, fmt.Errorf("failed to get tooth repo: %w", err)
		}

		var toothVersion semver.Version
		if ok, err := specifier.IsToothVersionSpecified(); err == nil && ok {
			toothVersion, err = specifier.ToothVersion()
			if err != nil {
				return tooth.Archive{}, fmt.Errorf("failed to get tooth version: %w", err)
			}
		} else {
			toothVersion, err = tooth.GetLatestStableVersion(ctx, toothRepo)
			if err != nil {
				return tooth.Archive{}, fmt.Errorf("failed to look up tooth version: %w", err)
			}
		}

		if err != nil {
			return tooth.Archive{}, fmt.Errorf("failed to get tooth repo: %w", err)
		}

		archivePath, err := downloadFromAllGoProxies(ctx, toothRepo, toothVersion)
		if err != nil {
			return tooth.Archive{}, fmt.Errorf("failed to download from all Go proxies: %w", err)
		}

		archive, err := tooth.MakeArchive(archivePath)
		if err != nil {
			return tooth.Archive{}, fmt.Errorf("failed to create archive %v: %w", archivePath, err)
		}

		validateArchive(archive, toothRepo, toothVersion)

		return archive, nil
	}

	// Never reach here.
	panic("unreachable")
}

// findMissingPrerequisites finds missing prerequisites of the tooth specified
// by the specifier and returns the map of missing prerequisites.
func findMissingPrerequisites(ctx context.Context,
	archiveList []tooth.Archive) (map[string]semver.Range, error) {
	var missingPrerequisiteMap = make(map[string]semver.Range)

	for _, archive := range archiveList {
		for prerequisite, versionRange := range archive.Metadata().Prerequisites() {
			isInstalled, err := tooth.IsToothInstalled(ctx, prerequisite)
			if err != nil {
				return nil, fmt.Errorf("failed to check if tooth is installed: %w", err)
			}

			if isInstalled {
				currentMetadata, err := tooth.GetInstalledToothMetadata(ctx, prerequisite)
				if err != nil {
					return nil, fmt.Errorf("failed to find installed tooth metadata: %w", err)
				}

				if !versionRange(currentMetadata.Version()) {
					missingPrerequisiteMap[prerequisite] = versionRange
				}

				break
			} else {
				// Check if the tooth is in the archive list.
				isInArchiveList := false
				for _, archive := range archiveList {
					if archive.Metadata().Tooth() == prerequisite && versionRange(archive.Metadata().Version()) {
						isInArchiveList = true
						break
					}
				}

				if !isInArchiveList {
					missingPrerequisiteMap[prerequisite] = versionRange
				}
			}
		}
	}

	return missingPrerequisiteMap, nil
}

// installToothArchiveList installs the tooth archive list.
func installToothArchiveList(ctx context.Context,
	archiveToInstallList []tooth.Archive, forceReinstall bool, upgrade bool) error {
	for _, archive := range archiveToInstallList {
		isInstalled, err := tooth.IsToothInstalled(ctx, archive.Metadata().Tooth())
		if err != nil {
			return fmt.Errorf("failed to check if tooth is installed: %w", err)
		}

		shouldInstall := false
		shouldUninstall := false

		if isInstalled && forceReinstall {
			log.Infof("Reinstalling tooth %v...", archive.Metadata().Tooth())

			shouldInstall = true
			shouldUninstall = true

		} else if isInstalled && upgrade {
			currentMetadata, err := tooth.GetInstalledToothMetadata(ctx,
				archive.Metadata().Tooth())
			if err != nil {
				return fmt.Errorf("failed to find installed tooth metadata: %w", err)
			}

			if archive.Metadata().Version().GT(currentMetadata.Version()) {
				log.Infof("Upgrading tooth %v...", archive.Metadata().Tooth())

				shouldInstall = true
				shouldUninstall = true
			} else {
				log.Infof("Tooth %v is already up-to-date", archive.Metadata().Tooth())

				shouldInstall = false
				shouldUninstall = false
			}

		} else if isInstalled {
			log.Infof("Tooth %v is already installed", archive.Metadata().Tooth())

			shouldInstall = false
			shouldUninstall = false
		} else {
			log.Infof("Installing tooth %v...", archive.Metadata().Tooth())

			shouldInstall = true
			shouldUninstall = false
		}

		if shouldUninstall {
			err = install.Uninstall(ctx, archive.Metadata().Tooth())
			if err != nil {
				return fmt.Errorf("failed to uninstall tooth: %w", err)
			}
		}

		if shouldInstall {
			err = install.Install(ctx, archive)
			if err != nil {
				return fmt.Errorf("failed to install tooth: %w", err)
			}
		}
	}

	return nil
}

// parseAndDownloadspecifierpkgtringList parses the specifier string list and
// downloads the tooth specified by the specifier, and returns the list of
// downloaded tooth archives.
func parseAndDownloadspecifierpkgtringList(ctx context.Context,
	specifierpkgtringList []string) ([]tooth.Archive, error) {

	archiveList := make([]tooth.Archive, 0)

	for _, specifierpkgtring := range specifierpkgtringList {
		specifier, err := specifierpkg.Parse(specifierpkgtring)
		if err != nil {
			return nil, fmt.Errorf("failed to parse specifier: %w", err)
		}

		archive, err := downloadSpecifier(ctx, specifier)
		if err != nil {
			return nil, fmt.Errorf("failed to install specifier: %w", err)
		}

		archiveList = append(archiveList, archive)
	}

	return archiveList, nil
}

// resolveDependencies resolves the dependencies of the tooth specified by the
// specifier and returns the paths to the downloaded teeth. rootArchiveList
// contains the root tooth archives to resolve dependencies.
// The first return value indicates whether the dependencies are resolved.
func resolveDependencies(ctx context.Context, rootArchiveList []tooth.Archive,
	upgradeFlag bool, forceReinstallFlag bool) ([]tooth.Archive, error) {

	var err error

	// fixedToothVersionMap records versions of teeth that are already installed
	// or will be installed.
	fixedToothVersionMap := make(map[string]semver.Version)

	installedToothMetadataList, err := tooth.GetAllInstalledToothMetadata(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to get all installed tooth metadata: %w", err)
	}

	for _, installedToothMetadata := range installedToothMetadataList {
		fixedToothVersionMap[installedToothMetadata.Tooth()] = installedToothMetadata.Version()
	}

	for _, rootArchive := range rootArchiveList {
		if _, ok := fixedToothVersionMap[rootArchive.Metadata().Tooth()]; !ok {
			// If the tooth is not installed, fix the version to the version of
			// the root tooth.
			fixedToothVersionMap[rootArchive.Metadata().Tooth()] = rootArchive.Metadata().Version()

		} else {
			// If the tooth is installed, check if the tooth should be reinstalled
			if !forceReinstallFlag && !(upgradeFlag && rootArchive.Metadata().Version().GT(
				fixedToothVersionMap[rootArchive.Metadata().Tooth()])) {
				continue
			}
		}
	}

	notResolvedArchiveQueue := list.New()
	for _, rootArchive := range rootArchiveList {
		notResolvedArchiveQueue.PushBack(rootArchive)
	}

	resolvedArchiveList := make([]tooth.Archive, 0)

	for notResolvedArchiveQueue.Len() > 0 {
		archive := notResolvedArchiveQueue.Front().Value.(tooth.Archive)
		notResolvedArchiveQueue.Remove(notResolvedArchiveQueue.Front())

		depMap := archive.Metadata().Dependencies()

		for dep, versionRange := range depMap {
			if _, ok := fixedToothVersionMap[dep]; ok {
				if !versionRange(fixedToothVersionMap[dep]) {
					return nil, fmt.Errorf("installed tooth %v does not match dependency %v",
						dep, dep)
				}

				// Avoid downloading the same tooth multiple times.
				continue
			}

			versionList, err := tooth.GetAvailableVersions(ctx, dep)
			if err != nil {
				return nil, fmt.Errorf("failed to get available version list: %w", err)
			}

			var targetVersion semver.Version
			isTargetVersionFound := false

			// First find stable versions
			for _, version := range versionList {
				if len(version.Pre) == 0 && versionRange(version) {
					targetVersion = version
					isTargetVersionFound = true
					break
				}
			}

			// If no stable version is found, find any version
			if !isTargetVersionFound {
				for _, version := range versionList {
					if versionRange(version) {
						targetVersion = version
						isTargetVersionFound = true
						break
					}
				}
			}

			if !isTargetVersionFound {
				return nil, fmt.Errorf("no available version found for dependency %v", dep)
			}

			archivePath, err := downloadFromAllGoProxies(ctx, dep, targetVersion)
			if err != nil {
				return nil, fmt.Errorf("failed to download tooth: %w", err)
			}

			currentArchive, err := tooth.MakeArchive(archivePath)
			if err != nil {
				return nil, fmt.Errorf("failed to create archive %v: %w", archivePath, err)
			}

			validateArchive(currentArchive, dep, targetVersion)

			notResolvedArchiveQueue.PushBack(currentArchive)

			fixedToothVersionMap[dep] = targetVersion
		}

		resolvedArchiveList = append(resolvedArchiveList, archive)
	}

	return resolvedArchiveList, nil
}

// validateArchive validates the archive.
func validateArchive(archive tooth.Archive, toothRepo string, version semver.Version) error {
	if archive.Metadata().Tooth() != toothRepo {
		return fmt.Errorf("tooth name mismatch: %v != %v", archive.Metadata().Tooth(), toothRepo)
	}

	if archive.Metadata().Version().NE(version) {
		return fmt.Errorf("tooth version mismatch: %v != %v", archive.Metadata().Version(), version)
	}

	return nil
}
