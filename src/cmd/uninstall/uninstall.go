package cmdlipuninstall

import (
	"flag"
	"os"
	"path/filepath"
	"strings"

	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/registry"
	"github.com/liteldev/lip/tooth/toothrecord"
	"github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag           bool
	yesFlag            bool
	keepPossessionFlag bool
}

const helpMessage = `
Usage:
  lip uninstall [options] <tooths>

Description:
  Uninstall tooths.

Options:
  -h, --help                  Show help.
  -y, --yes                   Skip confirmation.
  --keep-possession           Keep files that the tooth author specified the tooth to occupy. These files are often configuration files, data files, etc.`

// Run is the entry point.
func Run(args []string) {
	var err error

	flagSet := flag.NewFlagSet("uninstall", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.yesFlag, "yes", false, "")
	flagSet.BoolVar(&flagDict.yesFlag, "y", false, "")
	flagSet.BoolVar(&flagDict.keepPossessionFlag, "keep-possession", false, "")
	flagSet.Parse(args)

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// Check if there are any arguments.
	if flagSet.NArg() == 0 {
		logger.Error("Too few arguments")
		os.Exit(1)
	}

	// 1. Check if all tooth paths are installed.

	logger.Info("Checking if all tooth paths are installed...")

	// Get tooth paths from arguments.
	toothPathList := flagSet.Args()

	// Convert all aliases to tooth paths.
	for i, toothPath := range toothPathList {
		if !strings.Contains(toothPath, "/") {
			toothPathList[i], err = registry.LookupAlias(toothPath)
			if err != nil {
				logger.Error(err.Error())
				os.Exit(1)
			}
		}
	}

	// Make a map of tooth paths.
	// The value of the map is the name of the record file.
	toothPathMap := make(map[string]string)
	for _, toothPath := range toothPathList {
		toothPathMap[strings.ToLower(toothPath)] = ""
	}

	// Read record files.
	recordDir, err := localfile.RecordDir()
	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}

	files, err := os.ReadDir(recordDir)
	if err != nil {
		logger.Error("cannot read the record directory " + recordDir + ": " + err.Error())
		os.Exit(1)
	}

	for _, file := range files {
		// Read the file as JSON.
		content, err := os.ReadFile(recordDir + "/" + file.Name())
		if err != nil {
			logger.Error("cannot read the record file " + recordDir + "/" + file.Name() + ": " + err.Error())
			os.Exit(1)
		}

		// Parse the JSON.
		currentRecord, err := toothrecord.NewFromJSON(content)
		if err != nil {
			logger.Error(err.Error())
			os.Exit(1)
		}

		// Check if the tooth path is in toothPathMap.
		if _, ok := toothPathMap[currentRecord.ToothPath]; ok {
			toothPathMap[currentRecord.ToothPath] = file.Name()
		}
	}

	// Check if all tooths to uninstall are installed.
	for toothPath, recordFilePath := range toothPathMap {
		if recordFilePath == "" {
			logger.Error("the tooth " + toothPath + " is not installed")
			os.Exit(1)
		}
	}

	// 2. Uninstall tooths.

	logger.Info("Uninstalling tooths...")

	for toothPath, recordFileName := range toothPathMap {
		logger.Info("  Uninstalling " + toothPath + "...")

		possessionList := make([]string, 0)
		if flagDict.keepPossessionFlag {
			// Read the record file.
			recordFilePath := filepath.Join(recordDir, recordFileName)
			record, err := toothrecord.NewFromFile(recordFilePath)
			if err != nil {
				logger.Error("cannot read the record file " + recordFilePath + ": " + err.Error())
				os.Exit(1)
			}
			possessionList = record.Possession
		}

		err = Uninstall(recordFileName, possessionList, flagDict.yesFlag)
		if err != nil {
			logger.Error(err.Error())
			os.Exit(1)
		}
	}

	logger.Info("Successfully uninstalled all tooths.")
}
