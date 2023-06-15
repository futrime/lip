package main

import (
	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/logging"
	"github.com/lippkg/lip/pkg/plugins"
)

type PluginStruct struct{}

func (p PluginStruct) Name() string {
	return "HelloWorld Plugin"
}

func (p PluginStruct) Description() string {
	return "A plugin that says hello to the world."
}

func (p PluginStruct) Init(ctx contexts.Context, apiHub plugins.APIHubInterface) error {
	return nil
}

func (p PluginStruct) Run(args []string, ctx contexts.Context,
	apiHub plugins.APIHubInterface) error {
	logging.Info("Hello, world!")
	return nil
}

var Plugin plugins.PluginInterface = &PluginStruct{}
