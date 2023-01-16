package cmdliplist

import (
	"flag"
	"os"
	"strings"

	record "github.com/liteldev/lip/record"
	logger "github.com/liteldev/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip list [options]

Description:
  List installed teeth.

Options:
  -h, --help                  Show help.`

// Run is the entry point.
func Run() {
	flagSet := flag.NewFlagSet("list", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flag.BoolVar(&flagDict.helpFlag, "help", false, "")
	flag.BoolVar(&flagDict.helpFlag, "h", false, "")

	flagSet.Parse(os.Args[2:])

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// List installed teeth.
	listInstalledTeeth()
}

// listInstalledTeeth lists installed teeth.
func listInstalledTeeth() {
	// Get the sorted list of records.
	recordList, err := record.ListAll()
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
