package cmdlipinstall

import (
	"archive/zip"
	"errors"
	"fmt"
	"io"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"

	"github.com/liteldev/lip/download"
	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/specifiers"
	"github.com/liteldev/lip/tooth/toothfile"
	"github.com/liteldev/lip/tooth/toothrecord"
	"github.com/liteldev/lip/utils/logger"
	"github.com/liteldev/lip/utils/paths"
)

// getTooth gets the tooth file path of a tooth specifier either from the cache or from the tooth repository.
// If the tooth file is downloaded, it will be cached.
// If the specifier is local tooth file, it will return the path of the local tooth file.
// toothFilePath is the absolute path of the tooth file.
func getTooth(specifier specifiers.Specifier, progressBarStyle download.ProgressBarStyleType) (isCached bool, toothFilePath string, err error) {
	// For local tooth file, return the path directly.
	if specifier.Type() == specifiers.ToothFileKind {
		// Get full path of the tooth file.
		toothFilePath, err := filepath.Abs(specifier.ToothFilePath())
		if err != nil {
			return false, "", errors.New("cannot get full path of tooth file: " + specifier.ToothFilePath())
		}

		return false, toothFilePath, nil
	}

	// Get the path to the cache tooth file.
	cacheFileName := localfile.GetCachedToothFileName(specifier.String())
	cacheDirectory, err := localfile.CacheDir()
	if err != nil {
		return false, "", err
	}
	cacheFilePath := filepath.Join(cacheDirectory, cacheFileName)

	// Directly return the cached tooth file path if it exists.
	isCacheExist, err := localfile.IsCachedToothFileExist(specifier.String())
	if err != nil {
		return false, "", err
	}
	if isCacheExist {
		return true, cacheFilePath, nil
	}

	// Download the tooth file to the cache.
	err = downloadTooth(specifier, cacheFilePath, progressBarStyle)
	if err != nil {
		return false, "", err
	}

	return false, cacheFilePath, nil
}

// downloadTooth downloads a tooth file from a tooth repository, a tooth url,
// or a local path and returns the path of the downloaded tooth file.
// If the specifier is a requirement specifier, it should contain version.
func downloadTooth(specifier specifiers.Specifier, destination string, progressBarStyle download.ProgressBarStyleType) error {
	switch specifier.Type() {
	case specifiers.ToothFileKind:
		// Local tooth file is not accepted here.
		return errors.New("local tooth file is not able to be downloaded")

	case specifiers.ToothURLKind:
		// For tooth url, download the tooth file and return the path.

		tempFilePath := destination + ".tmp"

		err := download.DownloadFile(specifier.ToothURL(), tempFilePath, progressBarStyle)
		if err != nil {
			return err
		}

		// Move the downloaded file to the destination.
		os.Rename(tempFilePath, destination)

		return nil

	case specifiers.RequirementKind:
		// For requirement specifier, download the tooth via GOPROXY and return the path.

		tempFilePath := destination + ".tmp"

		urlPathSuffix := "+incompatible.zip"
		if strings.HasPrefix(specifier.ToothVersion().String(), "0.") || strings.HasPrefix(specifier.ToothVersion().String(), "1.") {
			urlPathSuffix = ".zip"
		}
		urlPath := specifier.ToothRepo() + "/@v/v" + specifier.ToothVersion().String() + urlPathSuffix

		err := download.DownloadGoproxyFile(urlPath, tempFilePath, progressBarStyle)
		if err != nil {
			return err
		}

		// Move the downloaded file to the destination.
		os.Rename(tempFilePath, destination)

		return nil
	}

	// Default to unknown error.
	return errors.New("unknown error")
}

// install installs the .tth file.
func install(t toothfile.ToothFile, isManuallyInstalled bool, isYes bool) error {
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

	// 2. Ask for confirmation if the tooth requires confirmation.

	if len(t.Metadata().Confirmation) > 0 {
		for _, confirmation := range t.Metadata().Confirmation {
			if confirmation.Type != "install" {
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
				return errors.New("installation cancelled")
			}
		}
	}

	// 3. Place the files to the right place in the workspace.

	// Open the .tth file.
	r, err := zip.OpenReader(t.FilePath())
	if err != nil {
		return errors.New("failed to open tooth file " + t.FilePath())
	}
	defer r.Close()

	workSpaceDir, err := localfile.WorkspaceDir()
	if err != nil {
		return err
	}

	// Get the file prefix.
	filePrefix := toothfile.GetFilePrefix(r)

	for _, placement := range t.Metadata().Placement {
		if placement.GOOS != "" && placement.GOOS != runtime.GOOS {
			continue
		}

		if placement.GOARCH != "" && placement.GOARCH != runtime.GOARCH {
			continue
		}

		source := placement.Source
		destination := placement.Destination

		if !isYes {
			if !paths.IsAncesterOf(workSpaceDir, destination) {
				logger.Info("This tooth is placing files to " + destination + ", which is not in current workspace. Do you want to continue? (y/N)")
				var ans string
				fmt.Scanln(&ans)
				if ans != "y" && ans != "Y" {
					return errors.New("installation aborted")
				}
				isYes = true
			}
		}

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
					return errors.New("failed to open " + source + " in " + t.FilePath())
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

	// 4. Run the post-install script.
	for _, commandItem := range t.Metadata().Commands {
		if commandItem.Type != "install" {
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

		// Run the command. When error occurs, just report it and continue.
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
				logger.Error("failed to run command: " + command + ": " + err.Error())
			}
		}
	}

	// 5. Install the record file.

	// Create a record object from the metadata.
	record := toothrecord.NewFromMetadata(t.Metadata(), isManuallyInstalled)

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

	return nil
}
