package cmdliptoothinit

import (
	"flag"
	"os"

	logger "github.com/liteldev/lip/utils/logger"
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
    ]
}`

const helpMessage = `
Usage:
  lip tooth init [options]

Description:
  Initialize and writes a new tooth.json file in the current directory, in effect creating a new tooth rooted at the current directory.

Options:
  -h, --help                  Show help.`

// Run is the entry point of the install command.
func Run() {
	flagSet := flag.NewFlagSet("init", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flag.BoolVar(&flagDict.helpFlag, "help", false, "")
	flag.BoolVar(&flagDict.helpFlag, "h", false, "")

	flagSet.Parse(os.Args[3:])

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// Default to initializing a new tooth.
	initTooth()
}

func initTooth() {
	// Check if tooth.json already exists.
	if _, err := os.Stat("tooth.json"); err == nil {
		logger.Error("tooth.json already exists in the current directory")
		return
	}

	// Create tooth.json.
	file, err := os.Create("tooth.json")
	if err != nil {
		logger.Error("failed to create tooth.json")
		return
	}

	// Write default tooth.json content.
	_, err = file.WriteString(defaultToothJsonContent)
	if err != nil {
		logger.Error("failed to write tooth.json")
		return
	}

	logger.Info("tooth.json created successfully")
	logger.Info("please edit tooth.json and modify the values with \"<>\"")
}
