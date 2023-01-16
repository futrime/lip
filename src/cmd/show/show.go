package cmdlipshow

import (
	"flag"
	"os"
	"path/filepath"

	localfile "github.com/liteldev/lip/localfile"
	record "github.com/liteldev/lip/record"
	logger "github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag  bool
	filesFlag bool
}

const helpMessage = `
Usage:
  lip show [options] <tooth path>

Description:
  Show information about a installed tooth.

Options:
  -h, --help                  Show help.
  --files                     Show the full list of installed files.`

// Run is the entry point.
func Run() {
	if len(os.Args) == 2 {
		logger.Info(helpMessage)
		return
	}

	flagSet := flag.NewFlagSet("list", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")

	flagSet.BoolVar(&flagDict.filesFlag, "files", false, "")

	flagSet.Parse(os.Args[2:])

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// The tooth path should not be empty or more than one.
	if len(flagSet.Args()) == 0 ||
		len(flagSet.Args()) > 1 {
		logger.Error("the tooth path should be exactly one")
		return
	}

	// Get the record file path.
	recordFileName := localfile.GetRecordFileName(flagSet.Args()[0])
	recordDir, err := localfile.RecordDir()
	if err != nil {
		logger.Error(err.Error())
		return
	}
	recordFilePath := filepath.Join(recordDir, recordFileName)

	// Check if the record file exists.
	if _, err := os.Stat(recordFilePath); os.IsNotExist(err) {
		logger.Error("the tooth is not installed")
		return
	}

	// Get the record file content.
	recordObject, err := record.New(recordFilePath)
	if err != nil {
		logger.Error(err.Error())
		return
	}

	// Show information.
	logger.Info("Tooth-path: " + recordObject.ToothPath)
	logger.Info("Version: " + recordObject.Version.String())
	logger.Info("Name: " + recordObject.Information.Name)
	logger.Info("Description: " + recordObject.Information.Description)
	logger.Info("Author: " + recordObject.Information.Author)
	logger.Info("License: " + recordObject.Information.License)
	logger.Info("Homepage: " + recordObject.Information.Homepage)

	// Show the full list of installed files if the files flag is set.
	if flagDict.filesFlag {
		logger.Info("Files:")

		for _, placement := range recordObject.Placement {
			logger.Info("  " + placement.Destination)
		}
	}
}
