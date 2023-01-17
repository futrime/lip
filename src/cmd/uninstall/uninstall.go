package cmdlipuninstall

import (
	"errors"
	"flag"
	"os"
	"path/filepath"

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

		err = uninstall(recordFileName)
		if err != nil {
			logger.Error(err.Error())
			return
		}
	}

	logger.Info("Successfully uninstalled all teeth.")
}

// uninstall uninstalls a tooth.
func uninstall(recordFileName string) error {
	// Read the record file.
	recordDir, err := localfile.RecordDir()
	if err != nil {
		return err
	}

	content, err := os.ReadFile(recordDir + "/" + recordFileName)
	if err != nil {
		return errors.New("cannot read the record file " + recordDir + "/" + recordFileName + ": " + err.Error())
	}

	// Parse the record file.
	currentRecord, err := toothrecord.NewFromJSON(content)
	if err != nil {
		return errors.New(err.Error())
	}

	// Interate over the placements and delete files specified
	// in the destinations.
	for _, placement := range currentRecord.Placement {
		workspaceDir, err := localfile.WorkSpaceDir()
		if err != nil {
			return err
		}

		destination := workspaceDir + "/" + placement.Destination

		// Continue if the destination does not exist.
		if _, err := os.Stat(destination); os.IsNotExist(err) {
			continue
		}

		err = os.Remove(destination)
		if err != nil {
			return errors.New("cannot delete the file " + destination + ": " + err.Error())
		}

		// Delete the parent directory if it is empty.
		// TODO: recursively delete parent directories until the workspace directory.
		parentDir := filepath.Dir(destination)
		files, err := os.ReadDir(parentDir)
		if err != nil {
			return errors.New("cannot read the directory " + parentDir + ": " + err.Error())
		}

		if len(files) == 0 {
			err = os.Remove(parentDir)
			if err != nil {
				return errors.New("cannot delete the directory " + parentDir + ": " + err.Error())
			}
		}
	}

	// Iterate over the possessions and delete the folders as well as
	// the files in the folders.
	for _, possession := range currentRecord.Possession {
		workspaceDir, err := localfile.WorkSpaceDir()
		if err != nil {
			return err
		}

		// Remove the folder.
		err = os.RemoveAll(workspaceDir + "/" + possession)
		if err != nil {
			return errors.New("cannot delete the folder " + workspaceDir + "/" + possession + ": " + err.Error())
		}
	}

	// Delete the record file.
	err = os.Remove(recordDir + "/" + recordFileName)
	if err != nil {
		return errors.New("cannot delete the record file " + recordDir + "/" + recordFileName + ": " + err.Error())
	}

	return nil
}
