package main

import (
	"fmt"
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

	ctx, err := initContext()
	if err != nil {
		logging.Error(err.Error())
		return
	}

	err = cli.Run(ctx)
	if err != nil {
		logging.Error(err.Error())
		return
	}
}

// initContext initializes the context.
func initContext() (contexts.Context, error) {
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

	lipVersion, err := versions.NewFromString(strings.TrimPrefix(LipVersionString, "v"))
	if err != nil {
		return contexts.Context{},
			fmt.Errorf("cannot parse lip version: %w", err)
	}

	workspaceDir, err := os.Getwd()
	if err != nil {
		return contexts.Context{},
			fmt.Errorf("cannot get current working directory: %w", err)
	}

	pluginSet, err := listPlugins(filepath.Join(workspaceDir, ".lip", "plugins"))
	if err != nil {
		return contexts.Context{},
			fmt.Errorf("cannot list plugins: %w", err)
	}

	ctx := contexts.New(globalDotLipDir, goProxyList, lipVersion, pluginSet, workspaceDir)
	if err != nil {
		return contexts.Context{},
			fmt.Errorf("cannot create context: %w", err)
	}

	return ctx, nil
}

// listPlugins lists all plugins.
func listPlugins(dir string) (map[string]struct{}, error) {
	var err error

	dllExtMap := map[string]string{
		"windows": ".dll",
		"darwin":  ".dylib",
		"linux":   ".so",
	}

	matches, err := filepath.Glob(filepath.Join(dir, "*"+dllExtMap[os.Getenv("GOOS")]))
	if err != nil {
		return nil, fmt.Errorf("cannot list plugins: %w", err)
	}

	pluginSet := make(map[string]struct{})
	for _, match := range matches {
		pluginSet[match] = struct{}{}
	}

	return pluginSet, nil
}
