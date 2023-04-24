package toothrecord

import (
	"errors"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"github.com/lippkg/lip/localfile"
)

// Get returns the tooth record of the specified tooth.
func Get(toothPath string) (Record, error) {
	// Get the tooth record list.
	recordList, err := ListAll()
	if err != nil {
		return Record{}, err
	}

	// Get the tooth record.
	for _, record := range recordList {
		if record.ToothPath == toothPath {
			return record, nil
		}
	}

	return Record{}, errors.New("tooth record not found")
}

// ListAll lists all installed tooth records.
func ListAll() ([]Record, error) {
	recordList := make([]Record, 0)

	// Get all record paths
	recordDir, err := localfile.RecordDir()
	if err != nil {
		return nil, errors.New("failed to get record directory: " + err.Error())
	}

	files, err := os.ReadDir(recordDir)
	if err != nil {
		return nil, errors.New("failed to read record directory: " + err.Error())
	}

	for _, file := range files {
		recordFilePath := filepath.Join(recordDir, file.Name())

		// Read record
		record, err := NewFromFile(recordFilePath)
		if err != nil {
			return nil, errors.New("failed to read record file" + file.Name() + ": " + err.Error())
		}

		recordList = append(recordList, record)
	}

	// Sort record list by tooth path in a case-insensitive order.
	sort.Slice(recordList, func(i, j int) bool {
		return strings.ToLower(recordList[i].ToothPath) < strings.ToLower(recordList[j].ToothPath)
	})

	return recordList, nil
}

// IsToothInstalled returns true if the tooth is installed.
func IsToothInstalled(toothPath string) (bool, error) {
	// Get the tooth record list.
	recordList, err := ListAll()
	if err != nil {
		return false, err
	}

	// Check if the tooth is installed.
	for _, record := range recordList {
		if record.ToothPath == toothPath {
			return true, nil
		}
	}

	return false, nil
}
