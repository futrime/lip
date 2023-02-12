package cmdlipshow

import (
	"encoding/json"
	"flag"
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/registry"
	"github.com/liteldev/lip/tooth/toothrecord"
	"github.com/liteldev/lip/tooth/toothrepo"
	"github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag  bool
	filesFlag bool
	jsonFlag  bool
}

const helpMessage = `
Usage:
  lip show [options] <tooth path>

Description:
  Show information about a installed tooth. If not installed, only version list is shown.

Options:
  -h, --help                  Show help.
  --files                     Show the full list of installed files.
  --json                      Output in JSON format. (cannot be hidden with "--quiet")`

// Run is the entry point.
func Run(args []string) {
	var err error

	if len(args) == 0 {
		logger.Info(helpMessage)
		return
	}

	flagSet := flag.NewFlagSet("list", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.filesFlag, "files", false, "")
	flagSet.BoolVar(&flagDict.jsonFlag, "json", false, "")
	flagSet.Parse(args)

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// The tooth path should not be empty or more than one.
	if len(flagSet.Args()) == 0 ||
		len(flagSet.Args()) > 1 {
		logger.Error("the tooth path should be exactly one")
		os.Exit(1)
	}

	// Get the record file path.
	// If the input is an alias, convert it to the repo path.
	toothPath := strings.ToLower(flagSet.Args()[0])
	if !strings.Contains(toothPath, "/") {
		toothPath, err = registry.LookupAlias(toothPath)
		if err != nil {
			logger.Error(err.Error())
			os.Exit(1)
		}
		logger.Info("The alias is converted to the repo path: " + toothPath)
	}

	recordFileName := localfile.GetRecordFileName(toothPath)
	recordDir, err := localfile.RecordDir()
	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}
	recordFilePath := filepath.Join(recordDir, recordFileName)

	outputJSONMap := map[string]interface{}{}

	// Check if the record file exists.
	if _, err := os.Stat(recordFilePath); os.IsNotExist(err) {
		logger.Info("The tooth is not installed")
		logger.Info("")
	} else {
		// Get the record file content.
		recordObject, err := toothrecord.New(recordFilePath)
		if err != nil {
			logger.Error(err.Error())
			os.Exit(1)
		}

		// Show information.
		logger.Info("Tooth information:")
		logger.Info("  Tooth-path: " + recordObject.ToothPath)
		logger.Info("  Version: " + recordObject.Version.String())
		logger.Info("  Name: " + recordObject.Information.Name)
		logger.Info("  Description: " + recordObject.Information.Description)
		logger.Info("  Author: " + recordObject.Information.Author)
		logger.Info("  License: " + recordObject.Information.License)
		logger.Info("  Homepage: " + recordObject.Information.Homepage)
		logger.Info("")

		// Save to JSON map.
		outputJSONMap["tooth"] = recordObject.ToothPath
		outputJSONMap["version"] = recordObject.Version.String()
		outputJSONMap["name"] = recordObject.Information.Name
		outputJSONMap["description"] = recordObject.Information.Description
		outputJSONMap["author"] = recordObject.Information.Author
		outputJSONMap["license"] = recordObject.Information.License
		outputJSONMap["homepage"] = recordObject.Information.Homepage

		// Show the full list of installed files if the files flag is set.
		if flagDict.filesFlag {
			logger.Info("Installed files:")

			outputJSONMap["files"] = []string{}

			for _, placement := range recordObject.Placement {
				logger.Info("  " + placement.Destination)

				// Save to JSON map.
				outputJSONMap["files"] = append(outputJSONMap["files"].([]string), placement.Destination)
			}

			logger.Info("")
		}
	}

	logger.Info("Fetching available versions...")

	// Show version information
	versionList, err := toothrepo.FetchVersionList(toothPath)
	if err != nil {
		logger.Error("failed to fetch available versions: " + err.Error())
		os.Exit(1)
	}

	logger.Info("Available versions:")
	versionListString := ""
	outputJSONMap["versions"] = []string{}
	for _, version := range versionList {
		versionListString += "  " + version.String()

		// Save to JSON map.
		outputJSONMap["versions"] = append(outputJSONMap["versions"].([]string), version.String())
	}
	logger.Info(versionListString)
	logger.Info("")

	// Output in JSON format.
	if flagDict.jsonFlag {
		outputJSON, _ := json.Marshal(outputJSONMap)
		fmt.Println(string(outputJSON))
	}
}
