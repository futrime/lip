package installing

import (
	"fmt"
	"os"
	"os/exec"
	"runtime"
)

// runCommands runs the given commands.
func runCommands(commands []string) error {
	var err error

	for _, command := range commands {
		var cmd *exec.Cmd
		switch runtime.GOOS {
		case "windows":
			cmd = exec.Command("cmd", "/C", command)
		default:
			cmd = exec.Command("sh", "-c", command)
		}
		cmd.Stdin = os.Stdin
		cmd.Stdout = os.Stdout
		cmd.Stderr = os.Stderr
		err = cmd.Run()
		if err != nil {
			return fmt.Errorf("failed to run pre-uninstall command: %w", err)
		}
	}

	return nil
}
