package cmdliptoothinit

import (
	"errors"
	"flag"
	"os"

	"github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag bool
}

const defaultToothJsonContent = `{
    "format_version": 1,
    "tooth": "<tooth path>",
    "version": "<version>",
    "dependencies": {},
    "information": {
        "name": "<name>",
        "description": "<description>",
        "author": "<author>",
        "license": "<license>",
        "homepage": "<homepage>"
    },
    "placement": [
        {
            "source": "<placement source>",
            "destination": "<placement destination>"
        }
    ],
	"possession": [
		"<private directory of your tooth ends with '/'>"
	]
}`

const helpMessage = `
Usage:
  lip tooth init [options]

Description:
  Initialize and writes a new tooth.json file in the current directory, in effect creating a new tooth rooted at the current directory.

Options:
  -h, --help                  Show help.`

// Run is the entry point.
func Run() {
	// If there is no argument, initialize a new tooth.
	if len(os.Args) == 3 {
		err := initTooth()

		if err != nil {
			logger.Error(err.Error())
			return
		}

		logger.Info("tooth.json created successfully")
		logger.Info("please edit tooth.json and modify the values with \"<>\"")

		return
	}

	flagSet := flag.NewFlagSet("init", flag.ExitOnError)

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

// initTooth initializes a new tooth.
func initTooth() error {
	// Check if tooth.json already exists.
	if _, err := os.Stat("tooth.json"); err == nil {
		return errors.New("tooth.json already exists in the current directory")
	}

	// Create tooth.json.
	file, err := os.Create("tooth.json")
	if err != nil {
		return errors.New("failed to create tooth.json")
	}

	// Write default tooth.json content.
	_, err = file.WriteString(defaultToothJsonContent)
	if err != nil {
		return errors.New("failed to write tooth.json")
	}

	return nil
}
