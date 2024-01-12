package cmdlipcache

import (
	"flag"
	"fmt"

	"github.com/lippkg/lip/internal/cmd/cmdlipcachepurge"
	"github.com/lippkg/lip/internal/context"
)

type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip cache [options]
  lip cache <command> [subcommand options] ...

Commands:
  purge                       Clear the cache.

Options:
  -h, --help                  Show help.
`

func Run(ctx *context.Context, args []string) error {
	flagSet := flag.NewFlagSet("cache", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")

	if err := flagSet.Parse(args); err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		fmt.Print(helpMessage)
		return nil
	}

	// If there is a subcommand, run it and exit.
	if flagSet.NArg() >= 1 {
		switch flagSet.Arg(0) {
		case "purge":
			if err := cmdlipcachepurge.Run(ctx, flagSet.Args()[1:]); err != nil {
				return err
			}
			return nil

		default:
			return fmt.Errorf("unknown command: lip cache %v", flagSet.Arg(0))
		}
	}

	return fmt.Errorf(
		"no command specified. See 'lip cache --help' for more information")
}
