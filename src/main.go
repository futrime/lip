package main

import (
	"os"
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

	cmdlip.Run()
}
