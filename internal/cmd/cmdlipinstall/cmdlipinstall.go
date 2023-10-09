package cmdlipinstall

import (
	"container/list"
	"flag"
	"fmt"
	"os"

	"github.com/lippkg/lip/internal/contexts"
	"github.com/lippkg/lip/internal/downloading"
	"github.com/lippkg/lip/internal/installing"
	"github.com/lippkg/lip/internal/logging"
	"github.com/lippkg/lip/internal/specifiers"
	"github.com/lippkg/lip/internal/teeth"
	"github.com/lippkg/lip/internal/versions"
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

func Run(ctx contexts.Context, args []string) error {
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
		logging.Info(helpMessage)
		return nil
	}

	// At least one specifier is required.
	if flagSet.NArg() == 0 {
		return fmt.Errorf("at least one specifier is required")
	}

	logging.Info("Downloading teeth and resolving dependencies...")

	// 1. Download teeth specified by the user and resolve their dependencies.

	archiveToInstallList, err := parseAndDownloadSpecifierStringList(ctx, flagSet.Args())
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

		// TODO: check prerequisites.
	}

	// 3. Sort teeth.

	archiveToInstallList, err = installing.SortToothArchives(archiveToInstallList)
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

	logging.Info("Done.")

	return nil
}

// ---------------------------------------------------------------------

// askForConfirmation asks for confirmation before installing the tooth.
func askForConfirmation(ctx contexts.Context,
	archiveList []teeth.Archive) error {

	// Print the list of teeth to be installed.
	logging.Info("The following teeth will be installed:")
	for _, archive := range archiveList {
		logging.Info("  %v: %v", archive.Metadata().Tooth(),
			archive.Metadata().Info().Name)
	}

	// Ask for confirmation.
	logging.Info("Do you want to continue? [y/N]")
	var ans string
	fmt.Scanln(&ans)
	if ans != "y" && ans != "Y" {
		return fmt.Errorf("aborted")
	}

	return nil
}

// downloadFromAllGoProxies downloads the tooth from all Go proxies and returns
// the path to the downloaded tooth.
func downloadFromAllGoProxies(ctx contexts.Context, toothRepo string,
	toothVersion versions.Version) (string, error) {

	var errList []error

	logging.Info("Downloading %v@%v...", toothRepo, toothVersion)

	for _, goProxy := range ctx.GoProxyList() {
		var err error

		downloadURL := downloading.CalculateDownloadURLViaGoProxy(
			goProxy, toothRepo, toothVersion)

		cachePath, err := ctx.CalculateCachePath(downloadURL)
		if err != nil {
			errList = append(errList,
				fmt.Errorf("failed to calculate cache path: %w", err))
		}

		// Skip downloading if the tooth is already in the cache.
		if _, err := os.Stat(cachePath); err == nil {
			return cachePath, nil
		} else if !os.IsNotExist(err) {
			errList = append(errList,
				fmt.Errorf("failed to check if the tooth is in the cache: %w", err))
			continue
		}

		pbarStyle := downloading.StyleDefault
		if logging.LoggingLevel() > logging.InfoLevel {
			pbarStyle = downloading.StyleNone
		}

		err = downloading.DownloadFile(downloadURL, cachePath, pbarStyle)
		if err != nil {
			errList = append(errList,
				fmt.Errorf("failed to download file: %w", err))
			continue
		}

		return cachePath, nil
	}

	return "", fmt.Errorf("failed to download from all Go proxies: %v", errList)
}

// downloadSpecifier downloads the tooth specified by the specifier and returns
// the path to the downloaded tooth.
func downloadSpecifier(ctx contexts.Context,
	specifier specifiers.Specifier) (string, error) {
	switch specifier.Type() {
	case specifiers.ToothArchiveKind:
		archivePath, err := specifier.ToothArchivePath()
		if err != nil {
			return "", fmt.Errorf("failed to get tooth archive path: %w", err)
		}

		return archivePath, nil

	case specifiers.ToothRepoKind:
		toothRepo, err := specifier.ToothRepo()
		if err != nil {
			return "", fmt.Errorf("failed to get tooth repo: %w", err)
		}

		toothVersion, err := teeth.GetToothLatestStableVersion(ctx, toothRepo)
		if err != nil {
			return "", fmt.Errorf("failed to look up tooth version: %w", err)
		}

		if err != nil {
			return "", fmt.Errorf("failed to get tooth repo: %w", err)
		}

		archivePath, err := downloadFromAllGoProxies(ctx, toothRepo, toothVersion)
		if err != nil {
			return "", fmt.Errorf("failed to download from all Go proxies: %w", err)
		}

		archive, err := teeth.NewArchive(archivePath)
		if err != nil {
			return "", fmt.Errorf("failed to create archive: %w", err)
		}

		validateArchive(archive, toothRepo, toothVersion)

		return archivePath, nil
	}

	// Never reach here.
	panic("unreachable")
}

