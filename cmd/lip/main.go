package main

import (
	"os"
	"path/filepath"
	"strings"

	"github.com/lippkg/lip/internal/cli"
	"github.com/lippkg/lip/internal/contexts"
	"github.com/lippkg/lip/internal/logging"
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
		logging.Error("cannot get user home directory: %w", err)
		return
	}

	globalDotLipDir := filepath.Join(userHomeDir, ".lip")

	goProxyList := []string{DefaultGoproxy}
	if goProxyEnvVar := os.Getenv("GOPROXY"); goProxyEnvVar != "" {
		goProxyList = strings.Split(goProxyEnvVar, ",")
	}

	lipVersion, err := versions.NewFromString(strings.TrimPrefix(LipVersionString, "v"))
	if err != nil {
		logging.Error("cannot parse Lip version: %w", err)
		return
	}

	workspaceDir, err := os.Getwd()
	if err != nil {
		logging.Error("cannot get workspace directory: %w", err)
		return
	}

	context := contexts.New(globalDotLipDir, goProxyList, lipVersion, workspaceDir)
	if err != nil {
		logging.Error("cannot create context: %w", err)
		return
	}

	err = cli.Run(context)
	if err != nil {
		logging.Error(err.Error())
		return
	}
}
