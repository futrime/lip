package cmdlipinstall

import (
	"container/list"
	"flag"
	"os"
	"path/filepath"

	cmdlipuninstall "github.com/liteldev/lip/cmd/uninstall"
	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/tooth/toothfile"
	"github.com/liteldev/lip/tooth/toothrecord"
	"github.com/liteldev/lip/utils/logger"
	versionutils "github.com/liteldev/lip/utils/version"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag           bool
	upgradeFlag        bool
	forceReinstallFlag bool
}

const helpMessage = `
Usage:
  lip install [options] <requirement specifiers>
  lip install [options] <tooth url/files>

Description:
  Install teeth from:

  - tooth repositories.
  - local or remote standalone tooth files (with suffix .tth).

Options:
  -h, --help                  Show help.
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

	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")

	flagSet.BoolVar(&flagDict.upgradeFlag, "upgrade", false, "")

	flagSet.BoolVar(&flagDict.forceReinstallFlag, "force-reinstall", false, "")

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

		// If the tooth file of the specifier is already downloaded, skip.
		if _, ok := downloadedToothFiles[specifier.String()]; ok {
			continue
		}

		logger.Info("  Fetching " + specifier.String() + "...")

		// Download the tooth file.
		downloadedToothFilePath, err := downloadTooth(specifier)
		if err != nil {
			logger.Error(err.Error())
			return
		}

		// Add the downloaded path to the downloaded tooth files.
		downloadedToothFiles[specifier.String()] = downloadedToothFilePath

		// Parse the tooth file
		toothFile, err := toothfile.New(downloadedToothFilePath)
		if err != nil {
			logger.Error(err.Error())
			return
		}

		// Validate the tooth file.
		toothPath := toothFile.Metadata().ToothPath
		if specifier.specifierType == RequirementSpecifierType &&
			toothPath != specifier.ToothRepo() {
			logger.Error("the tooth path of the downloaded tooth file does not match the requirement specifier")

			// Remove the downloaded tooth file.
			err = os.Remove(downloadedToothFilePath)
			if err != nil {
				logger.Error("failed to remove the downloaded tooth file: " + err.Error())
			}
			return
		}

		dependencies := toothFile.Metadata().Dependencies

		// Get proper version of each dependency and add them to the queue.
		for toothPath, versionRange := range dependencies {
			versionList, err := FetchVersionList(toothPath)
			if err != nil {
				logger.Error(err.Error())
				return
			}

			isMatched := false
		selectVersion:
			for _, version := range versionList {
				for _, innerVersionRange := range versionRange {
					for _, versionMatch := range innerVersionRange {
						if versionMatch.Match(version) {
							// Add the specifier to the queue.
							specifier, err := NewSpecifier(toothPath + "@" + version.String())
							if err != nil {
								logger.Error(err.Error())
								return
							}
							specifiersToFetch.PushBack(specifier)
							isMatched = true
							break selectVersion
						}
					}
				}
			}

			if !isMatched {
				// If no version is selected, error.
				logger.Error("no version of " + toothPath + " matches the requirement of " + specifier.String())
				return
			}
		}
	}

	// 3. Deal with force reinstall flag and upgrade flag.
	//    This process will check if the force reinstall flag is set. If it is set, all
	//    installed tooth specified by the specifiers will be reinstalled. If it is not
	//    set, it will check if the upgrade flag is set. If it is set, all installed tooth
	//    specified by the specifiers will be upgraded. If it is not set, all installed
	//    tooth specified by the specifiers will be skipped.

	if flagDict.forceReinstallFlag || flagDict.upgradeFlag {
		if flagDict.forceReinstallFlag {
			logger.Info("force-reinstall flag is set, reinstalling all installed tooth specified by the specifiers...")
		} else {
			logger.Info("upgrade flag is set, upgrading all installed tooth specified by the specifiers...")
		}

		for _, specifier := range specifiers {
			// If the specifier is not a requirement specifier, skip.
			if specifier.Type() != RequirementSpecifierType {
				continue
			}

			toothFile, err := toothfile.New(downloadedToothFiles[specifier.String()])
			if err != nil {
				logger.Error(err.Error())
				return
			}

			// If the tooth file of the specifier is not installed, skip.
			isInstalled, err := toothrecord.IsToothInstalled(toothFile.Metadata().ToothPath)
			if err != nil {
				logger.Error(err.Error())
				return
			}
			if !isInstalled {
				continue
			}

			// Get the tooth record.
			recordDir, err := localfile.RecordDir()
			if err != nil {
				logger.Error(err.Error())
				return
			}

			toothRecordFilePath := filepath.Join(recordDir, localfile.GetRecordFileName(toothFile.Metadata().ToothPath))
			toothRecord, err := toothrecord.New(toothRecordFilePath)
			if err != nil {
				logger.Error(err.Error())
				return
			}

			// Compare the version of the tooth file and the version of the tooth record.
			// If the version of the tooth file is not greater than the version of the tooth
			// record, skip.
			if !flagDict.forceReinstallFlag && !versionutils.GreaterThan(toothFile.Metadata().Version, toothRecord.Version) {
				continue
			}

			// If the tooth file of the specifier is installed, uninstall it.
			logger.Info("  Uninstalling " + toothFile.Metadata().ToothPath + "...")

			possessionList := toothFile.Metadata().Possession
			recordFileName := localfile.GetRecordFileName(toothFile.Metadata().ToothPath)

			err = cmdlipuninstall.Uninstall(recordFileName, possessionList)
			if err != nil {
				logger.Error(err.Error())
				return
			}
		}

	}

	// 4. Install tooth files.
	//    This process will install all downloaded tooth files. If the tooth file is
	//    already installed, it will be skipped. If the tooth file is not installed, it
	//    will be installed. If the tooth file is installed but the version is different,
	//    it will be upgraded or reinstalled according to the flags.

	logger.Info("Installing tooth files...")

	for _, downloadedToothFilePath := range downloadedToothFiles {

		// Open the tooth file.
		toothFile, err := toothfile.New(downloadedToothFilePath)
		if err != nil {
			logger.Error(err.Error())
			return
		}

		// If the tooth file is already installed, skip.
		isInstalled, err := toothrecord.IsToothInstalled(toothFile.Metadata().ToothPath)
		if err != nil {
			logger.Error(err.Error())
			return
		}
		if isInstalled {
			logger.Info("  " + toothFile.Metadata().ToothPath + " (" + downloadedToothFilePath + ") is already installed.")
			continue
		}

		// Install the tooth file.
		logger.Info("  Installing " + toothFile.Metadata().ToothPath + "@" +
			toothFile.Metadata().Version.String() + "...")

		// TODO: Check if the tooth file is manually installed.
		isManuallyInstalled := false

		err = install(toothFile, isManuallyInstalled)
		if err != nil {
			logger.Error(err.Error())
			return
		}
	}

	logger.Info("Successfully installed all tooth files.")
}
