package main

import (
	"fmt"
	"os"
	"path/filepath"

	nested "github.com/antonfisher/nested-logrus-formatter"
	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/cmd/cmdlip"
	"github.com/lippkg/lip/internal/context"

	log "github.com/sirupsen/logrus"
)

//------------------------------------------------------------------------------
// Configurations

const GoModuleProxyURL = "https://goproxy.io"

const LipVersionString = "0.17.0"

//------------------------------------------------------------------------------

func main() {
	var err error

	log.SetFormatter(&nested.Formatter{})

	ctx, err := createContext()
	if err != nil {
		log.Errorf("cannot initialize context: %v", err.Error())
		return
	}

	err = cmdlip.Run(ctx, os.Args[1:])
	if err != nil {
		log.Error(err.Error())
		return
	}
}

// createContext initializes the context.
func createContext() (context.Context, error) {
	userHomeDir, err := os.UserHomeDir()
	if err != nil {
		return context.Context{},
			fmt.Errorf("cannot get user home directory: %w", err)
	}

	globalDotLipDir := filepath.Join(userHomeDir, ".lip")

	lipVersion, err := semver.Parse(LipVersionString)
	if err != nil {
		return context.Context{},
			fmt.Errorf("cannot parse lip version: %w", err)
	}

	workspaceDir, err := os.Getwd()
	if err != nil {
		return context.Context{},
			fmt.Errorf("cannot get current working directory: %w", err)
	}

	ctx := context.New(lipVersion, globalDotLipDir, workspaceDir, GoModuleProxyURL)

	return ctx, nil
}
