package plugins

import "github.com/lippkg/lip/pkg/contexts"

type APIHubInterface interface {
}

type PluginInterface interface {
	// Name returns the name of the plugin.
	Name() string

	// Description returns the description of the plugin.
	Description() string

	// Init initializes the plugin.
	Init(ctx contexts.Context, apiHub APIHubInterface) error

	// Run runs the plugin. args is the arguments after the plugin name.
	Run(args []string, ctx contexts.Context, apiHub APIHubInterface) error
}
