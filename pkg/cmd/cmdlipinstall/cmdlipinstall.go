package cmdlipinstall

import (
	"flag"
	"fmt"
	"os"

	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/downloading"
	"github.com/lippkg/lip/pkg/installing"
	"github.com/lippkg/lip/pkg/logging"
	"github.com/lippkg/lip/pkg/specifiers"
	"github.com/lippkg/lip/pkg/teeth"
	"github.com/lippkg/lip/pkg/versionmatches"
	"github.com/lippkg/lip/pkg/versions"
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
  Install tooths from:

  - tooth repositories. (e.g. "github.com/tooth-hub/llbds3@3.1.0")
  - local tooth archives. (e.g. "./foo.tth")

Options:
  -h, --help                  Show help.
  --upgrade                   Upgrade the specified tooth to the newest available version.
  --force-reinstall           Reinstall the tooth even if they are already up-to-date.
  -y, --yes                   Assume yes to all prompts and run non-interactively.
  --no-dependencies           Do not install dependencies.

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

	// 2. Resolve dependencies.

	if !flagDict.noDependenciesFlag {
		var success bool
		success, archiveToInstallList, err = resolveDependencies(ctx, archiveToInstallList)
		if err != nil {
			return fmt.Errorf("failed to resolve dependencies: %w", err)
		}

		if !success {
			return fmt.Errorf("dependencies cannot match the version constraints")
		}
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
func resolveDependencies(ctx contexts.Context,
	rootArchiveList []teeth.Archive) (bool, []teeth.Archive, error) {

	var err error

	depMap := make(map[string][]versionmatches.Group)

	// List all dependencies.
	for _, rootArchive := range rootArchiveList {
		for toothRepo, matchGroup := range rootArchive.Metadata().Dependencies() {
			// Create the key if it does not exist.
			if _, ok := depMap[toothRepo]; !ok {
				depMap[toothRepo] = make([]versionmatches.Group, 0)
			}

			depMap[toothRepo] = append(depMap[toothRepo], matchGroup)
		}
	}

	// Construct exact version matchers for root tooth archives.
	for _, rootArchive := range rootArchiveList {
		matchItem, err := versionmatches.NewItem(rootArchive.Metadata().Version(), versionmatches.EqualMatchType)
		if err != nil {
			return false, nil, fmt.Errorf("failed to create match item: %w", err)
		}

		matchGroup := versionmatches.NewGroup([][]versionmatches.Item{{matchItem}})

		// Create the key if it does not exist.
		if _, ok := depMap[rootArchive.Metadata().Tooth()]; !ok {
			depMap[rootArchive.Metadata().Tooth()] = make([]versionmatches.Group, 0)
		}

		depMap[rootArchive.Metadata().Tooth()] = append(depMap[rootArchive.Metadata().Tooth()], matchGroup)
	}

	// Check if all dependencies are satisfied.
	for toothRepo, matchGroupList := range depMap {
		versionList, err := teeth.GetToothAvailableVersionList(ctx, toothRepo)
		if err != nil {
			return false, nil, fmt.Errorf("failed to get tooth available version list: %w", err)
		}

		isAnyVersionMatched := false
		for _, version := range versionList {
			isCurrentVersionMatched := true
			for _, matchGroup := range matchGroupList {
				if !matchGroup.Match(version) {
					isCurrentVersionMatched = false
					break
				}
			}

			if isCurrentVersionMatched {
				isAnyVersionMatched = true
				break
			}
		}

		if !isAnyVersionMatched {
			return false, nil, nil
		}
	}

	// Try versions of the first dependency not in the root tooth archives.
	firstDepRepo := ""

	for toothRepo := range depMap {
		isInRootArchive := false
		for _, rootArchive := range rootArchiveList {
			if toothRepo == rootArchive.Metadata().Tooth() {
				isInRootArchive = true
				break
			}
		}

		if isInRootArchive {
			continue
		}

		firstDepRepo = toothRepo
		break
	}

	// If there is no dependency, return the root tooth archives.
	if firstDepRepo == "" {
		return true, rootArchiveList, nil
	}

	versionList, err := teeth.GetToothAvailableVersionList(ctx, firstDepRepo)
	if err != nil {
		return false, nil, fmt.Errorf("failed to get tooth available version list: %w", err)
	}

	for _, version := range versionList {
		filePath, err := downloadFromAllGoProxies(ctx, firstDepRepo, version)
		if err != nil {
			return false, nil, fmt.Errorf("failed to download tooth: %w", err)
		}

		archive, err := teeth.NewArchive(filePath)
		if err != nil {
			return false, nil, fmt.Errorf("failed to create archive: %w", err)
		}

		newRootArchiveList := make([]teeth.Archive, len(rootArchiveList))
		copy(newRootArchiveList, rootArchiveList)
		newRootArchiveList = append(newRootArchiveList, archive)

		isResolved, resolvedArchiveList, err := resolveDependencies(ctx, newRootArchiveList)
		if err != nil {
			return false, nil, fmt.Errorf("failed to resolve dependencies: %w", err)
		}

		if isResolved {
			return true, resolvedArchiveList, nil
		}
	}

	// If no version is matched, fail.
	return false, nil, nil
}
