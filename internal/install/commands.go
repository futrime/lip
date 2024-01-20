package install

import (
	"fmt"
	"os"
	"os/exec"
	"runtime"

	log "github.com/sirupsen/logrus"
)

// runCommands runs the given commands.
func runCommands(commands []string) error {
	debugLogger := log.WithFields(log.Fields{
		"package": "install",
		"method":  "runCommands",
	})

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

		if err := cmd.Run(); err != nil {
			return fmt.Errorf("failed to run command %v: %w", command, err)
		}

		debugLogger.Debugf("Ran command %v", command)
	}

	return nil
}
