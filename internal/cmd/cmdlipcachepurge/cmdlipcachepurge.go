package cmdlipcachepurge

import (
	"flag"
	"fmt"
	"os"

	"github.com/lippkg/lip/internal/context"
)

type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip cache purge [options]

Description:
  Remove all items from the cache.

Options:
  -h, --help                  Show help.
`

func Run(ctx context.Context, args []string) error {
	flagSet := flag.NewFlagSet("purge", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")

	if err := flagSet.Parse(args); err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		fmt.Print(helpMessage)
		return nil
	}

	// Check if there are unexpected arguments.
	if flagSet.NArg() != 0 {
		return fmt.Errorf("unexpected arguments: %v", flagSet.Args())
	}

	// Purge the cache.

	if err := purgeCache(ctx); err != nil {
		return fmt.Errorf("failed to purge the cache: %w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// purgeCache removes all items from the cache.
func purgeCache(ctx context.Context) error {
	var err error

	cacheDir, err := ctx.CacheDir()
	if err != nil {
		return fmt.Errorf("failed to get the cache directory: %w", err)
	}

	// Remove the cache directory.
	err = os.RemoveAll(cacheDir.LocalString())
	if err != nil {
		return fmt.Errorf("failed to remove the cache directory: %w", err)
	}

	// Recreate the cache directory.
	err = os.MkdirAll(cacheDir.LocalString(), 0755)
	if err != nil {
		return fmt.Errorf("failed to recreate the cache directory: %w", err)
	}

	return nil
}
