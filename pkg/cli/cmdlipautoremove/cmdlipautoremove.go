package cmdlipautoremove

import (
	"flag"
	"fmt"

	"github.com/lippkg/lip/pkg/installing"
	"github.com/lippkg/lip/pkg/teeth"
	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/logging"
)

type FlagDict struct {
	helpFlag bool
	yesFlag  bool
}

const helpMessage = `
Usage:
  lip autoremove [options]

Description:
  Uninstall teeth that are not depended by any other teeth.

Options:
  -h, --help                  Show help.
  -y, --yes                   Skip confirmation.
`

func Run(ctx contexts.Context, args []string) error {
	var err error

	flagSet := flag.NewFlagSet("autoremove", flag.ContinueOnError)

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
		logging.Info(helpMessage)
		return nil
	}

	// Check if there are unexpected arguments.
	if flagSet.NArg() != 0 {
		return fmt.Errorf("unexpected arguments: %v", flagSet.Args())
	}

	// Autoremove teeth.
	err = autoremove(ctx, flagDict.yesFlag)
	if err != nil {
		return fmt.Errorf("failed to autoremove teeth: %w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// autoremove uninstalls teeth that are not depended by any other teeth.
func autoremove(ctx contexts.Context, yesFlag bool) error {
	var err error

	// 1. Get all isolated teeth.
	logging.Info("Discovering isolated teeth...")

	isolatedTeeth, err := listIsolatedTeeth(ctx)
	if err != nil {
		return fmt.Errorf("failed to list isolated teeth: %w", err)
	}

	// 2. Prompt for confirmation.
	if !yesFlag {
		err = promptForConfirmation(isolatedTeeth)
		if err != nil {
			return err // User cancelled.
		}
	}

	// 3. Uninstall isolated teeth.
	logging.Info("Uninstalling isolated teeth...")
	for _, tooth := range isolatedTeeth {
		logging.Info("  %s", tooth)

		err = installing.Uninstall(ctx, tooth)
		if err != nil {
			return fmt.Errorf("failed to uninstall tooth %s: %w", tooth, err)
		}
	}

	logging.Info("All isolated teeth are uninstalled.")

	return nil
}

// listEssentialTeeth lists all teeth depended by other teeth.
func listEssentialTeeth(ctx contexts.Context) ([]string, error) {
	var err error

	// Get manually installed teeth.
	manuallyInstalledTeeth, err := listManuallyInstalledTeeth(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to list manually installed teeth: %w", err)
	}

	// Mark all dependents as essential teeth.
	essentialTeeth := make(map[string]struct{})
	for _, dependent := range manuallyInstalledTeeth {
		essentialTeeth[dependent] = struct{}{}
	}

	// Get all installed teeth.
	metadataList, err := teeth.ListAllInstalledToothMetadata(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to list all installed teeth: %w", err)
	}

	// Iteratively mark all essential teeth.
	markCount := 0
	for {
		for _, metadata := range metadataList {
			// Skip if the tooth is not essential.
			if _, ok := essentialTeeth[metadata.Tooth()]; !ok {
				continue
			}

			for dep := range metadata.Dependencies() {
				if _, ok := essentialTeeth[dep]; !ok {
					essentialTeeth[dep] = struct{}{}
					markCount++
				}
			}
		}

		// Stop if no new teeth are marked.
		if markCount == 0 {
			break
		}

		markCount = 0
	}

	// Converts the map to a list.
	var essentialTeethList []string
	for tooth := range essentialTeeth {
		essentialTeethList = append(essentialTeethList, tooth)
	}

	return essentialTeethList, nil
}

// listIsolatedTeeth lists all isolated teeth.
func listIsolatedTeeth(ctx contexts.Context) ([]string, error) {
	var err error

	// Get all installed teeth.
	metadataList, err := teeth.ListAllInstalledToothMetadata(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to list all installed teeth: %w", err)
	}

	// Get all essential teeth.
	essentialTeeth, err := listEssentialTeeth(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to list essential teeth: %w", err)
	}

	essentialTeethSet := make(map[string]struct{})
	for _, tooth := range essentialTeeth {
		essentialTeethSet[tooth] = struct{}{}
	}

	// Get the difference.
	isolatedTeeth := make([]string, 0)
	for _, metadata := range metadataList {
		if _, ok := essentialTeethSet[metadata.Tooth()]; !ok {
			isolatedTeeth = append(isolatedTeeth, metadata.Tooth())
		}
	}

	return isolatedTeeth, nil
}

// listManuallyInstalledTeeth lists all manually installed teeth.
func listManuallyInstalledTeeth(ctx contexts.Context) ([]string, error) {
	var err error

	// Gets all installed teeth.
	metadataList, err := teeth.ListAllInstalledToothMetadata(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to list all installed teeth: %w", err)
	}

	// Marks all manually installed teeth.
	var manuallyInstalledTeeth []string
	for _, metadata := range metadataList {
		isManuallyInstalled, err := installing.CheckIsToothManuallyInstalled(
			ctx, metadata.Tooth())
		if err != nil {
			return nil, fmt.Errorf(
				"failed to check if tooth is manually installed: %w", err)
		}

		if isManuallyInstalled {
			manuallyInstalledTeeth = append(manuallyInstalledTeeth, metadata.Tooth())
		}
	}

	return manuallyInstalledTeeth, nil
}

// promptForConfirmation prompts for confirmation.
func promptForConfirmation(isolatedTeeth []string) error {
	// Print isolated teeth.
	logging.Info("The following teeth will be uninstalled:")
	for _, tooth := range isolatedTeeth {
		logging.Info("  %s", tooth)
	}

	// Prompt for confirmation.
	logging.Info("Do you want to continue? [y/N]")
	var ans string
	fmt.Scanln(&ans)
	if ans != "y" && ans != "Y" {
		return fmt.Errorf("aborted")
	}

	return nil
}
