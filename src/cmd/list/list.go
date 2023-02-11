package cmdliplist

import (
	"flag"
	"strings"

	"github.com/liteldev/lip/specifiers"
	"github.com/liteldev/lip/tooth/toothrecord"
	"github.com/liteldev/lip/utils/logger"
	"github.com/liteldev/lip/utils/versions"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag       bool
	upgradableFlag bool
}

const helpMessage = `
Usage:
  lip list [options]

Description:
  List installed tooths.

Options:
  -h, --help                  Show help.
  --upgradable                List upgradable tooths.`

// Run is the entry point.
func Run(args []string) {
	flagSet := flag.NewFlagSet("list", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.upgradableFlag, "upgradable", false, "")

	flagSet.Parse(args)

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	if flagDict.upgradableFlag {
		// List upgradable tooths.
		listUpgradableTooths()
	} else {
		// List installed tooths.
		listInstalledTooths()
	}
}

// listInstalledTooths lists installed tooths.
func listInstalledTooths() {
	// Get the sorted list of records.
	recordList, err := toothrecord.ListAll()
	if err != nil {
		logger.Error(err.Error())
		return
	}

	// Get the longest tooth path.
	longestToothPath := 20 // The mininum length
	for _, record := range recordList {
		if len(record.ToothPath) > longestToothPath {
			longestToothPath = len(record.ToothPath)
		}
	}

	// Get the longest version string.
	longestVersionString := 10 // The mininum length
	for _, record := range recordList {
		if len(record.Version.String()) > longestVersionString {
			longestVersionString = len(record.Version.String())
		}
	}

	// Print header.
	logger.Info("Tooth" + strings.Repeat(" ", longestToothPath-5) + " Version")
	logger.Info(strings.Repeat("-", longestToothPath) + " " +
		strings.Repeat("-", longestVersionString))

	// Print records.
	for _, record := range recordList {
		logger.Info(record.ToothPath + strings.Repeat(" ", longestToothPath-len(record.ToothPath)) +
			" " + record.Version.String())
	}
}

// listUpgradableTooths lists upgradable tooths.
func listUpgradableTooths() {
	type UpgradableToothInfoType struct {
		ToothPath string
		Version   versions.Version
	}

	logger.Info("Checking for upgradable tooths... (this may take a while)")

	// Get the sorted list of records.
	recordList, err := toothrecord.ListAll()
	if err != nil {
		logger.Error(err.Error())
		return
	}

	// Get upgradable tooths.
	upgradableToothInfoList := []UpgradableToothInfoType{}
	longestToothPath := 20     // The mininum length
	longestVersionString := 20 // The mininum length
	for _, record := range recordList {
		// Get the latest version.
		specifier, err := specifiers.New(record.ToothPath)
		if err != nil {
			logger.Error("failed to get the latest version of " + record.ToothPath + ": " + err.Error())
		}
		latestVersion := specifier.ToothVersion()
		if versions.Equal(latestVersion, record.Version) {
			continue
		}

		if len(record.ToothPath) > longestToothPath {
			longestToothPath = len(record.ToothPath)
		}
		if len(latestVersion.String()) > longestVersionString {
			longestVersionString = len(latestVersion.String())
		}

		upgradableToothInfoList = append(upgradableToothInfoList, UpgradableToothInfoType{
			ToothPath: record.ToothPath,
			Version:   latestVersion,
		})
	}

	// Print header.
	logger.Info("")
	logger.Info("Tooth" + strings.Repeat(" ", longestToothPath-5) + " Latest Version")
	logger.Info(strings.Repeat("-", longestToothPath) + " " +
		strings.Repeat("-", longestVersionString))

	// Print upgradable tooth information.
	for _, upgradableToothInfo := range upgradableToothInfoList {
		logger.Info(upgradableToothInfo.ToothPath + strings.Repeat(" ", longestToothPath-len(upgradableToothInfo.ToothPath)) +
			" " + upgradableToothInfo.Version.String())
	}
}
