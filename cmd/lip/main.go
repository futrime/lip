package main

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"github.com/lippkg/lip/pkg/cmd/cmdlip"
	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/logging"
	"github.com/lippkg/lip/pkg/plugins"
	"github.com/lippkg/lip/pkg/versions"
)

//------------------------------------------------------------------------------
// Configurations

const DefaultGoproxy = "https://goproxy.io"

const LipVersionString = "v0.0.0"

//------------------------------------------------------------------------------

func main() {
	var err error

	ctx, err := createContext()
	if err != nil {
		logging.Error("cannot initialize context: %v", err.Error())
		return
	}

	err = initPlugins(ctx)
	if err != nil {
		logging.Error("cannot initialize plugins: %v", err.Error())
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

// initPlugins initializes plugins.
func initPlugins(ctx contexts.Context) error {
	// TODO: Make the real API Hub.
	apiHub := struct{}{}

	for pluginName := range ctx.PluginSet() {
		plug, err := plugins.OpenPlugin(ctx, pluginName)
		if err != nil {
			return fmt.Errorf("cannot open plugin %v: %w", pluginName, err)
		}

		err = plug.Init(ctx, apiHub)
		if err != nil {
			return fmt.Errorf("cannot initialize plugin %v: %w", pluginName, err)
		}
	}

	return nil
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
