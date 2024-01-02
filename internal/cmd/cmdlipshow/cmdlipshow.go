package cmdlipshow

import (
	"encoding/json"
	"flag"
	"fmt"
	"strings"

	"github.com/lippkg/lip/internal/contexts"

	"github.com/lippkg/lip/internal/teeth"
	"github.com/olekukonko/tablewriter"
)

type FlagDict struct {
	helpFlag      bool
	availableFlag bool
	jsonFlag      bool
}

const helpMessage = `
Usage:
  lip show [options] <tooth repository URL>

Description:
  Show information about an installed tooth.

Options:
  -h, --help                  Show help.
  --available                 Show the full list of available versions.
  --json                      Output in JSON format.
`

func Run(ctx contexts.Context, args []string) error {
	var err error

	flagSet := flag.NewFlagSet("show", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.availableFlag, "available", false, "")
	flagSet.BoolVar(&flagDict.jsonFlag, "json", false, "")
	err = flagSet.Parse(args)
	if err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		fmt.Print(helpMessage)
		return nil
	}

	// Exactly one argument is required.
	if flagSet.NArg() != 1 {
		return fmt.Errorf("invalid number of arguments")
	}

	toothRepo := flagSet.Arg(0)

	// To lower case.
	toothRepo = strings.ToLower(toothRepo)

	if flagDict.jsonFlag {
		// When not installed, show the available versions.
		err = showJSON(ctx, toothRepo, flagDict.availableFlag)
		if err != nil {
			return fmt.Errorf("failed to show JSON: %w", err)
		}
	} else {
		err = showHumanReadable(ctx, toothRepo, flagDict.availableFlag)
		if err != nil {
			return fmt.Errorf("failed to show human-readable: %w", err)
		}
	}

	return nil
}

// ---------------------------------------------------------------------

// checkIsInstalledAndGetMetadata checks if the tooth is installed and returns
// its metadata.
func checkIsInstalledAndGetMetadata(ctx contexts.Context,
	toothRepo string) (bool, teeth.Metadata, error) {

	isInstalled, err := teeth.CheckIsToothInstalled(ctx, toothRepo)
	if err != nil {
		return false, teeth.Metadata{},
			fmt.Errorf("failed to check if tooth is installed: %w", err)
	}

	var metadata teeth.Metadata
	if isInstalled {
		metadata, err = teeth.GetInstalledToothMetadata(ctx, toothRepo)
		if err != nil {
			return false, teeth.Metadata{},
				fmt.Errorf("failed to find installed tooth metadata: %w", err)
		}
	}

	return isInstalled, metadata, nil
}

// showHumanReadable shows the information in a human-readable format.
func showHumanReadable(ctx contexts.Context, toothRepo string,
	availableFlag bool) error {
	var err error

	isInstalled, metadata, err := checkIsInstalledAndGetMetadata(ctx, toothRepo)
	if err != nil {
		return err
	}

	if !isInstalled {
		return fmt.Errorf("tooth not installed")
	}

	tableData := [][]string{
		{"Tooth Repo", metadata.Tooth()},
		{"Name", metadata.Info().Name},
		{"Description", metadata.Info().Description},
		{"Author", metadata.Info().Author},
		{"Source", metadata.Info().Source},
		{"Tags", strings.Join(metadata.Info().Tags, ", ")},
		{"Version", metadata.Version().String()},
	}

	if availableFlag {
		versionList, err := teeth.GetToothAvailableVersionList(ctx, toothRepo)
		if err != nil {
			return fmt.Errorf("failed to get tooth version list: %w", err)
		}

		availableVersions := make([]string, 0)
		for _, v := range versionList {
			availableVersions = append(availableVersions, v.String())
		}

		tableData = append(tableData, []string{"Available Versions",
			strings.Join(availableVersions, ", ")})
	}

	tableString := &strings.Builder{}
	table := tablewriter.NewWriter(tableString)
	table.SetHeader([]string{"Key", "Value"})

	for _, v := range tableData {
		table.Append(v)
	}

	table.Render()

	fmt.Print(tableString.String())

	return nil
}

// showJSON shows the information in JSON format.
func showJSON(ctx contexts.Context, toothRepo string,
	availableFlag bool) error {
	var err error

	isInstalled, metadata, err := checkIsInstalledAndGetMetadata(ctx, toothRepo)
	if err != nil {
		return err
	}

	jsonData := make(map[string]interface{})

	if isInstalled {
		jsonData["metadata"] = metadata.Raw()
	}

	if availableFlag {
		versionList, err := teeth.GetToothAvailableVersionList(ctx, toothRepo)
		if err != nil {
			return fmt.Errorf("failed to get tooth version list: %w", err)
		}

		availableVersions := make([]string, 0)
		for _, v := range versionList {
			availableVersions = append(availableVersions, v.String())
		}

		jsonData["available_versions"] = availableVersions
	}

	jsonBytes, err := json.Marshal(jsonData)
	if err != nil {
		return fmt.Errorf("failed to marshal JSON: %w", err)
	}

	fmt.Print(string(jsonBytes))

	return nil
}
