package cmdlipcachepurge

import (
	"errors"
	"flag"
	"os"

	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip cache purge [options]

Description:
  Remove all items from the cache.

Options:
  -h, --help                  Show help.`

// Run is the entry point.
func Run() {
	// If there is no argument, initialize a new tooth.
	if len(os.Args) == 3 {
		err := purgeCache()

		if err != nil {
			logger.Error(err.Error())
			return
		}

		return
	}

	flagSet := flag.NewFlagSet("purge", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")

	flagSet.Parse(os.Args[3:])

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}
}

// purgeCache removes all items from the cache.
func purgeCache() error {
	cacheDir, err := localfile.CacheDir()
	if err != nil {
		return err
	}

	err = os.RemoveAll(cacheDir)
	if err != nil {
		return errors.New("failed to remove cache directory: " + err.Error())
	}

	// Create a new cache directory.
	err = os.MkdirAll(cacheDir, 0755)
	if err != nil {
		return errors.New("failed to create a new cache directory: " + err.Error())
	}

	return nil
}
