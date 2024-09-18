package cmdlipinstall

import (
	"fmt"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/specifier"
	"github.com/urfave/cli/v2"

	"github.com/lippkg/lip/internal/tooth"
	log "github.com/sirupsen/logrus"
)

const descriptionText = `
Install teeth from:

- tooth repositories. (e.g. "github.com/tooth-hub/llbds3@3.1.0")
- local tooth archives. (e.g. "./foo.tth")
`

func Command(ctx *context.Context) *cli.Command {
	return &cli.Command{
		Name:        "install",
		Usage:       "install a tooth",
		Description: descriptionText,
		ArgsUsage:   " <specifier> [...]",
		Flags: []cli.Flag{
			&cli.BoolFlag{
				Name:               "yes",
				Aliases:            []string{"y"},
				Usage:              "skip confirmation",
				DisableDefaultText: true,
			},
			&cli.BoolFlag{
				Name:               "upgrade",
				Usage:              "upgrade the specified tooth to the newest available version",
				DisableDefaultText: true,
			},
			&cli.BoolFlag{
				Name:               "force-reinstall",
				Usage:              "reinstall the tooth even if they are already up-to-date",
				DisableDefaultText: true,
			},
			&cli.BoolFlag{
				Name:               "no-dependencies",
				Usage:              "do not install dependencies. Also bypass prerequisite checks",
				DisableDefaultText: true,
			},
		},
		Action: func(cCtx *cli.Context) error {
			debugLogger := log.WithFields(log.Fields{
				"package": "cmdlipinstall",
				"method":  "Run",
			})

			// At least one specifier is required.
			if cCtx.NArg() == 0 {
				return fmt.Errorf("at least one specifier is required")
			}

			log.Info("Downloading teeth and resolving dependencies...")

			// Parse specifiers.

			specifiers := make([]specifier.Specifier, 0)
			for _, specifierString := range cCtx.Args().Slice() {
				specifier, err := specifier.Parse(specifierString)
				if err != nil {
					return fmt.Errorf("failed to parse specifier\n\t%w", err)
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
				return fmt.Errorf("failed to parse and download specifier string list\n\t%w", err)
			}

			debugLogger.Debug("Got tooth archives from specifiers:")
			for _, archive := range specifiedArchives {
				debugLogger.Debugf("  %v@%v: %v", archive.Metadata().ToothRepoPath(), archive.Metadata().Version(), archive.FilePath().LocalString())
			}

			// Resolve dependencies and check prerequisites.

			archivesToInstall := specifiedArchives
			if !cCtx.Bool("no-dependencies") {
				archives, err := resolveDependencies(ctx, specifiedArchives, cCtx.Bool("upgrade"),
					cCtx.Bool("force-reinstall"))
				if err != nil {
					return fmt.Errorf("failed to resolve dependencies\n\t%w", err)
				}

				archivesToInstall = archives

				debugLogger.Debug("After resolving dependencies, got tooth archives to install:")
				for _, archive := range archivesToInstall {
					debugLogger.Debugf("  %v@%v: %v", archive.Metadata().ToothRepoPath(), archive.Metadata().Version(), archive.FilePath().LocalString())
				}

				_, missingPrerequisites, err := getMissingPrerequisites(ctx, archivesToInstall)
				if err != nil {
					return fmt.Errorf("failed to find missing prerequisites\n\t%w", err)
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

			filteredArchives, err := filterInstalledToothArchives(ctx, archivesToInstall, cCtx.Bool("upgrade"),
				cCtx.Bool("force-reinstall"))
			if err != nil {
				return fmt.Errorf("failed to filter installed teeth\n\t%w", err)
			}

			debugLogger.Debug("After filtering installed teeth, got archives to install:")
			for _, archive := range filteredArchives {
				debugLogger.Debugf("  %v@%v: %v", archive.Metadata().ToothRepoPath(), archive.Metadata().Version(), archive.FilePath().LocalString())
			}

			// Download tooth assets if necessary.

			for _, archive := range filteredArchives {
				if err := downloadToothAssetArchiveIfNotCached(ctx, archive); err != nil {
					return fmt.Errorf("failed to download tooth assets\n\t%w", err)
				}
			}

			// Ask for confirmation.

			if !cCtx.Bool("yes") {
				err := askForConfirmation(ctx, filteredArchives)
				if err != nil {
					return err
				}
			}

			// Install teeth.

			log.Info("Installing teeth...")

			for _, archive := range filteredArchives {
				if err := installToothArchive(ctx, archive, cCtx.Bool("force-reinstall"), cCtx.Bool("upgrade"), cCtx.Bool("yes")); err != nil {
					return fmt.Errorf("failed to install tooth archive %v\n\t%w", archive.FilePath().LocalString(), err)
				}
			}

			log.Info("Done.")

			return nil
		},
	}
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
