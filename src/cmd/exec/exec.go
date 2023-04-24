package cmdlipexec

import (
	"flag"
	"os"
	"os/exec"
	"runtime"
	"strings"

	"github.com/lippkg/lip/tooth/toothrecord"
	"github.com/lippkg/lip/utils/logger"
)

type FlagDict struct {
	helpFlag bool
	listFlag bool
}

const helpMessage = `
Usage:
  lip exec [options] <tool> [args...]

Description:
  Execute a Lip tool.

Options:
  -h, --help                  Show help.
  --list					  List all available tools.`

func Run(args []string) {
	flagSet := flag.NewFlagSet("exec", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.listFlag, "list", false, "")
	flagSet.Parse(args)

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	if flagDict.listFlag {
		if flagSet.NArg() > 0 {
			logger.Error("too many arguments.")
			os.Exit(1)
		}

		ListTools()
		return
	}

	// The tool name should not be empty.
	if len(flagSet.Args()) == 0 {
		logger.Error("missing tool name.")
		os.Exit(1)
	}

	// Run the tool.
	RunTool(flagSet.Args())
}

// ListTools lists all available tools.
func ListTools() {
	var err error

	recordList, err := toothrecord.ListAll()
	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}

	// Remove tooths that is not a tool.
	for i := 0; i < len(recordList); i++ {
		if !recordList[i].IsTool() {
			recordList = append(recordList[:i], recordList[i+1:]...)
			i--
		}
	}

	// Print table
	longestNameLength := 10        // The minimum length of the column.
	longestDescriptionLength := 20 // The minimum length of the column.
	for _, record := range recordList {
		if len(record.Tool.Name) > longestNameLength {
			longestNameLength = len(record.Tool.Name)
		}
		if len(record.Tool.Description) > longestDescriptionLength {
			longestDescriptionLength = len(record.Tool.Description)
		}
	}

	// Print header
	logger.Info("Name" + strings.Repeat(" ", longestNameLength-4) + " Description")
	logger.Info(strings.Repeat("-", longestNameLength) + " " +
		strings.Repeat("-", longestDescriptionLength))

	// Print tools
	for _, record := range recordList {
		logger.Info(record.Tool.Name +
			strings.Repeat(" ", longestNameLength-len(record.Tool.Name)) + " " +
			record.Tool.Description)
	}
}

// RunTool runs a tool.
func RunTool(args []string) {
	var err error

	toolName := args[0]
	toolArgs := args[1:]

	recordList, err := toothrecord.ListAll()
	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}

	toolPath := ""
FindCorrectToolPath:
	for _, record := range recordList {
		if !record.IsTool() || record.Tool.Name != toolName {
			continue
		}

		for _, entrypoint := range record.Tool.Entrypoints {
			if entrypoint.GOOS != runtime.GOOS {
				continue
			}

			if entrypoint.GOARCH != "" && entrypoint.GOARCH != runtime.GOARCH {
				continue
			}

			toolPath = entrypoint.Path
			break FindCorrectToolPath
		}
	}

	if toolPath == "" {
		logger.Error("tool not found.")
		os.Exit(1)
	}

	// Run the tool.
	cmd := exec.Command(toolPath, toolArgs...)
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	cmd.Stdin = os.Stdin

	err = cmd.Run()
	if err != nil {
		logger.Error("failed to run the tool: %s", err.Error())
		os.Exit(1)
	}
}
