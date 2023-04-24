package cmdlipautoremove

import (
	"flag"
	"os"

	cmdlipuninstall "github.com/lippkg/lip/cmd/uninstall"
	"github.com/lippkg/lip/localfile"
	"github.com/lippkg/lip/tooth/toothrecord"
	"github.com/lippkg/lip/utils/logger"
)

type FlagDict struct {
	helpFlag           bool
	yesFlag            bool
	keepPossessionFlag bool
}

const helpMessage = `
Usage:
  lip autoremove [options]

Description:
  Uninstall tooths that are not depended by any other tooths.

Options:
  -h, --help                  Show help.
  -y, --yes                   Skip confirmation.
  --keep-possession           Keep files that the tooth author specified the tooth to occupy. These files are often configuration files, data files, etc.`

func Run(args []string) {
	var err error

	flagSet := flag.NewFlagSet("autoremove", flag.ExitOnError)

	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.yesFlag, "yes", false, "")
	flagSet.BoolVar(&flagDict.yesFlag, "y", false, "")
	flagSet.BoolVar(&flagDict.keepPossessionFlag, "keep-possession", false, "")
	flagSet.Parse(args)

	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	if flagSet.NArg() != 0 {
		logger.Error("Too many arguments.")
		os.Exit(1)
	}

	logger.Info("Discovering tooths not depended by any other tooths...")

	// 1. Gets all installed tooths.
	recordList, err := toothrecord.ListAll()
	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}

	// 2. Marks all manually installed tooths.
	toothsToKeep := make(map[string]bool)
	for _, record := range recordList {
		if record.IsManuallyInstalled {
			toothsToKeep[record.ToothPath] = true
		}
	}

	// 3. Marks all tooths that are depended by other tooths.
	markCount := 0
	for {
		for _, record := range recordList {
			if !toothsToKeep[record.ToothPath] {
				continue
			}

			for dep := range record.Dependencies {
				if !toothsToKeep[dep] {
					toothsToKeep[dep] = true
					markCount++
					break
				}
			}
		}

		if markCount == 0 {
			break
		}

		markCount = 0
	}

	logger.Info("Uninstalling tooths not depended by any other tooths...")

	// 4. Uninstalls all unmarked tooths.
	for _, record := range recordList {
		if toothsToKeep[record.ToothPath] {
			continue
		}

		logger.Info("  Uninstalling " + record.ToothPath + "...")

		possessionList := make([]string, 0)
		if flagDict.keepPossessionFlag {
			possessionList = record.Possession
		}

		recordFileName := localfile.GetRecordFileName(record.ToothPath)

		err = cmdlipuninstall.Uninstall(recordFileName, possessionList, flagDict.yesFlag)
		if err != nil {
			logger.Error(err.Error())
			os.Exit(1)
		}
	}

	logger.Info("Successfully uninstalled all tooths not depended by any other tooths.")
}