// installToothArchiveList installs the tooth archive list.
func installToothArchiveList(ctx contexts.Context,
	archiveToInstallList []teeth.Archive, forceReinstall bool, upgrade bool) error {
	for _, archive := range archiveToInstallList {
		isInstalled, err := teeth.CheckIsToothInstalled(ctx, archive.Metadata().Tooth())
		if err != nil {
			return fmt.Errorf("failed to check if tooth is installed: %w", err)
		}

		shouldInstall := false
		shouldUninstall := false

		if isInstalled && forceReinstall {
			logging.Info("Reinstalling tooth %v...", archive.Metadata().Tooth())

			shouldInstall = true
			shouldUninstall = true

		} else if isInstalled && upgrade {
			currentMetadata, err := teeth.GetInstalledToothMetadata(ctx,
				archive.Metadata().Tooth())
			if err != nil {
				return fmt.Errorf("failed to find installed tooth metadata: %w", err)
			}

			if versions.GreaterThan(archive.Metadata().Version(), currentMetadata.Version()) {
				logging.Info("Upgrading tooth %v...", archive.Metadata().Tooth())

				shouldInstall = true
				shouldUninstall = true
			} else {
				logging.Info("Tooth %v is already up-to-date", archive.Metadata().Tooth())

				shouldInstall = false
				shouldUninstall = false
			}

		} else if isInstalled {
			logging.Info("Tooth %v is already installed", archive.Metadata().Tooth())

			shouldInstall = false
			shouldUninstall = false
		} else {
			logging.Info("Installing tooth %v...", archive.Metadata().Tooth())

			shouldInstall = true
			shouldUninstall = false
		}

		if shouldUninstall {
			err = installing.Uninstall(ctx, archive.Metadata().Tooth())
			if err != nil {
				return fmt.Errorf("failed to uninstall tooth: %w", err)
			}
		}

		if shouldInstall {
			err = installing.Install(ctx, archive)
			if err != nil {
				return fmt.Errorf("failed to install tooth: %w", err)
			}
		}
	}

	return nil
}

// parseAndDownloadSpecifierStringList parses the specifier string list and
// downloads the tooth specified by the specifier, and returns the list of
// downloaded tooth archives.
func parseAndDownloadSpecifierStringList(ctx contexts.Context,
	specifierStringList []string) ([]teeth.Archive, error) {

	archiveList := make([]teeth.Archive, 0)

	for _, specifierString := range specifierStringList {
		specifier, err := specifiers.New(specifierString)
		if err != nil {
			return nil, fmt.Errorf("failed to parse specifier: %w", err)
		}

		archivePath, err := downloadSpecifier(ctx, specifier)
		if err != nil {
			return nil, fmt.Errorf("failed to install specifier: %w", err)
		}

		archive, err := teeth.NewArchive(archivePath)
		if err != nil {
			return nil, fmt.Errorf("failed to create archive: %w", err)
		}

		archiveList = append(archiveList, archive)
	}

	return archiveList, nil
}

// resolveDependencies resolves the dependencies of the tooth specified by the
// specifier and returns the paths to the downloaded teeth. rootArchiveList
// contains the root tooth archives to resolve dependencies.
// The first return value indicates whether the dependencies are resolved.
func resolveDependencies(ctx contexts.Context, rootArchiveList []teeth.Archive,
	upgradeFlag bool, forceReinstallFlag bool) ([]teeth.Archive, error) {

	var err error

	// fixedToothVersionMap records versions of teeth that are already installed
	// or will be installed.
	fixedToothVersionMap := make(map[string]versions.Version)

	installedToothMetadataList, err := teeth.GetAllInstalledToothMetadata(ctx)
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
			if !forceReinstallFlag && !(upgradeFlag && versions.GreaterThan(rootArchive.Metadata().Version(),
				fixedToothVersionMap[rootArchive.Metadata().Tooth()])) {
				continue
			}
		}
	}

	notResolvedArchiveQueue := list.New()
	for _, rootArchive := range rootArchiveList {
		notResolvedArchiveQueue.PushBack(rootArchive)
	}

	resolvedArchiveList := make([]teeth.Archive, 0)

	for notResolvedArchiveQueue.Len() > 0 {
		archive := notResolvedArchiveQueue.Front().Value.(teeth.Archive)
		notResolvedArchiveQueue.Remove(notResolvedArchiveQueue.Front())

		depMap := archive.Metadata().Dependencies()

		for dep, match := range depMap {
			if _, ok := fixedToothVersionMap[dep]; ok {
				if !match.Match(fixedToothVersionMap[dep]) {
					return nil, fmt.Errorf("installed tooth %v does not match dependency %v",
						dep, match.String())
				}

				// Avoid downloading the same tooth multiple times.
				continue
			}

			versionList, err := teeth.GetToothAvailableVersionList(ctx, dep)
			if err != nil {
				return nil, fmt.Errorf("failed to get available version list: %w", err)
			}

			var targetVersion versions.Version
			isTargetVersionFound := false

			// First find stable versions
			for _, version := range versionList {
				if version.IsStable() && match.Match(version) {
					targetVersion = version
					isTargetVersionFound = true
					break
				}
			}

			// If no stable version is found, find any version
			if !isTargetVersionFound {
				for _, version := range versionList {
					if match.Match(version) {
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

			currentArchive, err := teeth.NewArchive(archivePath)
			if err != nil {
				return nil, fmt.Errorf("failed to create archive: %w", err)
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
func validateArchive(archive teeth.Archive, toothRepo string, version versions.Version) error {
	if archive.Metadata().Tooth() != toothRepo {
		return fmt.Errorf("tooth name mismatch: %v != %v", archive.Metadata().Tooth(), toothRepo)
	}

	if !versions.Equal(archive.Metadata().Version(), version) {
		return fmt.Errorf("tooth version mismatch: %v != %v", archive.Metadata().Version(), version)
	}

	return nil
}
