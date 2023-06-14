package main

import (
	"os"
	"path/filepath"
	"strings"

	"github.com/lippkg/lip/internal/cli"
	"github.com/lippkg/lip/internal/contexts"
	"github.com/lippkg/lip/internal/logging"
	"github.com/lippkg/lip/internal/paths"
	"github.com/lippkg/lip/internal/versions"
)

//------------------------------------------------------------------------------
// Configurations

const DefaultGoproxy = "https://goproxy.io"

const LipVersionString = "v0.0.0"

//------------------------------------------------------------------------------

func main() {
	var err error

	userHomeDir, err := os.UserHomeDir()
	if err != nil {
		logging.Error("cannot get user home directory: " + err.Error())
		return
	}

	globalDotLipDir := filepath.Join(userHomeDir, ".lip")
	globalDotLipDir, err = paths.Regularize(globalDotLipDir)
	if err != nil {
		logging.Error("cannot regularize global .lip directory: " + err.Error())
		return
	}

	goProxyList := []string{DefaultGoproxy}
	if goProxyEnvVar := os.Getenv("GOPROXY"); goProxyEnvVar != "" {
		goProxyList = strings.Split(goProxyEnvVar, ",")
	}

	lipVersion, err := versions.NewFromString(strings.TrimPrefix(LipVersionString, "v"))
	if err != nil {
		logging.Error("cannot parse Lip version: " + err.Error())
		return
	}

	workspaceDir, err := os.Getwd()
	if err != nil {
		logging.Error("cannot get workspace directory: " + err.Error())
		return
	}
	workspaceDir, err = paths.Regularize(workspaceDir)
	if err != nil {
		logging.Error("cannot regularize workspace directory: " + err.Error())
		return
	}

	context := contexts.New(globalDotLipDir, goProxyList, lipVersion, workspaceDir)

	err = cli.Run(context)
	if err != nil {
		logging.Error(err.Error())
		return
	}
}
