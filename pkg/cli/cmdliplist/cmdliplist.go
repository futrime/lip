package cmdliplist

import (
	"encoding/json"
	"flag"
	"fmt"
	"strings"

	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/installing"
	"github.com/lippkg/lip/pkg/logging"
	"github.com/lippkg/lip/pkg/teeth"
	"github.com/lippkg/lip/pkg/versions"
	"github.com/olekukonko/tablewriter"
)

type FlagDict struct {
	helpFlag       bool
	upgradableFlag bool
	jsonFlag       bool
}

const helpMessage = `
Usage:
  lip list [options]

Description:
  List installed tooths.

Options:
  -h, --help                  Show help.
  --upgradable                List upgradable tooths.
  --json                      Output in JSON format.
`

func Run(ctx contexts.Context, args []string) error {
	var err error

	flagSet := flag.NewFlagSet("list", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.upgradableFlag, "upgradable", false, "")
	flagSet.BoolVar(&flagDict.jsonFlag, "json", false, "")
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

	if flagDict.upgradableFlag {
		err = listUpgradable(ctx, flagDict.jsonFlag)
		if err != nil {
			return fmt.Errorf("failed to list upgradable tooths: %w", err)
		}

		return nil

	} else {
		err = listAll(ctx, flagDict.jsonFlag)
		if err != nil {
			return fmt.Errorf("failed to list all tooths: %w", err)
		}

		return nil
	}
}

// ---------------------------------------------------------------------

// listAll lists all installed tooths.
func listAll(ctx contexts.Context, jsonFlag bool) error {
	var err error

	metadataList, err := teeth.ListAllInstalledToothMetadata(ctx)
	if err != nil {
		return fmt.Errorf("failed to list all installed tooths: %w", err)
	}

	if jsonFlag {
		dataList := make([]teeth.RawMetadata, 0)

		for _, metadata := range metadataList {
			dataList = append(dataList, metadata.Raw())
		}

		// Marshal the data.
		jsonBytes, err := json.Marshal(dataList)
		if err != nil {
			return fmt.Errorf("failed to marshal JSON: %w", err)
		}

		jsonString := string(jsonBytes)
		logging.Info(jsonString)
	} else {
		tableData := make([][]string, 0)
		for _, metadata := range metadataList {
			tableData = append(tableData, []string{
				metadata.Tooth(),
				metadata.Info().Name,
				metadata.Version().String(),
				metadata.Info().Author,
				metadata.Info().Description,
			})
		}

		tableString := &strings.Builder{}
		table := tablewriter.NewWriter(tableString)
		table.SetHeader([]string{
			"Tooth", "Name", "Version", "Author", "Description",
		})

		for _, row := range tableData {
			table.Append(row)
		}

		table.Render()

		logging.Info(tableString.String())
	}

	return nil
}

// listUpgradable lists upgradable tooths.
func listUpgradable(ctx contexts.Context, jsonFlag bool) error {
	var err error

	metadataList, err := teeth.ListAllInstalledToothMetadata(ctx)
	if err != nil {
		return fmt.Errorf("failed to list all installed tooths: %w", err)
	}

	if jsonFlag {
		dataList := make([]teeth.RawMetadata, 0)

		for _, metadata := range metadataList {
			currentVersion := metadata.Version()
			latestVersion, err := installing.LookUpVersion(ctx,
				metadata.Tooth())
			if err != nil {
				return fmt.Errorf(
					"failed to look up latest version: %w", err)
			}

			if versions.GreaterThan(latestVersion, currentVersion) {
				dataList = append(dataList, metadata.Raw())
			}
		}

		// Marshal the data.
		jsonBytes, err := json.Marshal(dataList)
		if err != nil {
			return fmt.Errorf("failed to marshal JSON: %w", err)
		}

		jsonString := string(jsonBytes)
		logging.Info(jsonString)
	} else {
		tableData := make([][]string, 0)
		for _, metadata := range metadataList {
			currentVersion := metadata.Version()
			latestVersion, err := installing.LookUpVersion(ctx,
				metadata.Tooth())
			if err != nil {
				return fmt.Errorf(
					"failed to look up latest version: %w", err)
			}

			if versions.GreaterThan(latestVersion, currentVersion) {
				tableData = append(tableData, []string{
					metadata.Tooth(),
					metadata.Info().Name,
					metadata.Version().String(),
					metadata.Info().Author,
					metadata.Info().Description,
				})
			}
		}

		tableString := &strings.Builder{}
		table := tablewriter.NewWriter(tableString)
		table.SetHeader([]string{
			"Tooth", "Name", "Version", "Author", "Description",
		})

		for _, row := range tableData {
			table.Append(row)
		}

		table.Render()

		logging.Info(tableString.String())
	}

	return nil
}
