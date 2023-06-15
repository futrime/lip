package cmdlipexec

import (
	"flag"
	"fmt"
	"strings"

	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/logging"
	"github.com/lippkg/lip/pkg/plugins"
	"github.com/olekukonko/tablewriter"
)

type FlagDict struct {
	helpFlag bool
	listFlag bool
}

const helpMessage = `
Usage:
  lip exec [options] <plugin> [args...]

Description:
  Execute a Lip plugin.

Options:
  -h, --help                  Show help.
  --list					  List all available plugins.
`

func Run(ctx contexts.Context, args []string) error {
	var err error

	flagSet := flag.NewFlagSet("exec", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.BoolVar(&flagDict.listFlag, "list", false, "")
	err = flagSet.Parse(args)
	if err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logging.Info(helpMessage)
		return nil
	}

	if flagDict.listFlag {
		if flagSet.NArg() != 0 {
			return fmt.Errorf("unexpected arguments: %v", flagSet.Args())
		}

		err = listPlugins(ctx)
		if err != nil {
			return fmt.Errorf("failed to list plugins: %w", err)
		}

		return nil
	}

	if flagSet.NArg() == 0 {
		return fmt.Errorf("no plugin specified")
	}

	err = execPlugin(ctx, flagSet.Args()[0], flagSet.Args()[1:])
	if err != nil {
		return fmt.Errorf("failed to execute plugin: %w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// execPlugin executes a plugin.
func execPlugin(ctx contexts.Context, pluginName string, args []string) error {
	var err error

	isExist := false
	for plName := range ctx.PluginSet() {
		if plName == pluginName {
			isExist = true
			break
		}
	}

	if !isExist {
		return fmt.Errorf("plugin %v not found", pluginName)
	}

	plug, err := plugins.OpenPlugin(ctx, pluginName)
	if err != nil {
		return fmt.Errorf("failed to open plugin %v: %w", pluginName, err)
	}

	// TODO: Make the real API Hub.
	apiHub := struct{}{}

	err = plug.Run(args, ctx, apiHub)
	if err != nil {
		return fmt.Errorf("failed to run plugin %v: %w", pluginName, err)
	}

	return nil
}

// listPlugins lists all available plugins.
func listPlugins(ctx contexts.Context) error {
	// Get plugin information.
	tableData := make([][]string, 0)
	for pluginName := range ctx.PluginSet() {
		plug, err := plugins.OpenPlugin(ctx, pluginName)
		if err != nil {
			return fmt.Errorf("failed to open plugin %v: %w", pluginName, err)
		}

		tableData = append(tableData, []string{
			pluginName, // The command name.
			plug.Name(),
			plug.Description(),
		})
	}

	tableString := &strings.Builder{}
	table := tablewriter.NewWriter(tableString)
	table.SetHeader([]string{"Command", "Name", "Description"})

	for _, v := range tableData {
		table.Append(v)
	}

	table.Render()

	logging.Info(tableString.String())

	return nil
}
