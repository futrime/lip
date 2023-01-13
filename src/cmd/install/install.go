package cmdlipinstall

import (
	"encoding/json"
	"flag"
	"net/http"
	"os"
	"regexp"
	"strings"

	"github.com/liteldev/lip/context"
	logger "github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag           bool
	dryRunFlag         bool
	upgradeFlag        bool
	forceReinstallFlag bool
}

const helpMessage = `
Usage:
  lip install [options] <requirement specifiers>
  lip install [options] <tooth url/paths>

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

	logger.Info("Validating requirement specifiers and tooth url/paths...")

	// Validate all spedifiers.
	isAllSpecifiersValid := true
	for _, specifier := range specifiers {
		if !validateSpecifier(specifier) {
			isAllSpecifiersValid = false
		}
	}

	// If any specifier is invalid, exit.
	if !isAllSpecifiersValid {
		return
	}

	// Default to unknown error.
	logger.Error("unknown error")
}

// validateSpecifier validates the requirement specifier or tooth url/path.
func validateSpecifier(specifier string) bool {
	if len(specifier) >= 3 && specifier[len(specifier)-3:] == ".tt" {
		// If the specifier ends with .tt, it is a tooth url/path.
		if (len(specifier) >= 7 && specifier[0:7] == "http://") ||
			(len(specifier) >= 8 && specifier[0:8] == "https://") {
			// If the specifier starts with http:// or https://, it is a tooth url.

			// Check if the tooth url can be accessed.
			_, err := http.Head(specifier)

			if err != nil {
				logger.Error("cannot access tooth url: " + specifier)
				return false
			}
			return true
		} else {
			// Otherwise, it is a tooth path.

			// Check if the tooth path exists.
			_, err := os.Stat(specifier)

			if err != nil {
				logger.Error("cannot access tooth path: " + specifier)
				return false
			}
			return true
		}
	} else {
		// Otherwise, it is a requirement specifier.

		reg := regexp.MustCompile(`^[a-z0-9-_\.\/]+(@\d+\.\d+\.\d+(-[a-z]+(\.\d+)?)?)?$`)

		// If not matched or the matched string is not the same as the specifier, it is an
		// invalid requirement specifier.
		if reg.FindString(specifier) != specifier {
			logger.Error("invalid requirement specifier syntax: " + specifier)
			return false
		}

		repoPath, version := parseSpecifier(specifier)

		// If the version is empty, fetch information of the latest version.
		if version == "" {
			url := context.Goproxy + "/" + repoPath + "/@latest"

			// Get the latest version.
			resp, err := http.Get(url)
			if err != nil {
				logger.Error("cannot access GOPROXY: " + repoPath)
				return false
			}
			defer resp.Body.Close()

			if resp.StatusCode != 200 {
				logger.Error("cannot access tooth path: " + repoPath)
				return false
			}

			// Parse as JSON.
			var data interface{}
			err = json.NewDecoder(resp.Body).Decode(&data)

			// If the response is not JSON, it is an invalid requirement specifier.
			if err != nil {
				logger.Error("invalid JSON response: " + repoPath)
				return false
			}

			// If the response is JSON, the latest version is the value of the key "Version".
			version = data.(map[string]interface{})["Version"].(string)

			// The version should not start with v0.0.0.
			if len(version) >= 7 && version[0:7] == "v0.0.0-" {
				logger.Error("cannot find a stable latest version: " + repoPath)
				return false
			}

			// Remove the prefix v.
			version = version[1:]
		}

		// Check if the version is valid.
		url := context.Goproxy + "/" + repoPath + "/@v/" + version + ".info"

		// Get the version information.
		resp, err := http.Get(url)
		if err != nil {
			logger.Error("cannot access GOPROXY: " + repoPath)
			return false
		}
		defer resp.Body.Close()

		// If the status code is 200, the version is valid.
		if resp.StatusCode != 200 {
			logger.Error("cannot access tooth path: " + repoPath)
			return false
		}
		return true
	}

	// Never reach here.
}

// parseSpecifier parses the requirement specifier.
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
