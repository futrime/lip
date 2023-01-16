package cmdlipinstall

import (
	"container/list"
	"flag"
	"os"

	"github.com/liteldev/lip/toothfile"
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

	// 1. Validate the requirement specifier or tooth url/path.
	//    This process will check if the tooth file exists or the tooth url can be accessed
	//    and if the requirement specifier syntax is valid. For requirement specifier, it
	//    will also check if the tooth repository can be accessed via GOPROXY.

	logger.Info("Validating requirement specifiers and tooth url/files...")

	// Make specifiers.
	var specifiers []Specifier
	for _, specifierString := range flagSet.Args() {
		specifier, err := NewSpecifier(specifierString)
		if err != nil {
			logger.Error(err.Error())
			return
		}

		specifiers = append(specifiers, specifier)
	}

	// Check if the requirement specifier or tooth url/path is missing.
	if len(specifiers) == 0 {
		logger.Error("missing requirement specifier or tooth url/path")
		return
	}

	// 2. Parse dependency and download tooth files.
	//    This process will maintain an array of tooth files to be downloaded. For each
	//    specifier at the beginning, it will be added to the array. Then, for each
	//    specifier in the array, if it is downloaded, it will be removed from the array.
	//    If it is not downloaded, it will be parsed to get its dependencies and add them
	//    to the array. This process will continue until the array is empty.

	logger.Info("Parsing dependencies and downloading tooth files...")

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
		specifier := specifiersToFetch.Front().Value.(Specifier)
		specifiersToFetch.Remove(specifiersToFetch.Front())

		// If the specifier is already downloaded, skip.
		if _, ok := downloadedToothFiles[specifier.String()]; ok {
			continue
		}

		logger.Info("Fetching " + specifier.String() + "...")

		// Download the tooth file.
		downloadedToothFilePath, err := downloadTooth(specifier)
		if err != nil {
			logger.Error(err.Error())
			return
		}

		// Add the downloaded path to the downloaded tooth files.
		downloadedToothFiles[specifier.String()] = downloadedToothFilePath

		// TODO: Parse the tooth file to get its dependencies and add them to the queue.
	}

	// 3. Install tooth files.
	//    This process will install all downloaded tooth files. If the tooth file is
	//    already installed, it will be skipped. If the tooth file is not installed, it
	//    will be installed. If the tooth file is installed but the version is different,
	//    it will be upgraded or reinstalled according to the flags.

	logger.Info("Installing tooth files...")

	for _, downloadedToothFilePath := range downloadedToothFiles {
		logger.Info("Installing " + downloadedToothFilePath + "...")

		// Open the tooth file.
		toothFile, err := toothfile.New(downloadedToothFilePath)
		if err != nil {
			logger.Error(err.Error())
			return
		}

		// Install the tooth file.
		err = toothFile.Install()
		if err != nil {
			logger.Error(err.Error())
			return
		}
	}
}
