package cmdlipshow

import (
	"encoding/json"
	"flag"
	"fmt"
	"strings"

	"github.com/lippkg/lip/internal/context"

	"github.com/lippkg/lip/internal/tooth"
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

func Run(ctx *context.Context, args []string) error {

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
	err := flagSet.Parse(args)
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

	toothRepoPath := flagSet.Arg(0)

	if err := show(ctx, toothRepoPath, flagDict.availableFlag, flagDict.jsonFlag); err != nil {
		return fmt.Errorf("failed to show JSON: %w", err)
	}

	return nil
}

// checkIsInstalledAndGetMetadata checks if the tooth is installed and returns
// its metadata.
func checkIsInstalledAndGetMetadata(ctx *context.Context,
	toothRepoPath string) (bool, tooth.Metadata, error) {

	isInstalled, err := tooth.IsInstalled(ctx, toothRepoPath)
	if err != nil {
		return false, tooth.Metadata{},
			fmt.Errorf("failed to check if tooth is installed: %w", err)
	}

	if isInstalled {
		metadata, err := tooth.GetMetadata(ctx, toothRepoPath)
		if err != nil {
			return false, tooth.Metadata{},
				fmt.Errorf("failed to find installed tooth metadata: %w", err)
		}

		return true, metadata, nil
	} else {
		return false, tooth.Metadata{}, nil
	}
}

func show(ctx *context.Context, toothRepoPath string,
	availableFlag bool, jsonFlag bool) error {

	isInstalled, metadata, err := checkIsInstalledAndGetMetadata(ctx, toothRepoPath)
	if err != nil {
		return err
	}

	availableVersions := make([]string, 0)
	if availableFlag {
		versionList, err := tooth.GetAvailableVersions(ctx, toothRepoPath)
		if err != nil {
			return fmt.Errorf("failed to get tooth version list: %w", err)
		}

		for _, v := range versionList {
			availableVersions = append(availableVersions, v.String())
		}
	}

	if !isInstalled && !availableFlag {
		return fmt.Errorf("tooth is not installed")
	}

	if jsonFlag {
		info := make(map[string]interface{})

		if isInstalled {
			info["metadata"] = metadata
		}

		if availableFlag {
			info["available_versions"] = availableVersions
		}

		jsonBytes, err := json.Marshal(info)
		if err != nil {
			return fmt.Errorf("failed to marshal JSON: %w", err)
		}

		fmt.Print(string(jsonBytes))

	} else {
		tableData := make([][]string, 0)

		if isInstalled {
			tableData = append(tableData, [][]string{
				{"Tooth Repo", metadata.ToothRepoPath()},
				{"Name", metadata.Info().Name},
				{"Description", metadata.Info().Description},
				{"Author", metadata.Info().Author},
				{"Tags", strings.Join(metadata.Info().Tags, ", ")},
				{"Version", metadata.Version().String()},
			}...)
		}

		if availableFlag {
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
	}

	return nil
}
