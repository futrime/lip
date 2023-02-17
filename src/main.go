package main

import (
	"os"
	"os/exec"
	"runtime"

	cmdlip "github.com/liteldev/lip/cmd"
	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/utils/logger"
)

func main() {
	var err error

	// Initialize the ~/.lip and ./.lip directories.
	err = localfile.Init()
	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}

	// Change the working directory to the project root.
	workspaceDir, err := localfile.WorkspaceDir()
	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}
	err = os.Chdir(workspaceDir)
	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}

	// Attempt to execute .lip/tools/lip or .lip/tools/lip.exe if it exists.
	if os.Getenv("LIP_REDIRECTED") == "" { // Prevent infinite redirection.
		lipExeName := "lip"
		if runtime.GOOS == "windows" {
			lipExeName = "lip.exe"
		}

		// Remove {lipExeName} and lip.remove if they exist.
		if _, err := os.Stat(".lip/tools/lip/lip.remove"); err == nil {
			logger.Info("Removing .lip/tools/lip/" + lipExeName + " and .lip/tools/lip/lip.remove")
			err = os.Remove(".lip/tools/lip/" + lipExeName)
			if err != nil {
				logger.Error("failed to remove old Lip version: " + err.Error())
				os.Exit(1)
			}
			err = os.Remove(".lip/tools/lip/lip.remove")
			if err != nil {
				logger.Error("failed to remove old Lip version: " + err.Error())
				os.Exit(1)
			}
		}

		// Move lip.update to {lipExeName} if it exists.
		if _, err := os.Stat(".lip/tools/lip/lip.update"); err == nil {
			logger.Info("Moving .lip/tools/lip/lip.update to .lip/tools/lip/" + lipExeName)

			// Remove the old {lipExeName} if it exists.
			if _, err := os.Stat(".lip/tools/lip/" + lipExeName); err == nil {
				err = os.Remove(".lip/tools/lip/" + lipExeName)
				if err != nil {
					logger.Error("failed to remove old Lip version: " + err.Error())
					os.Exit(1)
				}
			}

			// Move the new {lipExeName}.
			err = os.Rename(".lip/tools/lip/lip.update", ".lip/tools/lip/"+lipExeName)
			if err != nil {
				logger.Error("failed to move new Lip version: " + err.Error())
				os.Exit(1)
			}
		}

		if _, err := os.Stat(".lip/tools/lip/" + lipExeName); err == nil {
			logger.Info("Redirecting to .lip/tools/lip/" + lipExeName)
			cmd := exec.Command(".lip/tools/lip/"+lipExeName, os.Args[1:]...)
			cmd.Env = append(os.Environ(), "LIP_REDIRECTED=1")
			cmd.Stdout = os.Stdout
			cmd.Stderr = os.Stderr
			cmd.Stdin = os.Stdin
			err = cmd.Run()
			if err != nil {
				logger.Error("redirection failed, falling back: " + err.Error())
				cmdlip.Run(os.Args[1:])
				return
			}
			return
		}
	}

	cmdlip.Run(os.Args[1:])
}
