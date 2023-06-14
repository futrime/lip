package cmdlip

import (
	"errors"
	"flag"
	"fmt"
	"os"

	"github.com/lippkg/lip/internal/contexts"
	"github.com/lippkg/lip/internal/loggingutils"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag    bool
	versionFlag bool
	verboseFlag bool
	quietFlag   bool
}

const helpMessage = `
Usage:
  lip [options] [<command> [subcommand options]] ...

Commands:
  autoremove                  Uninstall tooths that are not depended by any other tooths.
  cache                       Inspect and manage Lip's cache.
  exec                        Execute a Lip tool.
  install                     Install a tooth.
  list                        List installed tooths.
  show                        Show information about installed tooths.
  tooth                       Maintain a tooth.
  uninstall                   Uninstall a tooth.

Options:
  -h, --help                  Show help.
  -V, --version               Show version and exit.
  -v, --verbose               Show verbose output.
  -q, --quiet                 Show only errors.
`

// Run is the entry point of the lip command.
func Run(ctx contexts.Context, args []string) error {
	var err error

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
	err = flagSet.Parse(args[1:])
	if err != nil {
		return err
	}

	// Set logging level.
	if flagDict.verboseFlag {
		loggingutils.SetLoggingLevel(loggingutils.DebugLevel)
	} else if flagDict.quietFlag {
		loggingutils.SetLoggingLevel(loggingutils.ErrorLevel)
	} else {
		loggingutils.SetLoggingLevel(loggingutils.InfoLevel)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		loggingutils.Info(helpMessage)
		return nil
	}

	// Version flag has the second highest priority.
	if flagDict.versionFlag {
		loggingutils.Info("Lip %s from %s", ctx.LipVersion().String(), os.Args[0])
		return nil
	}

	// Verbose and quiet flags are mutually exclusive.
	if flagDict.verboseFlag && flagDict.quietFlag {
		return errors.New("verbose and quiet flags are mutually exclusive")
	}

	// If there is a subcommand, run it and exit.
	if flagSet.NArg() >= 1 {
		switch flagSet.Arg(0) {

		default:
			return fmt.Errorf("unknown command: lip %s", flagSet.Arg(0))
		}
	}

	return errors.New("no command specified. See 'lip --help' for more information")
}
