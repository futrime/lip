// Package localfile deals with ~/.lip directory and its contents
// and ./.lip directory and its contents.
package localfile

import (
	"encoding/base64"
	"errors"
	"os"
	"path/filepath"
)

// Init initializes the ~/.lip and ./.lip directories.
// It should be called before any other functions in this package.
func Init() error {
	// Initialize the ~/.lip directory.
	homeLipDir, err := HomeLipDir()
	if err != nil {
		return err
	}
	cacheDir, err := CacheDir()
	if err != nil {
		return err
	}
	os.MkdirAll(homeLipDir, 0755)
	os.MkdirAll(cacheDir, 0755)

	// Initialize the ./.lip directory.
	workspaceLipDir, err := WorkspaceLipDir()
	if err != nil {
		return err
	}
	os.MkdirAll(workspaceLipDir, 0755)
	recordDir, err := RecordDir()
	if err != nil {
		return err
	}
	os.MkdirAll(recordDir, 0755)

	return nil
}

// CacheDir returns the path to the ~/.lip/cache directory.
func CacheDir() (string, error) {
	homeLipDir, err := HomeLipDir()
	if err != nil {
		return "", err
	}
	cacheDir := homeLipDir + "/cache"
	return cacheDir, nil
}

// GetCachedToothFileName returns the file name of the cached tooth file.
// Note that the cached tooth file may not exist.
func GetCachedToothFileName(fullSpecifier string) string {
	// Encode the full specifier with Base64.
	fullSpecifier = base64.StdEncoding.EncodeToString([]byte(fullSpecifier))

	return fullSpecifier + ".tth"
}

// GetRecordFileName returns the file name of the record file.
func GetRecordFileName(toothPath string) string {
	// Encode the tooth path with Base64.
	toothPath = base64.StdEncoding.EncodeToString([]byte(toothPath))

	return toothPath + ".json"
}

// HomeLipDir returns the path to the ~/.lip directory.
func HomeLipDir() (string, error) {
	// Set context.HomeLipDir.
	dirname, err := os.UserHomeDir()
	if err != nil {
		err = errors.New("failed to get user home directory")
		return "", err
	}
	homeLipDir := dirname + "/.lip"
	return homeLipDir, nil
}

// IsCachedToothFileExist returns true if the cached tooth file exists.
func IsCachedToothFileExist(fullSpecifier string) (bool, error) {
	// Get the path to the cached tooth file.
	cachedToothFileName := GetCachedToothFileName(fullSpecifier)

	// Check if the cached tooth file exists.
	cacheDir, err := CacheDir()
	if err != nil {
		return false, err
	}

	cachedToothFilePath := cacheDir + "/" + cachedToothFileName
	if _, err := os.Stat(cachedToothFilePath); err != nil {
		return false, nil
	}

	return true, nil
}

// RecordDir returns the path to the ./.lip/records directory.
func RecordDir() (string, error) {
	workspaceLipDir, err := WorkspaceLipDir()
	if err != nil {
		return "", err
	}
	recordDir := workspaceLipDir + "/records"
	return recordDir, nil
}

// WorkspaceDir returns the absolute path to the current working directory.
func WorkspaceDir() (string, error) {
	dirname, err := os.Getwd()
	if err != nil {
		err = errors.New("failed to get current directory: " + err.Error())
		return "", err
	}
	dirname, err = filepath.Abs(dirname)
	if err != nil {
		err = errors.New("failed to get absolute path of current directory: " + err.Error())
		return "", err
	}
	return dirname, nil
}

// WorkspaceLipDir returns the absolute path to the ./.lip directory.
func WorkspaceLipDir() (string, error) {
	dirname, err := WorkspaceDir()
	if err != nil {
		return "", err
	}

	workspaceLipDir := filepath.Join(dirname, ".lip")

	return workspaceLipDir, nil
}
