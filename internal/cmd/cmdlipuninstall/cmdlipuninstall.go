package cmdlipuninstall

import (
	"fmt"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/install"
	log "github.com/sirupsen/logrus"
	"github.com/urfave/cli/v2"

	"github.com/lippkg/lip/internal/tooth"
)

func Command(ctx *context.Context) *cli.Command {
	return &cli.Command{
		Name:      "uninstall",
		Usage:     "uninstall a tooth",
		ArgsUsage: "<tooth repository URL> [...]",
		Flags: []cli.Flag{
			&cli.BoolFlag{
				Name:               "yes",
				Aliases:            []string{"y"},
				Usage:              "skip confirmation",
				DisableDefaultText: true,
			},
		},
		Description: "Uninstall teeth.",
		Action: func(cCtx *cli.Context) error {
			// At least one specifier is required.
			if cCtx.NArg() == 0 {
				return fmt.Errorf("at least one specifier is required")
			}

			toothRepoPathList := cCtx.Args().Slice()

			// 1. Check if all teeth are installed.

			for _, toothRepoPath := range toothRepoPathList {

				isInstalled, err := tooth.IsInstalled(ctx, toothRepoPath)
				if err != nil {
					return fmt.Errorf("failed to check if tooth is installed\n\t%w", err)
				}

				if !isInstalled {
					return fmt.Errorf("tooth %v is not installed", toothRepoPath)
				}
			}

			// 2. Prompt for confirmation.

			if !cCtx.Bool("yes") {
				err := askForConfirmation(ctx, toothRepoPathList)
				if err != nil {
					return err
				}
			}

			// 3. Uninstall all teeth.

			for _, toothRepoPath := range toothRepoPathList {
				err := install.Uninstall(ctx, toothRepoPath)
				if err != nil {
					return fmt.Errorf("failed to uninstall tooth %v\n\t%w", toothRepoPath, err)
				}
			}

			log.Info("Done.")

			return nil
		},
	}
}

// ---------------------------------------------------------------------

// askForConfirmation asks for confirmation before installing the tooth.
func askForConfirmation(ctx *context.Context,
	toothRepoPathList []string) error {

	// Print the list of teeth to be installed.
	log.Info("The following teeth will be uninstalled:")
	for _, toothRepoPath := range toothRepoPathList {
		metadata, err := tooth.GetMetadata(ctx, toothRepoPath)
		if err != nil {
			return fmt.Errorf("failed to get installed tooth metadata\n\t%w", err)
		}

		log.Infof("  %v@%v: %v", toothRepoPath, metadata.Version(),
			metadata.Info().Name)
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
