package cmdliptoothinit

import (
	"bufio"
	"errors"
	"flag"
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"github.com/lippkg/lip/internal/context"
	log "github.com/sirupsen/logrus"

	"github.com/lippkg/lip/internal/teeth"
)

type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip tooth init [options]

Description:
  Initialize and writes a new tooth.json file in the current directory, in effect creating a new tooth rooted at the current directory.

Options:
  -h, --help                  Show help.
`

const toothJSONTemplate = `{
	"format_version": 2,
	"tooth": "",
	"version": "0.0.0",
	"info": {
		"name": "",
		"description": "",
		"author": "",
		"source": "",
		"tags": []
	}
}
`

func Run(ctx context.Context, args []string) error {
	var err error

	flagSet := flag.NewFlagSet("init", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	err = flagSet.Parse(args)
	if err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		fmt.Print(helpMessage)
		return nil
	}

	// Check if there are unexpected arguments.
	if flagSet.NArg() != 0 {
		return fmt.Errorf("unexpected arguments: %v", flagSet.Args())
	}

	err = initTooth(ctx)
	if err != nil {
		return fmt.Errorf("failed to initialize the tooth: %w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// initTooth initializes a new tooth in the current directory.
func initTooth(ctx context.Context) error {
	var err error

	// Check if tooth.json already exists.
	_, err = os.Stat("tooth.json")
	if err == nil {
		return fmt.Errorf("tooth.json already exists")
	}

	rawMetadata, err := teeth.NewRawMetadata([]byte(toothJSONTemplate))
	if err != nil {
		return errors.New("failed to create a new tooth rawMetadata")
	}

	// Ask for information.
	var ans string
	scanner := bufio.NewScanner(os.Stdin)

	log.Info("What is the tooth path? (e.g. github.com/tooth-hub/llbds3)")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Tooth = ans

	// To lower case.
	rawMetadata.Tooth = strings.ToLower(rawMetadata.Tooth)

	log.Info("What is the name?")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Info.Name = ans

	log.Info("What is the description?")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Info.Description = ans

	log.Info("What is the author? Please input your GitHub username.")
	scanner.Scan()
	ans = scanner.Text()
	rawMetadata.Info.Author = ans

	toothJSONBytes, err := rawMetadata.JSON(true)
	if err != nil {
		return errors.New("failed to convert tooth rawMetadata to JSON")
	}

	_, err = teeth.NewMetadata(toothJSONBytes)
	if err != nil {
		return errors.New("some information is invalid: " + err.Error())
	}

	// Create tooth.json.
	workspaceDir, err := os.Getwd()
	if err != nil {
		return errors.New("failed to get workspace directory")
	}

	file, err := os.Create(filepath.Join(workspaceDir, "tooth.json"))
	if err != nil {
		return errors.New("failed to create tooth.json")
	}
	defer file.Close()

	// Write default tooth.json content.
	_, err = file.WriteString(string(toothJSONBytes))
	if err != nil {
		return errors.New("failed to write tooth.json")
	}

	log.Info("Successfully initialized a new tooth.")

	return nil
}
