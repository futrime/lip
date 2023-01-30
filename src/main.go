package main

import (
	"os"
	"os/exec"
	"runtime"
	"strings"

	cmdlip "github.com/liteldev/lip/cmd"
	"github.com/liteldev/lip/context"
	"github.com/liteldev/lip/localfile"
	"github.com/liteldev/lip/utils/logger"
	"github.com/liteldev/lip/utils/version"
)

func main() {
	// Set Version.
	var err error
	context.VersionString = strings.TrimPrefix(context.VersionString, "v")
	context.Version, err = version.NewFromString(context.VersionString)
	if err != nil {
		context.Version, _ = version.NewFromString("0.0.0")
	}

	// Set Goproxy if environment variable GOPROXY is set.
	if goproxy := os.Getenv("GOPROXY"); goproxy != "" {
		context.Goproxy = goproxy
	} else {
		context.Goproxy = context.DefaultGoproxy
	}

	// Initialize the ~/.lip and ./.lip directories.
	err = localfile.Init()
	if err != nil {
		logger.Error(err.Error())
	}

	// Change the working directory to the project root.
	workspaceDir, err := localfile.WorkSpaceDir()
	if err != nil {
		logger.Error(err.Error())
	}
	err = os.Chdir(workspaceDir)
	if err != nil {
		logger.Error(err.Error())
	}

	// Attempt to execute .lip/tools/lip or .lip/tools/lip.exe if it exists.
	if os.Getenv("LIP_REDIRECTED") == "" {
		lipExeName := "lip"
		if runtime.GOOS == "windows" {
			lipExeName = "lip.exe"
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
				logger.Error(err.Error())
				return
			}
			return
		}
	}

	cmdlip.Run()
}
