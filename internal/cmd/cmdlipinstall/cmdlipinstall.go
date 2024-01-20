package cmdlipinstall

import (
	"flag"
	"fmt"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/specifier"

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
`

func Run(ctx *context.Context, args []string) error {
	debugLogger := log.WithFields(log.Fields{
		"package": "cmdlipinstall",
		"method":  "Run",
	})

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

	if err := flagSet.Parse(args); err != nil {
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

	// Parse specifiers.

	specifiers := make([]specifier.Specifier, 0)
	for _, specifierString := range flagSet.Args() {
		specifier, err := specifier.Parse(specifierString)
		if err != nil {
			return fmt.Errorf("failed to parse specifier: %w", err)
		}

		specifiers = append(specifiers, specifier)
	}

	debugLogger.Debug("Got specifiers from arguments:")
	for _, specifier := range specifiers {
		debugLogger.Debugf("  %v", specifier)
	}

	// Download remote tooth archives. Then open all specified tooth archives.

	specifiedArchives, err := resolveSpecifiers(ctx, specifiers)
	if err != nil {
		return fmt.Errorf("failed to parse and download specifier string list: %w", err)
	}

	debugLogger.Debug("Got tooth archives from specifiers:")
	for _, archive := range specifiedArchives {
		debugLogger.Debugf("  %v@%v: %v", archive.Metadata().ToothRepoPath(), archive.Metadata().Version(), archive.FilePath().LocalString())
	}

	// Resolve dependencies and check prerequisites.

	archivesToInstall := specifiedArchives
	if !flagDict.noDependenciesFlag {
		archives, err := resolveDependencies(ctx, specifiedArchives, flagDict.upgradeFlag,
			flagDict.forceReinstallFlag)
		if err != nil {
			return fmt.Errorf("failed to resolve dependencies: %w", err)
		}

		archivesToInstall = archives

		debugLogger.Debug("After resolving dependencies, got tooth archives to install:")
		for _, archive := range archivesToInstall {
			debugLogger.Debugf("  %v@%v: %v", archive.Metadata().ToothRepoPath(), archive.Metadata().Version(), archive.FilePath().LocalString())
		}

		_, missingPrerequisites, err := getMissingPrerequisites(ctx, archivesToInstall)
		if err != nil {
			return fmt.Errorf("failed to find missing prerequisites: %w", err)
		}

		if len(missingPrerequisites) != 0 {
			message := "Missing prerequisites:\n"
			for prerequisite, versionRangeString := range missingPrerequisites {
				message += fmt.Sprintf("  %v: %v\n", prerequisite, versionRangeString)
			}
			return fmt.Errorf(message)
		}
	}

	// Filter installed teeth.

	filteredArchives, err := filterInstalledToothArchives(ctx, archivesToInstall, flagDict.upgradeFlag,
		flagDict.forceReinstallFlag)
	if err != nil {
		return fmt.Errorf("failed to filter installed teeth: %w", err)
	}

	debugLogger.Debug("After filtering installed teeth, got archives to install:")
	for _, archive := range filteredArchives {
		debugLogger.Debugf("  %v@%v: %v", archive.Metadata().ToothRepoPath(), archive.Metadata().Version(), archive.FilePath().LocalString())
	}

	// Download tooth assets if necessary.

	for _, archive := range filteredArchives {
		if err := downloadToothAssetArchiveIfNotCached(ctx, archive); err != nil {
			return fmt.Errorf("failed to download tooth assets: %w", err)
		}
	}

	// Ask for confirmation.

	if !flagDict.yesFlag {
		err := askForConfirmation(ctx, filteredArchives)
		if err != nil {
			return err
		}
	}

	// Install teeth.

	log.Info("Installing teeth...")

	for _, archive := range filteredArchives {
		if err := installToothArchive(ctx, archive, flagDict.forceReinstallFlag, flagDict.upgradeFlag); err != nil {
			return fmt.Errorf("failed to install tooth archive %v: %w", archive.FilePath().LocalString(), err)
		}
	}

	log.Info("Done.")

	return nil
}

// askForConfirmation asks for confirmation before installing the tooth.
func askForConfirmation(ctx *context.Context,
	archiveList []tooth.Archive) error {

	// Print the list of teeth to be installed.
	log.Info("The following teeth will be installed:")
	for _, archive := range archiveList {
		log.Infof("  %v@%v: %v", archive.Metadata().ToothRepoPath(), archive.Metadata().Version(),
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
