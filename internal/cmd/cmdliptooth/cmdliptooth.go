package cmdliptooth

import (
	"flag"
	"fmt"

	"github.com/lippkg/lip/internal/cmd/cmdliptoothinit"
	"github.com/lippkg/lip/internal/cmd/cmdliptoothpack"
	"github.com/lippkg/lip/internal/contexts"
	"github.com/lippkg/lip/internal/logging"
)

type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip tooth [options]
  lip tooth <command> [subcommand options] ...

Commands:
  init                        Initialize and writes a new tooth.json file in the current directory.
  pack                        Pack the current directory into a tooth file.

Options:
  -h, --help                  Show help.
`

func Run(ctx contexts.Context, args []string) error {
	var err error

	flagSet := flag.NewFlagSet("tooth", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	err = flagSet.Parse(args)
	if err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logging.Info(helpMessage)
		return nil
	}

	// If there is a subcommand, run it and exit.
	if flagSet.NArg() >= 1 {
		switch flagSet.Arg(0) {
		case "init":
			err = cmdliptoothinit.Run(ctx, flagSet.Args()[1:])
			if err != nil {
				return err
			}
			return nil

		case "pack":
			err = cmdliptoothpack.Run(ctx, flagSet.Args()[1:])
			if err != nil {
				return err
			}
			return nil

		default:
			return fmt.Errorf("unknown command: lip tooth %v", flagSet.Arg(0))
		}
	}

	return fmt.Errorf(
		"no command specified. See 'lip tooth --help' for more information")
}
