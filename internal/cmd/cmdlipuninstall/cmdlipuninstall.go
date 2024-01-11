package cmdlipuninstall

import (
	"flag"
	"fmt"
	"strings"

	"github.com/lippkg/lip/internal/contexts"
	"github.com/lippkg/lip/internal/installing"
	log "github.com/sirupsen/logrus"

	"github.com/lippkg/lip/internal/teeth"
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

func Run(ctx contexts.Context, args []string) error {
	var err error

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

	toothRepoList := flagSet.Args()

	// To lower case.
	for i, toothRepo := range toothRepoList {
		toothRepoList[i] = strings.ToLower(toothRepo)
	}

	// 1. Check if all teeth are installed.

	for _, toothRepo := range toothRepoList {

		isInstalled, err := teeth.CheckIsToothInstalled(ctx, toothRepo)
		if err != nil {
			return fmt.Errorf("failed to check if tooth is installed: %w", err)
		}

		if !isInstalled {
			return fmt.Errorf("tooth %v is not installed", toothRepo)
		}
	}

	// 2. Prompt for confirmation.

	if !flagDict.yesFlag {
		err = askForConfirmation(ctx, toothRepoList)
		if err != nil {
			return err
		}
	}

	// 3. Uninstall all teeth.

	for _, toothRepo := range toothRepoList {
		err = installing.Uninstall(ctx, toothRepo)
		if err != nil {
			return fmt.Errorf("failed to uninstall tooth %v: %w", toothRepo, err)
		}
	}

	log.Info("Done.")

	return nil
}

// ---------------------------------------------------------------------

// askForConfirmation asks for confirmation before installing the tooth.
func askForConfirmation(ctx contexts.Context,
	toothRepoList []string) error {

	// Print the list of teeth to be installed.
	log.Info("The following teeth will be uninstalled:")
	for _, toothRepo := range toothRepoList {
		metadata, err := teeth.GetInstalledToothMetadata(ctx, toothRepo)
		if err != nil {
			return fmt.Errorf("failed to get installed tooth metadata: %w", err)
		}

		log.Infof("  %v: %v", toothRepo,
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
