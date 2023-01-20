package cmdlipuninstall

import (
	"flag"
	"os"

	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/tooth/toothrecord"
	"github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip uninstall [options] <tooth paths>

Description:
  Uninstall teeth.

Options:
  -h, --help                  Show help.`

// Run is the entry point.
func Run() {
	// If there is no argument, print help message and exit.
	if len(os.Args) == 2 {
		logger.Info(helpMessage)
		return
	}

	flagSet := flag.NewFlagSet("uninstall", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")

	flagSet.Parse(os.Args[2:])

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// 1. Check if all tooth paths are installed.

	logger.Info("Checking if all tooth paths are installed...")

	// Get tooth paths from arguments.
	toothPathList := flagSet.Args()

	// Make a map of tooth paths.
	// The value of the map is the name of the record file.
	toothPathMap := make(map[string]string)
	for _, toothPath := range toothPathList {
		toothPathMap[toothPath] = ""
	}

	// Read record files.
	recordDir, err := localfile.RecordDir()
	if err != nil {
		logger.Error(err.Error())
		return
	}

	files, err := os.ReadDir(recordDir)
	if err != nil {
		logger.Error("cannot read the record directory " + recordDir + ": " + err.Error())
		return
	}

	for _, file := range files {
		// Read the file as JSON.
		content, err := os.ReadFile(recordDir + "/" + file.Name())
		if err != nil {
			logger.Error("cannot read the record file " + recordDir + "/" + file.Name() + ": " + err.Error())
			return
		}

		// Parse the JSON.
		currentRecord, err := toothrecord.NewFromJSON(content)
		if err != nil {
			logger.Error(err.Error())
			return
		}

		// Check if the tooth path is in toothPathMap.
		if _, ok := toothPathMap[currentRecord.ToothPath]; ok {
			toothPathMap[currentRecord.ToothPath] = file.Name()
		}
	}

	// Check if all teeth to uninstall are installed.
	for toothPath, recordFilePath := range toothPathMap {
		if recordFilePath == "" {
			logger.Error("the tooth " + toothPath + " is not installed")
			return
		}
	}

	// 2. Uninstall teeth.

	logger.Info("Uninstalling teeth...")

	for toothPath, recordFileName := range toothPathMap {
		logger.Info("Uninstalling " + toothPath + "...")

		err = uninstall(recordFileName, make([]string, 0))
		if err != nil {
			logger.Error(err.Error())
			return
		}
	}

	logger.Info("Successfully uninstalled all teeth.")
}
