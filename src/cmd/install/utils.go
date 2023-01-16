package cmdlipinstall

import (
	"bufio"
	"encoding/json"
	"errors"
	"net/http"
	"path/filepath"
	"regexp"
	"sort"
	"strings"

	"github.com/liteldev/lip/context"
	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/utils/download"
	versionutils "github.com/liteldev/lip/utils/version"
)

// downloadTooth downloads a tooth file from a tooth repository, a tooth url,
// or a local path and returns the path of the downloaded tooth file.
// If the specifier is a requirement specifier, it should contain version.
func downloadTooth(specifier Specifier) (string, error) {
	switch specifier.SpecifierType() {
	case ToothFileSpecifierType:
		// For local tooth file, just return the path.

		// Get full path of the tooth file.
		toothFilePath, err := filepath.Abs(specifier.ToothPath())
		if err != nil {
			return "", errors.New("cannot get full path of tooth file: " + specifier.ToothPath())
		}

		return toothFilePath, nil

	case ToothURLSpecifierType:
		// For tooth url, download the tooth file and return the path.

		cacheFileName := localfile.GetCachedToothFileName(specifier.String())

		// Directly return the cached tooth file path if it exists.
		isCacheExist, err := localfile.IsCachedToothFileExist(cacheFileName)
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
		isCacheExist, err := localfile.IsCachedToothFileExist(cacheFileName)
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
		url := context.Goproxy + "/" + specifier.ToothRepo() + "/@v/v" + specifier.ToothVersion().String() + ".zip"

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

	// Get the version list.
	resp, err := http.Get(url)
	if err != nil {
		return nil, errors.New("cannot access GOPROXY: " + repoPath)
	}
	defer resp.Body.Close()

	if resp.StatusCode != 200 {
		return nil, errors.New("cannot access tooth repository: " + repoPath)
	}

	// Each line is a version.
	var versionList []versionutils.Version
	scanner := bufio.NewScanner(resp.Body)
	for scanner.Scan() {
		version, err := versionutils.NewFromString(strings.TrimPrefix(scanner.Text(), "v"))
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

// fetchLatestVersion fetches the latest version of the tooth repository.
func fetchLatestVersion(repoPath string) (versionutils.Version, error) {
	if !isValidRepoPath(repoPath) {
		return versionutils.Version{}, errors.New("invalid repository path: " + repoPath)
	}

	url := context.Goproxy + "/" + repoPath + "/@latest"

	// Get the latest version.
	resp, err := http.Get(url)
	if err != nil {
		return versionutils.Version{}, errors.New("cannot access GOPROXY: " + repoPath)
	}
	defer resp.Body.Close()

	if resp.StatusCode != 200 {
		return versionutils.Version{}, errors.New("cannot access tooth repository: " + repoPath)
	}

	// Parse as JSON.
	var data interface{}
	err = json.NewDecoder(resp.Body).Decode(&data)

	// If the response is not JSON, it is an invalid requirement specifier.
	if err != nil {
		return versionutils.Version{}, errors.New("invalid JSON response: " + repoPath)
	}

	// If the response is JSON, the latest version is the value of the key "Version".
	versionString := data.(map[string]interface{})["Version"].(string)

	// The version should not start with v0.0.0.
	if strings.HasPrefix(versionString, "v0.0.0-") {
		return versionutils.Version{}, errors.New("cannot find a stable latest version: " + repoPath)
	}

	// Remove the prefix v.
	versionString = strings.TrimPrefix(versionString, "v")

	// Parse the version.
	version, err := versionutils.NewFromString(versionString)
	if err != nil {
		return versionutils.Version{}, err
	}

	return version, nil
}

// isValidRepoPath checks if the repoPath is valid.
func isValidRepoPath(repoPath string) bool {
	reg := regexp.MustCompile(`^[a-z0-9][a-z0-9-_\.\/]*$`)

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
	url := context.Goproxy + "/" + repoPath + "/@v/v" + version.String() + ".info"

	// Get the version information.
	resp, err := http.Get(url)
	if err != nil {
		return errors.New("cannot access GOPROXY: " + repoPath)
	}
	defer resp.Body.Close()

	// If the status code is 200, the version is valid.
	if resp.StatusCode != 200 {
		return errors.New("cannot access tooth repository: " + repoPath)
	}

	return nil
}
