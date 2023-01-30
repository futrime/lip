package cmdlipexec

import (
	"flag"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"

	"github.com/liteldev/lip/utils/logger"
)

type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip exec [options] <tool> [args...]

Description:
  Execute a Lip tool.

Options:
  -h, --help                  Show help.`

func Run() {
	if len(os.Args) == 2 {
		logger.Info(helpMessage)
		return
	}

	flagSet := flag.NewFlagSet("exec", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict

	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")

	flagSet.Parse(os.Args[2:])

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// The tool name should not be empty.
	if len(flagSet.Args()) == 0 {
		logger.Error("missing tool name.")
		return
	}

	toolName := flagSet.Args()[0]
	toolPath := ".lip/tools/" + toolName + "/" + toolName
	toolPath = filepath.FromSlash(toolPath) // Convert to OS path.
	if runtime.GOOS == "windows" {
		if _, err := os.Stat(toolPath + ".exe"); err == nil {
			toolPath += ".exe"
		} else if _, err := os.Stat(toolPath + ".cmd"); err == nil {
			toolPath += ".cmd"

		} else {
			logger.Error("tool not found: " + toolPath)
			return
		}
	} else {
		if _, err := os.Stat(toolPath); err != nil {
			logger.Error("tool not found: " + toolPath)
			return
		}
	}

	// Execute the tool.
	var cmd *exec.Cmd
	if runtime.GOOS == "windows" && filepath.Ext(toolPath) == ".cmd" {
		// If the tool is a .cmd file, we need to use "cmd /c" to execute it.
		// Otherwise, the tool will not be able to read from stdin.
		args := []string{"/c", toolPath}
		args = append(args, flagSet.Args()[1:]...)
		cmd = exec.Command("cmd", args...)
	} else {
		cmd = exec.Command(toolPath, flagSet.Args()[1:]...)
	}
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	cmd.Stdin = os.Stdin
	err := cmd.Run()
	if err != nil {
		logger.Error("failed to run tool: " + toolName + ": " + err.Error())
		return
	}
}
