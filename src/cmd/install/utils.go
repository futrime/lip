package cmdlipinstall

import (
	"archive/zip"
	"bufio"
	"errors"
	"io"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"sort"
	"strings"

	"github.com/liteldev/lip/context"
	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/tooth/toothfile"
	"github.com/liteldev/lip/tooth/toothrecord"
	"github.com/liteldev/lip/utils/download"
	versionutils "github.com/liteldev/lip/utils/version"
)

// downloadTooth downloads a tooth file from a tooth repository, a tooth url,
// or a local path and returns the path of the downloaded tooth file.
// If the specifier is a requirement specifier, it should contain version.
func downloadTooth(specifier Specifier) (string, error) {
	switch specifier.Type() {
	case ToothFileSpecifierType:
		// For local tooth file, just return the path.

		// Get full path of the tooth file.
		toothFilePath, err := filepath.Abs(specifier.ToothFilePath())
		if err != nil {
			return "", errors.New("cannot get full path of tooth file: " + specifier.ToothFilePath())
		}

		return toothFilePath, nil

	case ToothURLSpecifierType:
		// For tooth url, download the tooth file and return the path.

		cacheFileName := localfile.GetCachedToothFileName(specifier.String())

		// Directly return the cached tooth file path if it exists.
		isCacheExist, err := localfile.IsCachedToothFileExist(specifier.String())
		if err != nil {
			return "", err
		}

		if isCacheExist {
			cacheDir, err := localfile.CacheDir()
			if err != nil {
				return "", err
			}
			return cacheDir + "/" + cacheFileName, nil
		}

		// Download the tooth file to the cache.
		cacheDir, err := localfile.CacheDir()
		if err != nil {
			return "", err
		}

		cacheFilePath := cacheDir + "/" + cacheFileName

		err = download.DownloadFile(specifier.ToothURL(), cacheFilePath)
		if err != nil {
			return "", err
		}

		return cacheFilePath, nil

	case RequirementSpecifierType:
		// For requirement specifier, download the tooth via GOPROXY and return the path.

		cacheFileName := localfile.GetCachedToothFileName(specifier.String())

		// Directly return the cached tooth file path if it exists.
		isCacheExist, err := localfile.IsCachedToothFileExist(specifier.String())
		if err != nil {
			return "", err
		}

		if isCacheExist {
			cacheDir, err := localfile.CacheDir()
			if err != nil {
				return "", err
			}
			return cacheDir + "/" + cacheFileName, nil
		}

		// Get the tooth file url.
		urlSuffix := "+incompatible.zip"
		if strings.HasPrefix(specifier.ToothVersion().String(), "0.") || strings.HasPrefix(specifier.ToothVersion().String(), "1.") {
			urlSuffix = ".zip"
		}
		url := context.Goproxy + "/" + specifier.ToothRepo() + "/@v/v" + specifier.ToothVersion().String() + urlSuffix

		// Download the tooth file to the cache.
		cacheDir, err := localfile.CacheDir()
		if err != nil {
			return "", err
		}

		cacheFilePath := cacheDir + "/" + cacheFileName

		err = download.DownloadFile(url, cacheFilePath)
		if err != nil {
			return "", err
		}

		return cacheFilePath, nil
	}

	// Default to unknown error.
	return "", errors.New("unknown error")
}

// fetchVersionList fetches the version list of a tooth repository.
func fetchVersionList(repoPath string) ([]versionutils.Version, error) {
	if !isValidRepoPath(repoPath) {
		return nil, errors.New("invalid repository path: " + repoPath)
	}

	url := context.Goproxy + "/" + repoPath + "/@v/list"

	// To lowercases.
	url = strings.ToLower(url)

	// Get the version list.
	resp, err := http.Get(url)
	if err != nil {
		return nil, errors.New("cannot access GOPROXY: " + context.Goproxy)
	}
	defer resp.Body.Close()

	if resp.StatusCode != 200 {
		return nil, errors.New("cannot access tooth repository: " + repoPath)
	}

	// Each line is a version.
	var versionList []versionutils.Version
	scanner := bufio.NewScanner(resp.Body)
	for scanner.Scan() {
		versionString := scanner.Text()
		versionString = strings.TrimPrefix(versionString, "v")
		versionString = strings.TrimSuffix(versionString, "+incompatible")
		version, err := versionutils.NewFromString(versionString)
		if err != nil {
			continue
		}
		versionList = append(versionList, version)
	}

	// Sort the version list in descending order.
	sort.Slice(versionList, func(i, j int) bool {
		return versionutils.GreaterThan(versionList[i], versionList[j])
	})

	return versionList, nil
}

// Install installs the .tth file.
func install(t toothfile.ToothFile) error {
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
	record := toothrecord.NewFromMetadata(t.Metadata())

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
	r, err := zip.OpenReader(t.FilePath())
	if err != nil {
		return errors.New("failed to open tooth file " + t.FilePath())
	}
	defer r.Close()

	// Get the file prefix.
	filePrefix := toothfile.GetFilePrefix(r)

	for _, placement := range t.Metadata().Placement {
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

		// Run the command.
		for _, command := range commandItem.Commands {
			var cmd *exec.Cmd
			switch runtime.GOOS {
			case "windows":
				cmd = exec.Command("cmd", "/C", command)
			default:
				cmd = exec.Command("sh", "-c", command)
			}
			cmd.Stderr = os.Stderr
			cmd.Stdout = os.Stdout
			err := cmd.Run()
			if err != nil {
				return errors.New("failed to run command: " + command + ": " + err.Error())
			}
		}
	}

	return nil
}

// isValidRepoPath checks if the repoPath is valid.
func isValidRepoPath(repoPath string) bool {
	reg := regexp.MustCompile(`^[a-zA-Z\d-_\.\/]*$`)

	// If not matched or the matched string is not the same as the specifier, it is an
	// invalid requirement specifier.
	return reg.FindString(repoPath) == repoPath
}

// validateToothRepoVersion checks if the version of the tooth repository is valid.
func validateToothRepoVersion(repoPath string, version versionutils.Version) error {
	if !isValidRepoPath(repoPath) {
		return errors.New("invalid repository path: " + repoPath)
	}

	// Check if the version is valid.
	urlSuffix := "+incompatible.info"
	if strings.HasPrefix(version.String(), "0.") || strings.HasPrefix(version.String(), "1.") {
		urlSuffix = ".info"
	}
	url := context.Goproxy + "/" + repoPath + "/@v/v" + version.String() + urlSuffix

	// To lower case.
	url = strings.ToLower(url)

	// Get the version information.
	resp, err := http.Get(url)
	if err != nil {
		return errors.New("cannot access GOPROXY: " + context.Goproxy)
	}
	defer resp.Body.Close()

	// If the status code is 200, the version is valid.
	if resp.StatusCode != 200 {
		return errors.New("cannot access tooth: " + repoPath + "@" + version.String())
	}

	return nil
}
