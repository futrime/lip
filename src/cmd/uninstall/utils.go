package cmdlipuninstall

import (
	"errors"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"

	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/tooth/toothrecord"
	"github.com/liteldev/lip/utils/logger"
	"github.com/liteldev/lip/utils/paths"
)

// Uninstall uninstalls a tooth.
// It deletes the files and folders specified in the record file.
// It also deletes the record file.
// However, when files are in both the possession of the record file
// and one in the possession list, the file is not deleted.
func Uninstall(recordFileName string, possessionList []string, isYes bool) error {
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

	// 2. Ask for confirmation if the tooth requires confirmation.

	if len(currentRecord.Confirmation) > 0 {
		for _, confirmation := range currentRecord.Confirmation {
			if confirmation.Type != "uninstall" {
				continue
			}

			if confirmation.GOOS != "" && confirmation.GOOS != runtime.GOOS {
				continue
			}

			if confirmation.GOARCH != "" && confirmation.GOARCH != runtime.GOARCH {
				continue
			}

			logger.Info(confirmation.Message + " (Y/n)")
			var ans string
			fmt.Scanln(&ans)
			if ans != "Y" && ans != "y" && ans != "" {
				return errors.New("uninstallation cancelled")
			}
		}
	}

	// 2. Run pre-uninstall commands.
	//    Iterate over the commands and run the commands that are
	//    for the current OS and architecture.
	for _, commandItem := range currentRecord.Commands {
		if commandItem.Type != "uninstall" {
			continue
		}

		// Validate GOOS
		if commandItem.GOOS != runtime.GOOS {
			continue
		}

		// Validate GOARCH. If GOARCH is empty, it is valid for all GOARCH.
		if commandItem.GOARCH != "" && commandItem.GOARCH != runtime.GOARCH {
			continue
		}

		// Run the command.
		for _, command := range commandItem.Commands {
			var cmd *exec.Cmd
			switch runtime.GOOS {
			case "windows":
				cmd = exec.Command("cmd", "/C", command)
			default:
				cmd = exec.Command("sh", "-c", command)
			}
			cmd.Stdin = os.Stdin
			cmd.Stderr = os.Stderr
			cmd.Stdout = os.Stdout
			err := cmd.Run()
			if err != nil {
				return errors.New("failed to run command: " + command + ": " + err.Error())
			}
		}
	}

	// 3. Delete files and folders.
	//    Iterate over the placements and delete files specified
	//    in the destinations. Iterate over the possessions and delete the folders
	//    as well as the files in the folders but keep the files in the specific possession
	//    list to preserved.

	//    Interate over the placements and delete files specified
	//    in the destinations.
	for _, placement := range currentRecord.Placement {
		if placement.GOOS != "" && placement.GOOS != runtime.GOOS {
			continue
		}

		if placement.GOARCH != "" && placement.GOARCH != runtime.GOARCH {
			continue
		}

		destination := placement.Destination
		destination = filepath.FromSlash(destination)
		destination = filepath.Clean(destination)

		// Get the absolute path of the destination.
		destination, err = filepath.Abs(destination)
		if err != nil {
			return errors.New("cannot get the absolute path of the destination " + destination + ": " + err.Error())
		}

		// Continue if the destination does not exist.
		if _, err := os.Stat(destination); os.IsNotExist(err) {
			continue
		}

		err = os.Remove(destination)
		if err != nil {
			logger.Error("cannot delete the file " + destination + ": " + err.Error() + ". Please delete it manually.")
		}

		// Delete all ancestor directories if they are empty until the workspace directory.
		workspaceDir, err := localfile.WorkspaceDir()
		if err != nil {
			return err
		}

		for parentDir := filepath.Dir(destination); parentDir != workspaceDir && paths.IsAncesterOf(workspaceDir, parentDir); parentDir = filepath.Dir(parentDir) {
			files, err := os.ReadDir(parentDir)
			if err != nil {
				return errors.New("cannot read the directory " + parentDir + ": " + err.Error())
			}

			if len(files) == 0 {
				err = os.Remove(parentDir)
				if err != nil {
					logger.Error("cannot delete the directory " + parentDir + ": " + err.Error() + ". Please delete it manually.")
				}
			} else {
				break
			}
		}
	}

	// Iterate over the possessions and delete the folders as well as
	// the files in the folders.
	for _, possession := range currentRecord.Possession {
		// Continue if the possession is in the new possession list.
		isInNewPossessionList := false
		for _, newPossession := range possessionList {
			if possession == newPossession {
				isInNewPossessionList = true
				break
			}
		}

		if isInNewPossessionList {
			continue
		}

		// Remove the folder.
		err = os.RemoveAll(possession)
		if err != nil {
			logger.Error("cannot delete the folder " + possession + ": " + err.Error() + ". Please delete it manually.")
		}
	}

	// 4. Delete the record file.
	//    The record file is deleted after the files and folders are deleted
	//    so that the record file is not deleted if the files and folders
	//    cannot be deleted.
	err = os.Remove(recordDir + "/" + recordFileName)
	if err != nil {
		logger.Error("cannot delete the record file " + recordDir + "/" + recordFileName + ": " + err.Error() + ". Please delete it manually.")
	}

	return nil
}
