package cmdliplist

import (
	"encoding/json"
	"fmt"
	"strings"

	"github.com/lippkg/lip/internal/context"
	log "github.com/sirupsen/logrus"
	"github.com/urfave/cli/v2"

	"github.com/lippkg/lip/internal/tooth"
	"github.com/olekukonko/tablewriter"
)

func Command(ctx *context.Context) *cli.Command {
	return &cli.Command{
		Name:        "list",
		Usage:       "list installed teeth",
		Description: "List installed teeth.",
		Flags: []cli.Flag{
			&cli.BoolFlag{
				Name:               "json",
				Usage:              "output in JSON format",
				DisableDefaultText: true,
			},
			&cli.BoolFlag{
				Name:               "upgradable",
				Usage:              "list upgradable teeth",
				DisableDefaultText: true,
			},
		},
		Action: func(cCtx *cli.Context) error {
			// Check if there are unexpected arguments.
			if cCtx.NArg() != 0 {
				return fmt.Errorf("unexpected arguments: %v", cCtx.Args())
			}

			if cCtx.Bool("upgradable") {
				err := listUpgradable(ctx, cCtx.Bool("json"))
				if err != nil {
					return fmt.Errorf("failed to list upgradable teeth\n\t%w", err)
				}

				return nil
			} else {
				err := listAll(ctx, cCtx.Bool("json"))
				if err != nil {
					return fmt.Errorf("failed to list all teeth\n\t%w", err)
				}

				return nil
			}
		},
	}
}

// ---------------------------------------------------------------------

// listAll lists all installed teeth.
func listAll(ctx *context.Context, jsonFlag bool) error {

	metadataList, err := tooth.GetAllMetadata(ctx)
	if err != nil {
		return fmt.Errorf("failed to list all installed teeth\n\t%w", err)
	}

	if jsonFlag {
		// Marshal the data.
		jsonBytes, err := json.Marshal(metadataList)
		if err != nil {
			return fmt.Errorf("failed to marshal JSON\n\t%w", err)
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
func listUpgradable(ctx *context.Context, jsonFlag bool) error {

	metadataList, err := tooth.GetAllMetadata(ctx)
	if err != nil {
		return fmt.Errorf("failed to list all installed teeth\n\t%w", err)
	}

	if jsonFlag {
		dataList := make([]tooth.Metadata, 0)

		for _, metadata := range metadataList {
			currentVersion := metadata.Version()
			latestVersion, err := tooth.GetLatestVersion(ctx,
				metadata.ToothRepoPath())
			if err != nil {
				log.Errorf(
					"\n\tfailed to look up latest version for %v\n\t%v", metadata.ToothRepoPath(), err.Error())
				continue
			}

			if latestVersion.GT(currentVersion) {
				dataList = append(dataList, metadata)
			}
		}

		// Marshal the data.
		jsonBytes, err := json.Marshal(dataList)
		if err != nil {
			return fmt.Errorf("failed to marshal JSON\n\t%w", err)
		}

		jsonString := string(jsonBytes)
		fmt.Print(jsonString)

	} else {
		tableData := make([][]string, 0)
		for _, metadata := range metadataList {
			currentVersion := metadata.Version()
			latestVersion, err := tooth.GetLatestVersion(ctx,
				metadata.ToothRepoPath())
			if err != nil {
				log.Errorf(
					"\n\tfailed to look up latest version for %v\n\t%v", metadata.ToothRepoPath(), err.Error())
				continue
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

		tableStringBuilder := &strings.Builder{}
		table := tablewriter.NewWriter(tableStringBuilder)
		table.SetHeader([]string{
			"Tooth", "Name", "Version", "Latest",
		})

		for _, row := range tableData {
			table.Append(row)
		}

		table.Render()

		fmt.Print(tableStringBuilder.String())
	}

	return nil
}
