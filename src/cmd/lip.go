// Package cmdlip is the entry point of the lip command.
package cmdlip

import (
	"flag"
	"os"
	"path/filepath"

	"github.com/liteldev/lip/context"
	"github.com/liteldev/lip/utils/logger"

	cmdlipautoremove "github.com/liteldev/lip/cmd/autoremove"
	cmdlipcache "github.com/liteldev/lip/cmd/cache"
	cmdlipexec "github.com/liteldev/lip/cmd/exec"
	cmdlipinstall "github.com/liteldev/lip/cmd/install"
	cmdliplist "github.com/liteldev/lip/cmd/list"
	cmdlipshow "github.com/liteldev/lip/cmd/show"
	cmdliptooth "github.com/liteldev/lip/cmd/tooth"
	cmdlipuninstall "github.com/liteldev/lip/cmd/uninstall"
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
  -q, --quiet                 Show only errors.`

const versionMessage = "Lip %s from %s"

// Run is the entry point of the lip command.
func Run(args []string) {
	// Initialize context
	context.Init()

	flagSet := flag.NewFlagSet("lip", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
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
	flagSet.Parse(args)

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// Version flag has the second highest priority.
	if flagDict.versionFlag {
		exPath, _ := filepath.Abs(os.Args[0])
		logger.Info(versionMessage, context.Version.String(), exPath)
		return
	}

	// Verbose and quiet flags are mutually exclusive.
	if flagDict.verboseFlag && flagDict.quietFlag {
		logger.Error("Verbose and quiet flags are mutually exclusive")
		os.Exit(1)
	}

	// Set logging level.
	if flagDict.verboseFlag {
		logger.SetLevel(logger.DebugLevel)
	} else if flagDict.quietFlag {
		logger.SetLevel(logger.ErrorLevel)
	} else {
		logger.SetLevel(logger.InfoLevel)
	}

	// If there is a subcommand, run it and exit.
	if flagSet.NArg() >= 1 {
		switch flagSet.Arg(0) {
		case "autoremove":
			cmdlipautoremove.Run(flagSet.Args()[1:])
			return
		case "cache":
			cmdlipcache.Run(flagSet.Args()[1:])
			return
		case "exec":
			cmdlipexec.Run(flagSet.Args()[1:])
			return
		case "install":
			cmdlipinstall.Run(flagSet.Args()[1:])
			return
		case "list":
			cmdliplist.Run(flagSet.Args()[1:])
			return
		case "show":
			cmdlipshow.Run(flagSet.Args()[1:])
			return
		case "tooth":
			cmdliptooth.Run(flagSet.Args()[1:])
			return
		case "uninstall":
			cmdlipuninstall.Run(flagSet.Args()[1:])
			return
		default:
			logger.Error("Unknown command: lip %s", flagSet.Arg(0))
			os.Exit(1)
		}
	}

	// Otherwise, print help message and exit.
	logger.Info(helpMessage)
}
