// Package toothfile includes functions for .tth files.
package toothfile

import (
	"archive/zip"
	"errors"
	"io"
	"os"
	"path/filepath"
	"strings"

	localfile "github.com/liteldev/lip/localfile"
	metadata "github.com/liteldev/lip/tooth/toothmetadata"
	recordutils "github.com/liteldev/lip/tooth/toothrecord"
)

// ToothFile is the struct that contains the metadata of a .tth file.
type ToothFile struct {
	filePath string
	metadata metadata.Metadata
}

// New creates a new ToothFile struct from a file path of a .tth file.
func New(filePath string) (ToothFile, error) {
	r, err := zip.OpenReader(filePath)
	if err != nil {
		return ToothFile{}, errors.New("failed to open tooth file " + filePath)
	}
	defer r.Close()

	// Get the file prefix.
	filePrefix := getFilePrefix(r)

	// Iterate through the files in the archive,
	// and find tooth.json.
	for _, f := range r.File {
		if f.Name == filePrefix+"tooth.json" {
			// Open tooth.json.
			rc, err := f.Open()
			if err != nil {
				return ToothFile{}, errors.New("failed to open tooth.json in " + filePath)
			}

			// Read tooth.json as a string.
			data, err := io.ReadAll(rc)
			rc.Close()
			if err != nil {
				return ToothFile{}, errors.New("failed to read tooth.json in " + filePath)
			}

			// Decode tooth.json.
			metadata, err := metadata.NewFromJSON(data)
			if err != nil {
				return ToothFile{}, err
			}

			// Parse the wildcard placements.
			metadata = parseMetadataPlacement(metadata, r, filePrefix)

			return ToothFile{filePath, metadata}, nil
		}
	}

	// If tooth.json is not found, return an error.
	return ToothFile{}, errors.New("tooth.json not found in " + filePath)
}

// FilePath returns the file path of the .tth file.
func (t ToothFile) FilePath() string {
	return t.filePath
}

// Metadata returns the metadata of the .tth file.
func (t ToothFile) Metadata() metadata.Metadata {
	return t.metadata
}

// Install installs the .tth file.
// TODO#1: Check if the tooth is already installed.
// TODO#2: Directory placement.
func (t ToothFile) Install() error {
	// 1. Check if the tooth is already installed.

	recordDir, err := localfile.RecordDir()
	if err != nil {
		return err
	}

	recordFilePath := recordDir + "/" +
		localfile.GetRecordFileName(t.Metadata().ToothPath)

	// If the record file already exists, return an error.
	if _, err := os.Stat(recordFilePath); err == nil {
		return errors.New("the tooth is already installed")
	}

	// 2. Install the record file.

	// Create a record object from the metadata.
	record := recordutils.NewFromMetadata(t.metadata)

	// Encode the record object to JSON.
	recordJSON, err := record.JSON()
	if err != nil {
		return err
	}

	// Write the metadata bytes to the record file.
	err = os.WriteFile(recordFilePath, recordJSON, 0755)
	if err != nil {
		return errors.New("failed to write record file " + recordFilePath + " " + err.Error())
	}

	// 3. Place the files to the right place in the workspace.

	workSpaceDir, err := localfile.WorkSpaceDir()
	if err != nil {
		return err
	}

	// Open the .tth file.
	r, err := zip.OpenReader(t.filePath)
	if err != nil {
		return errors.New("failed to open tooth file " + t.filePath)
	}
	defer r.Close()

	// Get the file prefix.
	filePrefix := getFilePrefix(r)

	for _, placement := range t.metadata.Placement {
		source := placement.Source
		destination := workSpaceDir + "/" + placement.Destination

		// Create the parent directory of the destination.
		os.MkdirAll(filepath.Dir(destination), 0755)

		// Iterate through the files in the archive,
		// and find the source file.
		for _, f := range r.File {
			// Do not copy directories.
			if strings.HasSuffix(f.Name, "/") {
				continue
			}

			if f.Name == filePrefix+source {
				// Open the source file.
				rc, err := f.Open()
				if err != nil {
					return errors.New("failed to open " + source + " in " + t.filePath)
				}

				// Directly copy the source file to the destination.
				fw, err := os.Create(destination)
				if err != nil {
					return errors.New("failed to create " + destination)
				}

				io.Copy(fw, rc)

				rc.Close()
				fw.Close()
			}
		}
	}

	return nil
}
