package cmdlipuninstall

import (
	"errors"
	"os"
	"path/filepath"

	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/tooth/toothrecord"
)

// uninstall uninstalls a tooth.
// It deletes the files and folders specified in the record file.
// It also deletes the record file.
// However, when files are in both the possession of the record file
// and one in the possession list, the file is not deleted.
func uninstall(recordFileName string, possessionList []string) error {
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
