package cmdlipinstall

import (
	"flag"
	"fmt"

	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/downloading"
	"github.com/lippkg/lip/pkg/installing"
	"github.com/lippkg/lip/pkg/logging"
	"github.com/lippkg/lip/pkg/specifiers"
	"github.com/lippkg/lip/pkg/teeth"
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
		archiveToInstallList, err = resolveDependencies(ctx, archiveToInstallList)
		if err != nil {
			return fmt.Errorf("failed to resolve dependencies: %w", err)
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
	toothVersion versions.Version,
	progressbarStyle downloading.ProgressBarStyleType) (string, error) {

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

		err = downloading.DownloadFile(downloadURL, cachePath, progressbarStyle)
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
	specifier specifiers.Specifier,
	progressbarStyle downloading.ProgressBarStyleType) (string, error) {
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

		archivePath, err := downloadFromAllGoProxies(ctx, toothRepo, toothVersion,
			progressbarStyle)
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

		pbarStyle := downloading.StyleDefault
		if logging.LoggingLevel() > logging.InfoLevel {
			pbarStyle = downloading.StyleNone
		}

		archivePath, err := downloadSpecifier(ctx, specifier, pbarStyle)
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
// specifier and returns the paths to the downloaded teeth.
func resolveDependencies(ctx contexts.Context,
	archiveToInstallPathList []teeth.Archive) ([]teeth.Archive, error) {
	// TODO
	return archiveToInstallPathList, nil
}
