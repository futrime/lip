package cmdlipfreeze

import (
	"fmt"
	"os"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/path"
	log "github.com/sirupsen/logrus"
	"github.com/urfave/cli/v2"

	"github.com/lippkg/lip/internal/tooth"
)

func Command(ctx *context.Context) *cli.Command {
	return &cli.Command{
		Name:        "freeze",
		Usage:       "generate specifiers.txt",
		Description: "Generate a specifiers.txt that can be used for batch installation.",
		ArgsUsage:   "[output path]",
		Action: func(cCtx *cli.Context) error {

			var outputPath = path.MustParse("./specifiers.txt")

			if cCtx.NArg() >= 1 {
				return fmt.Errorf("expected zero or one argument")
			}

			if cCtx.NArg() == 1 {
				if input, err := path.Parse(cCtx.Args().Get(0)); err == nil {
					outputPath = input
				} else {
					return fmt.Errorf("failed to perse the path to specifiers.txt\n\t%w", err)
				}
			}

			var specifiers string

			metadataList, err := tooth.GetAllMetadata(ctx)
			if err != nil {
				return fmt.Errorf("failed to get all installed teeth\n\t%w", err)
			}
			for _, i := range metadataList {
				specifiers += fmt.Sprintf("%v@%v\n", i.ToothRepoPath(), i.Version().String())
			}

			if err := os.WriteFile(outputPath.LocalString(), []byte(specifiers), 0666); err != nil {
				return fmt.Errorf("failed to create specifiers.txt\n\t%w", err)
			}

			log.Info("specifiers.txt generated successful.")
			return nil
		},
	}
}
