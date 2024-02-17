package cmdlipuninstall

import (
	"flag"
	"fmt"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/install"
	log "github.com/sirupsen/logrus"

	"github.com/lippkg/lip/internal/tooth"
)

type FlagDict struct {
	helpFlag bool
	yesFlag  bool
}

const helpMessage = `
Usage:
  lip uninstall [options] <tooth repository URL> [...]

Description:
  Uninstall teeth.

Options:
  -h, --help                  Show help.
  -y, --yes                   Skip confirmation.
`

func Run(ctx *context.Context, args []string) error {

	flagSet := flag.NewFlagSet("uninstall", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.yesFlag, "yes", false, "")
	flagSet.BoolVar(&flagDict.yesFlag, "y", false, "")
	err := flagSet.Parse(args)
	if err != nil {
		return fmt.Errorf("failed to parse flags\n\t%w", err)
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

	toothRepoPathList := flagSet.Args()

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

	if !flagDict.yesFlag {
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
