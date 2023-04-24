package cmdliptoothinit

import (
	"bufio"
	"errors"
	"flag"
	"os"

	"github.com/lippkg/lip/tooth/toothmetadata"
	"github.com/lippkg/lip/utils/logger"
	"github.com/lippkg/lip/utils/versions"
	"github.com/lippkg/lip/utils/versions/versionmatch"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip tooth init [options]

Description:
  Initialize and writes a new tooth.json file in the current directory, in effect creating a new tooth rooted at the current directory.

Options:
  -h, --help                  Show help.`

// Run is the entry point.
func Run(args []string) {
	flagSet := flag.NewFlagSet("init", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.Parse(args)

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// No other arguments are supported.
	if flagSet.NArg() > 0 {
		logger.Error("Too many arguments.")
		os.Exit(1)
	}

	err := initTooth()

	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}

	logger.Info("tooth.json created successfully. Please edit it to complete the tooth metadata. " +
		"For more information, please visit https://lip.docs.litebds.com/en/#/tooth_json_file_reference")
}

// initTooth initializes a new tooth.
func initTooth() error {
	// Check if tooth.json already exists.
	if _, err := os.Stat("tooth.json"); err == nil {
		return errors.New("tooth.json already exists in the current directory")
	}

	version, _ := versions.New(0, 0, 0, "", 0)

	metadata := toothmetadata.Metadata{
		ToothPath:    "<NOT SPECIFIED>",
		Version:      version,
		Dependencies: make(map[string]([][]versionmatch.VersionMatch)),
		Information: toothmetadata.InfoStruct{
			Name:        "<NOT SPECIFIED>",
			Description: "<NOT SPECIFIED>",
			Author:      "<NOT SPECIFIED>",
			License:     "<NOT SPECIFIED>",
			Homepage:    "<NOT SPECIFIED>",
		},
		Placement:    make([]toothmetadata.PlacementStruct, 0),
		Possession:   make([]string, 0),
		Commands:     make([]toothmetadata.CommandStruct, 0),
		Confirmation: make([]toothmetadata.ConfirmationStruct, 0),
		Tool:         toothmetadata.ToolStruct{},
	}

	// Ask for information.
	var ans string
	scanner := bufio.NewScanner(os.Stdin)

	logger.Info("What is the tooth path? (e.g. github.com/lippkg/lip)")
	scanner.Scan()
	ans = scanner.Text()
	metadata.ToothPath = ans

	logger.Info("What is the name?")
	scanner.Scan()
	ans = scanner.Text()
	metadata.Information.Name = ans

	logger.Info("What is the description?")
	scanner.Scan()
	ans = scanner.Text()
	metadata.Information.Description = ans

	logger.Info("What is the author? Please input your GitHub username.")
	scanner.Scan()
	ans = scanner.Text()
	metadata.Information.Author = ans

	logger.Info("What is the license? (e.g. MIT) For private use, just left blank.")
	scanner.Scan()
	ans = scanner.Text()
	metadata.Information.License = ans

	logger.Info("What is the homepage? (e.g. https://lip.docs.litebds.com) Left blank if you don't have one.")
	scanner.Scan()
	ans = scanner.Text()
	metadata.Information.Homepage = ans

	toothJsonBytes, err := metadata.JSON()
	if err != nil {
		return errors.New("failed to convert tooth metadata to JSON")
	}

	metadata, err = toothmetadata.NewFromJSON(toothJsonBytes)
	if err != nil {
		return errors.New("some information is invalid: " + err.Error())
	}

	// Create tooth.json.
	file, err := os.Create("tooth.json")
	if err != nil {
		return errors.New("failed to create tooth.json")
	}

	// Write default tooth.json content.
	_, err = file.WriteString(string(toothJsonBytes))
	if err != nil {
		return errors.New("failed to write tooth.json")
	}

	return nil
}
