package main

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"github.com/lippkg/lip/internal/cmd/cmdlip"
	"github.com/lippkg/lip/internal/contexts"
	"github.com/lippkg/lip/internal/logging"
	"github.com/lippkg/lip/internal/versions"
)

//------------------------------------------------------------------------------
// Configurations

const DefaultGoproxy = "https://goproxy.io"

const LipVersionString = "0.15.2"

//------------------------------------------------------------------------------

func main() {
	var err error

	ctx, err := createContext()
	if err != nil {
		logging.Error("cannot initialize context: %v", err.Error())
		return
	}

	err = cmdlip.Run(ctx, os.Args[1:])
	if err != nil {
		logging.Error(err.Error())
		return
	}
}

// createContext initializes the context.
func createContext() (contexts.Context, error) {
	userHomeDir, err := os.UserHomeDir()
	if err != nil {
		return contexts.Context{},
			fmt.Errorf("cannot get user home directory: %w", err)
	}

	globalDotLipDir := filepath.Join(userHomeDir, ".lip")

	goProxyList := []string{DefaultGoproxy}
	if goProxyEnvVar := os.Getenv("GOPROXY"); goProxyEnvVar != "" {
		goProxyList = strings.Split(goProxyEnvVar, ",")
	}

	lipVersion, err := versions.NewFromString(LipVersionString)
	if err != nil {
		return contexts.Context{},
			fmt.Errorf("cannot parse lip version: %w", err)
	}

	workspaceDir, err := os.Getwd()
	if err != nil {
		return contexts.Context{},
			fmt.Errorf("cannot get current working directory: %w", err)
	}

	ctx := contexts.New(lipVersion, globalDotLipDir, workspaceDir, goProxyList)

	return ctx, nil
}
