package cmdlipshow

import (
	"encoding/json"
	"fmt"
	"strings"

	"github.com/lippkg/lip/internal/context"
	"github.com/urfave/cli/v2"

	"github.com/lippkg/lip/internal/tooth"
	"github.com/olekukonko/tablewriter"
)

func Command(ctx *context.Context) *cli.Command {
	return &cli.Command{
		Name:        "show",
		Usage:       "show information about installed teeth",
		Description: "Show information about an installed tooth.",
		ArgsUsage:   "<tooth repository URL>",
		Flags: []cli.Flag{
			&cli.BoolFlag{
				Name:               "available",
				Usage:              "show the full list of available versions",
				DisableDefaultText: true,
			},
			&cli.BoolFlag{
				Name:               "json",
				Usage:              "output in JSON format",
				DisableDefaultText: true,
			},
		},
		Action: func(cCtx *cli.Context) error {
			if cCtx.NArg() != 1 {
				return fmt.Errorf("invalid number of arguments")
			}

			toothRepoPath := cCtx.Args().Get(0)

			if err := show(ctx, toothRepoPath, cCtx.Bool("available"), cCtx.Bool("json")); err != nil {
				return fmt.Errorf("failed to show JSON\n\t%w", err)
			}

			return nil
		},
	}
}

// checkIsInstalledAndGetMetadata checks if the tooth is installed and returns
// its metadata.
func checkIsInstalledAndGetMetadata(ctx *context.Context,
	toothRepoPath string) (bool, tooth.Metadata, error) {

	isInstalled, err := tooth.IsInstalled(ctx, toothRepoPath)
	if err != nil {
		return false, tooth.Metadata{},
			fmt.Errorf("failed to check if tooth is installed\n\t%w", err)
	}

	if isInstalled {
		metadata, err := tooth.GetMetadata(ctx, toothRepoPath)
		if err != nil {
			return false, tooth.Metadata{},
				fmt.Errorf("failed to find installed tooth metadata\n\t%w", err)
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
			return fmt.Errorf("failed to get tooth version list\n\t%w", err)
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
			return fmt.Errorf("failed to marshal JSON\n\t%w", err)
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
