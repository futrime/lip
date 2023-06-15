package plugins

import (
	"fmt"
	"path/filepath"
	"plugin"
	"runtime"

	"github.com/lippkg/lip/pkg/contexts"
)

// OpenPlugin opens a plugin.
func OpenPlugin(ctx contexts.Context,
	pluginName string) (PluginInterface, error) {
	var err error

	dllExtMap := map[string]string{
		"windows": ".dll",
		"darwin":  ".dylib",
		"linux":   ".so",
	}

	pluginDir, err := ctx.PluginDir()
	if err != nil {
		return nil, fmt.Errorf("failed to get plugin directory: %w", err)
	}

	pluginDll, err := plugin.Open(
		filepath.Join(pluginDir, pluginName+dllExtMap[runtime.GOOS]))
	if err != nil {
		return nil, fmt.Errorf("failed to open plugin %v: %w", pluginName, err)
	}

	pluginSymbol, err := pluginDll.Lookup("Plugin")
	if err != nil {
		return nil, fmt.Errorf(
			"failed to lookup symbol in plugin %v: %w", pluginName, err)
	}

	pluginInterface, ok := pluginSymbol.(PluginInterface)
	if !ok {
		return nil, fmt.Errorf("invalid plugin %v", pluginName)
	}

	return pluginInterface, nil
}
