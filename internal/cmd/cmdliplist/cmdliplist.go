package cmdliplist

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
	helpFlag       bool
	upgradableFlag bool
	jsonFlag       bool
}

const helpMessage = `
Usage:
  lip list [options]

Description:
  List installed teeth.

Options:
  -h, --help                  Show help.
  --upgradable                List upgradable teeth.
  --json                      Output in JSON format.
`

func Run(ctx context.Context, args []string) error {

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
	err := flagSet.Parse(args)
	if err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		fmt.Print(helpMessage)
		return nil
	}

	// Check if there are unexpected arguments.
	if flagSet.NArg() != 0 {
		return fmt.Errorf("unexpected arguments: %v", flagSet.Args())
	}

	if flagDict.upgradableFlag {
		err := listUpgradable(ctx, flagDict.jsonFlag)
		if err != nil {
			return fmt.Errorf("failed to list upgradable teeth: %w", err)
		}

		return nil

	} else {
		err := listAll(ctx, flagDict.jsonFlag)
		if err != nil {
			return fmt.Errorf("failed to list all teeth: %w", err)
		}

		return nil
	}
}

// ---------------------------------------------------------------------

// listAll lists all installed teeth.
func listAll(ctx context.Context, jsonFlag bool) error {

	metadataList, err := tooth.GetAllMetadata(ctx)
	if err != nil {
		return fmt.Errorf("failed to list all installed teeth: %w", err)
	}

	if jsonFlag {
		// Marshal the data.
		jsonBytes, err := json.Marshal(metadataList)
		if err != nil {
			return fmt.Errorf("failed to marshal JSON: %w", err)
		}

		jsonString := string(jsonBytes)
		fmt.Print(jsonString)
	} else {
		tableData := make([][]string, 0)
		for _, metadata := range metadataList {
			tableData = append(tableData, []string{
				metadata.ToothRepoPath(),
				metadata.Info().Name,
				metadata.Version().String(),
			})
		}

		tableString := &strings.Builder{}
		table := tablewriter.NewWriter(tableString)
		table.SetHeader([]string{
			"Tooth", "Name", "Version",
		})

		for _, row := range tableData {
			table.Append(row)
		}

		table.Render()

		fmt.Print(tableString.String())
	}

	return nil
}

// listUpgradable lists upgradable teeth.
func listUpgradable(ctx context.Context, jsonFlag bool) error {

	metadataList, err := tooth.GetAllMetadata(ctx)
	if err != nil {
		return fmt.Errorf("failed to list all installed teeth: %w", err)
	}

	if jsonFlag {
		dataList := make([]tooth.Metadata, 0)

		for _, metadata := range metadataList {
			currentVersion := metadata.Version()
			latestVersion, err := tooth.GetLatestStableVersion(ctx,
				metadata.ToothRepoPath())
			if err != nil {
				return fmt.Errorf(
					"failed to look up latest version: %w", err)
			}

			if latestVersion.GT(currentVersion) {
				dataList = append(dataList, metadata)
			}
		}

		// Marshal the data.
		jsonBytes, err := json.Marshal(dataList)
		if err != nil {
			return fmt.Errorf("failed to marshal JSON: %w", err)
		}

		jsonString := string(jsonBytes)
		fmt.Print(jsonString)
	} else {
		tableData := make([][]string, 0)
		for _, metadata := range metadataList {
			currentVersion := metadata.Version()
			latestVersion, err := tooth.GetLatestStableVersion(ctx,
				metadata.ToothRepoPath())
			if err != nil {
				return fmt.Errorf(
					"failed to look up latest version: %w", err)
			}

			if latestVersion.GT(currentVersion) {
				tableData = append(tableData, []string{
					metadata.ToothRepoPath(),
					metadata.Info().Name,
					metadata.Version().String(),
					latestVersion.String(),
				})
			}
		}

		tableString := &strings.Builder{}
		table := tablewriter.NewWriter(tableString)
		table.SetHeader([]string{
			"Tooth", "Name", "Version", "Latest",
		})

		for _, row := range tableData {
			table.Append(row)
		}

		table.Render()

		fmt.Print(tableString.String())
	}

	return nil
}
