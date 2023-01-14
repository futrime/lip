package main

import (
	"os"

	cmdlip "github.com/liteldev/lip/cmd"
	context "github.com/liteldev/lip/context"
	localfile "github.com/liteldev/lip/localfile"
	logger "github.com/liteldev/lip/utils/logger"
)

func main() {
	// Set Goproxy if environment variable GOPROXY is set.
	if goproxy := os.Getenv("GOPROXY"); goproxy != "" {
		context.Goproxy = goproxy
	} else {
		context.Goproxy = context.DefaultGoproxy
	}

	// Initialize the ~/.lip and ./.lip directories.
	err := localfile.Init()
	if err != nil {
		logger.Error(err.Error())
	}

	cmdlip.Run()
}
