// Package cmdlip is the entry point of the lip command.
package cmdlip

import (
	"flag"
	"os"
	"path/filepath"

	cmdlipcache "github.com/liteldev/lip/cmd/cache"
	cmdlipexec "github.com/liteldev/lip/cmd/exec"
	cmdlipinstall "github.com/liteldev/lip/cmd/install"
	cmdliplist "github.com/liteldev/lip/cmd/list"
	cmdlipshow "github.com/liteldev/lip/cmd/show"
	cmdliptooth "github.com/liteldev/lip/cmd/tooth"
	cmdlipuninstall "github.com/liteldev/lip/cmd/uninstall"
	"github.com/liteldev/lip/context"
	"github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag    bool
	versionFlag bool
}

const helpMessage = `
Usage:
  lip [options]
  lip <command> [subcommand options] ...

Commands:
  cache                       Inspect and manage Lip's cache.
  exec                        Execute a Lip tool.
  install                     Install a tooth.
  list                        List installed teeth.
  show                        Show information about installed teeth.
  tooth                       Maintain a tooth.
  uninstall                   Uninstall a tooth.

Options:
  -h, --help                  Show help.
  -V, --version               Show version and exit.`

const versionMessage = "Lip %s from %s"

// Run is the entry point of the lip command.
func Run() {
	// Initialize context
	context.Init()

	// If there is a subcommand, run it and exit.
	if len(os.Args) >= 2 {
		switch os.Args[1] {
		case "cache":
			cmdlipcache.Run()
			return
		case "exec":
			cmdlipexec.Run()
			return
		case "install":
			cmdlipinstall.Run()
			return
		case "list":
			cmdliplist.Run()
			return
		case "show":
			cmdlipshow.Run()
			return
		case "tooth":
			cmdliptooth.Run()
			return
		case "uninstall":
			cmdlipuninstall.Run()
			return
		}
	}

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

	flagSet.Parse(os.Args[1:])

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	if flagDict.versionFlag {
		exPath, _ := filepath.Abs(os.Args[0])
		logger.Info(versionMessage, context.Version.String(), exPath)
		return
	}

	// If there is no flag, print help message and exit.
	logger.Info(helpMessage)
}
