package cmdlip

import (
	"flag"
	"fmt"
	"os"

	nested "github.com/antonfisher/nested-logrus-formatter"
	"github.com/lippkg/lip/internal/cmd/cmdlipcache"
	"github.com/lippkg/lip/internal/cmd/cmdlipconfig"
	"github.com/lippkg/lip/internal/cmd/cmdlipinstall"
	"github.com/lippkg/lip/internal/cmd/cmdliplist"
	"github.com/lippkg/lip/internal/cmd/cmdlipshow"
	"github.com/lippkg/lip/internal/cmd/cmdliptooth"
	"github.com/lippkg/lip/internal/cmd/cmdlipuninstall"
	"github.com/lippkg/lip/internal/context"

	log "github.com/sirupsen/logrus"
)

type FlagDict struct {
	helpFlag    bool
	versionFlag bool
	verboseFlag bool
	quietFlag   bool
	noColorFlag bool
}

const helpMessage = `
Usage:
  lip [options] [<command> [subcommand options]] ...

Commands:
  cache                       Inspect and manage lip's cache.
  config                      Manage configuration.
  install                     Install a tooth.
  list                        List installed teeth.
  show                        Show information about installed teeth.
  tooth                       Maintain a tooth.
  uninstall                   Uninstall a tooth.

Options:
  -h, --help                  Show help.
  -V, --version               Show version and exit.
  -v, --verbose               Show verbose output.
  -q, --quiet                 Show only errors.
  --no-color                  Disable color output.
`

func Run(ctx *context.Context, args []string) error {
	flagSet := flag.NewFlagSet("lip", flag.ContinueOnError)

	// Rewrite the default messages.
	flagSet.Usage = func() {
		// Do nothing.
	}

	// Parse flags.
	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.versionFlag, "version", false, "")
	flagSet.BoolVar(&flagDict.versionFlag, "V", false, "")
	flagSet.BoolVar(&flagDict.verboseFlag, "verbose", false, "")
	flagSet.BoolVar(&flagDict.verboseFlag, "v", false, "")
	flagSet.BoolVar(&flagDict.quietFlag, "quiet", false, "")
	flagSet.BoolVar(&flagDict.quietFlag, "q", false, "")
	flagSet.BoolVar(&flagDict.noColorFlag, "no-color", false, "")

	if err := flagSet.Parse(args); err != nil {
		return fmt.Errorf("cannot parse flags\n\t%w", err)
	}

	if flagDict.noColorFlag {
		log.SetFormatter(&nested.Formatter{NoColors: true})
	}

	// Set logging level.
	if flagDict.verboseFlag {
		log.SetLevel(log.DebugLevel)
	} else if flagDict.quietFlag {
		log.SetLevel(log.ErrorLevel)
	} else {
		log.SetLevel(log.InfoLevel)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		fmt.Print(helpMessage)
		return nil
	}

	// Version flag has the second highest priority.
	if flagDict.versionFlag {
		fmt.Printf("lip %v from %v", ctx.LipVersion().String(), os.Args[0])
		return nil
	}

	// Verbose and quiet flags are mutually exclusive.
	if flagDict.verboseFlag && flagDict.quietFlag {
		return fmt.Errorf("verbose and quiet flags are mutually exclusive")
	}

	// If there is a subcommand, run it and exit.
	if flagSet.NArg() >= 1 {
		switch flagSet.Arg(0) {
		case "cache":
			if err := cmdlipcache.Run(ctx, flagSet.Args()[1:]); err != nil {
				return err
			}
			return nil

		case "config":
			if err := cmdlipconfig.Run(ctx, flagSet.Args()[1:]); err != nil {
				return err
			}
			return nil

		case "install":
			if err := cmdlipinstall.Run(ctx, flagSet.Args()[1:]); err != nil {
				return err
			}
			return nil

		case "list":
			if err := cmdliplist.Run(ctx, flagSet.Args()[1:]); err != nil {
				return err
			}
			return nil

		case "show":
			if err := cmdlipshow.Run(ctx, flagSet.Args()[1:]); err != nil {
				return err
			}
			return nil

		case "tooth":
			if err := cmdliptooth.Run(ctx, flagSet.Args()[1:]); err != nil {
				return err
			}
			return nil

		case "uninstall":
			if err := cmdlipuninstall.Run(ctx, flagSet.Args()[1:]); err != nil {
				return err
			}
			return nil

		default:
			return fmt.Errorf("unknown command: lip %v", flagSet.Arg(0))
		}
	}

	return fmt.Errorf("no command specified. See 'lip --help' for more information")
}
