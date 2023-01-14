package cmdlipinstall

import (
	"container/list"
	"encoding/json"
	"errors"
	"flag"
	"net/http"
	"os"
	"path/filepath"
	"regexp"
	"strings"

	context "github.com/liteldev/lip/context"
	localfile "github.com/liteldev/lip/localfile"
	download "github.com/liteldev/lip/utils/download"
	logger "github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag           bool
	dryRunFlag         bool
	upgradeFlag        bool
	forceReinstallFlag bool
}

// SpecifierType is the type of specifier.
const (
	ToothFileSpecifierType = iota
	ToothURLSpecifierType
	RequirementSpecifierType
)

type SpecifierType int

const helpMessage = `
Usage:
  lip install [options] <requirement specifiers>
  lip install [options] <tooth url/files>

Description:
  Install a tooth from:

  - tooth repositories.
  - local or remote standalone tooth files (with suffix .tt).

Options:
  -h, --help                  Show help.
  --dry-run                   Don't actually install anything, just print what would be.
  --upgrade                   Upgrade the specified tooth to the newest available version.
  --force-reinstall           Reinstall the tooth even if they are already up-to-date.`

// Run is the entry point.
func Run() {
	// Validate the context.
	if err := context.Validate(); err != nil {
		logger.Error(err.Error())
		return
	}

	// If there is no argument, print help message and exit.
	if len(os.Args) == 2 {
		logger.Info(helpMessage)
		return
	}

	flagSet := flag.NewFlagSet("install", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flag.BoolVar(&flagDict.helpFlag, "help", false, "")
	flag.BoolVar(&flagDict.helpFlag, "h", false, "")

	flag.BoolVar(&flagDict.dryRunFlag, "dry-run", false, "")

	flag.BoolVar(&flagDict.upgradeFlag, "upgrade", false, "")

	flag.BoolVar(&flagDict.forceReinstallFlag, "force-reinstall", false, "")

	flagSet.Parse(os.Args[2:])

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// Get the requirement specifier or tooth url/path.
	var specifiers []string = flagSet.Args()

	// Check if the requirement specifier or tooth url/path is missing.
	if len(specifiers) == 0 {
		logger.Error("missing requirement specifier or tooth url/path")
		return
	}

	// 1. Validate the requirement specifier or tooth url/path.
	//    This process will check if the tooth file exists or the tooth url can be accessed
	//    and if the requirement specifier syntax is valid. For requirement specifier, it
	//    will also check if the tooth repository can be accessed via GOPROXY.

	logger.Info("Validating requirement specifiers and tooth url/files...")

	isAllSpecifiersValid := true
	for _, specifier := range specifiers {
		if err := validateSpecifier(specifier); err != nil {
			logger.Error(err.Error())
			isAllSpecifiersValid = false
		}
	}

	// If any specifier is invalid, exit.
	if !isAllSpecifiersValid {
		return
	}

	// 2. Parse dependency and download tooth files.
	//    This process will maintain an array of tooth files to be downloaded. For each
	//    specifier at the beginning, it will be added to the array. Then, for each
	//    specifier in the array, if it is downloaded, it will be removed from the array.
	//    If it is not downloaded, it will be parsed to get its dependencies and add them
	//    to the array. This process will continue until the array is empty.

	logger.Info("Parsing dependencies and downloading tooth files...")

	// If the specifier is a requirement specifier but missing version, fetch the
	// latest version and add it to the specifier.
	for i, specifier := range specifiers {
		if getSpecifierType(specifier) == RequirementSpecifierType &&
			!strings.Contains(specifier, "@") {
			latestVersion, err := fetchLatestVersion(specifier)
			if err != nil {
				logger.Error(err.Error())
				return
			}
			specifiers[i] = specifier + "@" + latestVersion
		}
	}

	// An array of downloaded tooth files.
	// The key is the specifier and the value is the path of the downloaded tooth file.
	downloadedToothFiles := make(map[string]string)
	// An queue of tooth files to be downloaded.
	var specifiersToFetch list.List

	// Add all specifiers to the queue.
	for _, specifier := range specifiers {
		specifiersToFetch.PushBack(specifier)
	}

	for specifiersToFetch.Len() > 0 {
		// Get the first specifier to fetch
		specifier := specifiersToFetch.Front().Value.(string)
		specifiersToFetch.Remove(specifiersToFetch.Front())

		// If the specifier is already downloaded, skip.
		if _, ok := downloadedToothFiles[specifier]; ok {
			continue
		}

		logger.Info("Fetching " + specifier + "...")

		// Download the tooth file.
		downloadedToothFilePath, err := downloadToothFile(specifier)
		if err != nil {
			logger.Error(err.Error())
			return
		}

		// Add the downloaded path to the downloaded tooth files.
		downloadedToothFiles[specifier] = downloadedToothFilePath

		// TODO: Parse the tooth file to get its dependencies and add them to the queue.
	}

	// TODO: Install the downloaded tooth files in the correct order.
}

// downloadToothFile downloads a tooth file from a tooth repository, a tooth url,
// or a local path and returns the path of the downloaded tooth file.
// If the specifier is a requirement specifier, it should contain version.
func downloadToothFile(specifier string) (string, error) {
	switch getSpecifierType(specifier) {
	case ToothFileSpecifierType:
		// For local tooth file, just return the path.

		// Get full path of the tooth file.
		toothFilePath, err := filepath.Abs(specifier)
		if err != nil {
			return "", errors.New("cannot get full path of tooth file: " + specifier)
		}

		return toothFilePath, nil

	case ToothURLSpecifierType:
		// For tooth url, download the tooth file and return the path.

		cacheFileName := localfile.GetCachedToothFileName(specifier)

		// Directly return the cached tooth file path if it exists.
		if isExist, err := localfile.IsCachedToothFileExist(cacheFileName); err != nil {
			return "", err
		} else if isExist {
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

		err = download.DownloadFile(specifier, cacheFilePath)
		if err != nil {
			return "", err
		}

		return cacheFilePath, nil

	case RequirementSpecifierType:
		// For requirement specifier, download the tooth via GOPROXY and return the path.

		cacheFileName := localfile.GetCachedToothFileName(specifier)

		// Directly return the cached tooth file path if it exists.
		if isExist, err := localfile.IsCachedToothFileExist(cacheFileName); err != nil {
			return "", err
		} else if isExist {
			cacheDir, err := localfile.CacheDir()
			if err != nil {
				return "", err
			}
			return cacheDir + "/" + cacheFileName, nil
		}

		// Get the tooth repository path and version.
		repoPath, version := parseSpecifier(specifier)

		// Get the tooth file url.
		url := context.Goproxy + "/" + repoPath + "/@v/v" + version + ".zip"

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

// fetchLatestVersion fetches the latest version of the tooth repository.
// The repoPath should be in the format of repoPath without @version.
func fetchLatestVersion(repoPath string) (string, error) {
	url := context.Goproxy + "/" + repoPath + "/@latest"

	// Get the latest version.
	resp, err := http.Get(url)
	if err != nil {
		return "", errors.New("cannot access GOPROXY: " + repoPath)
	}
	defer resp.Body.Close()

	if resp.StatusCode != 200 {
		return "", errors.New("cannot access tooth repository: " + repoPath)
	}

	// Parse as JSON.
	var data interface{}
	err = json.NewDecoder(resp.Body).Decode(&data)

	// If the response is not JSON, it is an invalid requirement specifier.
	if err != nil {
		return "", errors.New("invalid JSON response: " + repoPath)
	}

	// If the response is JSON, the latest version is the value of the key "Version".
	version := data.(map[string]interface{})["Version"].(string)

	// The version should not start with v0.0.0.
	if len(version) >= 7 && version[0:7] == "v0.0.0-" {
		return "", errors.New("cannot find a stable latest version: " + repoPath)
	}

	// Remove the prefix v.
	return version[1:], nil
}

// getSpecifierType gets the type of the requirement specifier.
func getSpecifierType(specifier string) SpecifierType {
	if strings.HasSuffix(specifier, ".tt") {
		if strings.HasPrefix(specifier, "http://") || strings.HasPrefix(specifier, "https://") {
			return ToothURLSpecifierType
		} else {
			return ToothFileSpecifierType
		}
	} else {
		return RequirementSpecifierType
	}
}

// parseSpecifier parses the requirement specifier.
// The specifier should be in the format of repoPath[@version].
func parseSpecifier(specifier string) (string, string) {
	// Split the specifier by @.
	splittedSpecifier := strings.Split(specifier, "@")

	// If the specifier contains @, the first part is the repo path and the second part is
	// the version.
	if len(splittedSpecifier) == 2 {
		return splittedSpecifier[0], splittedSpecifier[1]
	}

	// Otherwise, the repo path is the specifier itself.
	return specifier, ""
}

// validateSpecifier validates the requirement specifier or tooth url/path.
func validateSpecifier(specifier string) error {
	switch getSpecifierType(specifier) {
	case RequirementSpecifierType:
		reg := regexp.MustCompile(`^[a-z0-9-_\.\/]+(@\d+\.\d+\.\d+(-[a-z]+(\.\d+)?)?)?$`)

		// If not matched or the matched string is not the same as the specifier, it is an
		// invalid requirement specifier.
		if reg.FindString(specifier) != specifier {
			return errors.New("invalid requirement specifier syntax: " + specifier)
		}

		repoPath, version := parseSpecifier(specifier)

		// If the version is empty, fetch information of the latest version.
		if version == "" {
			var err error
			version, err = fetchLatestVersion(repoPath)
			if err != nil {
				return err
			}
		}

		// Check if the version is valid.
		url := context.Goproxy + "/" + repoPath + "/@v/v" + version + ".info"

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
	case ToothURLSpecifierType:
		// Check if the tooth url can be accessed.
		resp, err := http.Head(specifier)

		if err != nil || resp.StatusCode != 200 {
			return errors.New("cannot access tooth file url: " + specifier)
		}

		return nil
	case ToothFileSpecifierType:
		// Check if the tooth file exists.
		_, err := os.Stat(specifier)

		if err != nil {
			return errors.New("cannot access tooth file: " + specifier)
		}

		return nil
	}

	// Default to unknown error.
	return errors.New("unknown error")
}
