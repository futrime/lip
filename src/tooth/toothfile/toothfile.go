// Package toothfile includes functions for .tth files.
package toothfile

import (
	"archive/zip"
	"errors"
	"io"

	metadata "github.com/liteldev/lip/tooth/toothmetadata"
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
	filePrefix := GetFilePrefix(r)

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
