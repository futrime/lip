package cmdlipcache

import (
	"fmt"

	"github.com/lippkg/lip/internal/cmd/cmdlipcachepurge"
	"github.com/lippkg/lip/internal/context"

	"github.com/urfave/cli/v2"
)

func Command(ctx *context.Context) *cli.Command {
	return &cli.Command{
		Name:  "cache",
		Usage: "inspect and manage lip's cache",
		Subcommands: []*cli.Command{
			cmdlipcachepurge.Command(ctx),
		},
		Action: func(cCtx *cli.Context) error {
			if cCtx.NArg() >= 1 {
				return fmt.Errorf("unknown command: lip %v %v", cCtx.Command.Name, cCtx.Args().First())
			}
			return fmt.Errorf(
				"no command specified. See 'lip %v --help' for more information", cCtx.Command.Name)
		},
	}
}
