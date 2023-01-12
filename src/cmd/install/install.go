package cmdinstall

import (
	"flag"
	"os"

	"github.com/liteldev/lip/utils/logger"
)

type FlagDict struct {
	helpFlag           bool
	dryRunFlag         bool
	upgradeFlag        bool
	forceReinstallFlag bool
}

func Run() {
	const helpMessage = `
Usage:
  lip install [options] <requirement specifier>
  lip install [options] <tooth url/path>

Description:
  Install a tooth from:

  - A tooth repository.
  - A local or remote standalone tooth file (with suffix .tt).

Options:
  -h, --help                  Show help.
  --dry-run                   Don't actually install anything, just print what would be.
  --upgrade                   Upgrade the specified tooth to the newest available version.
  --force-reinstall           Reinstall the tooth even if they are already up-to-date.`

	flagSet := flag.NewFlagSet("install", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flag.BoolVar(&flagDict.helpFlag, "help", false, "")
	flag.BoolVar(&flagDict.helpFlag, "h", false, "")

	flag.BoolVar(&flagDict.dryRunFlag, "dry-run", false, "")

	flag.BoolVar(&flagDict.upgradeFlag, "upgrade", false, "")

	flag.BoolVar(&flagDict.forceReinstallFlag, "force-reinstall", false, "")

	flagSet.Parse(os.Args[2:])

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// Print help message if argument number is not 1.
	if flagSet.NArg() != 1 {
		logger.Error("Invalid number of arguments.")
		logger.Info(helpMessage)
		return
	}

	// Default to help message.
	logger.Info(helpMessage)
}
