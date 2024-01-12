package cmdliptoothinit

import (
	"bufio"
	"flag"
	"fmt"
	"os"
	"strings"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/path"
	log "github.com/sirupsen/logrus"
	"golang.org/x/mod/module"

	"github.com/lippkg/lip/internal/tooth"
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

	rawMetadata, err := tooth.MakeRawMetadata([]byte(toothJSONTemplate))
	if err != nil {
		return fmt.Errorf("failed to create rawMetadata: %w", err)
	}

	// Ask for information.
	var ans string
	scanner := bufio.NewScanner(os.Stdin)

	log.Info("What is the tooth repo path? (e.g. github.com/tooth-hub/llbds3)")
	scanner.Scan()
	ans = scanner.Text()

	if err := module.CheckPath(ans); err != nil {
		return fmt.Errorf("invalid tooth repo path: %w", err)
	}

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

	toothJSONBytes, err := rawMetadata.MarshalJSON()
	if err != nil {
		return fmt.Errorf("failed to marshal rawMetadata: %w", err)
	}

	_, err = tooth.MakeMetadata(toothJSONBytes)
	if err != nil {
		return fmt.Errorf("failed to validate rawMetadata: %w", err)
	}

	// Create tooth.json.
	workspaceDirStr, err := os.Getwd()
	if err != nil {
		return fmt.Errorf("failed to get workspace directory: %w", err)
	}

	workspaceDir, err := path.Parse(workspaceDirStr)
	if err != nil {
		return fmt.Errorf("failed to parse workspace directory: %w", err)
	}

	file, err := os.Create(workspaceDir.Join(path.MustParse("tooth.json")).LocalString())
	if err != nil {
		return fmt.Errorf("failed to create tooth.json: %w", err)
	}
	defer file.Close()

	// Write default tooth.json content.
	_, err = file.Write(toothJSONBytes)
	if err != nil {
		return fmt.Errorf("failed to write tooth.json: %w", err)
	}

	log.Info("Successfully initialized a new tooth.")

	return nil
}
