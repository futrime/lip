package cmdlipcachepurge

import (
	"fmt"
	"os"

	"github.com/lippkg/lip/internal/context"

	"github.com/urfave/cli/v2"
)

func Command(ctx *context.Context) *cli.Command {
	return &cli.Command{
		Name:        "purge",
		Usage:       "clear the cache",
		Description: "Remove all items from the cache.",
		Action: func(cCtx *cli.Context) error {
			if cCtx.NArg() != 0 {
				return fmt.Errorf("unexpected arguments: %v", cCtx.Args())
			}

			if err := purgeCache(ctx); err != nil {
				return fmt.Errorf("failed to purge the cache\n\t%w", err)
			}

			return nil
		},
	}
}

// ---------------------------------------------------------------------

// purgeCache removes all items from the cache.
func purgeCache(ctx *context.Context) error {
	cacheDir, err := ctx.CacheDir()
	if err != nil {
		return fmt.Errorf("failed to get the cache directory\n\t%w", err)
	}

	// Remove the cache directory.
	if err := os.RemoveAll(cacheDir.LocalString()); err != nil {
		return fmt.Errorf("failed to remove the cache directory\n\t%w", err)
	}

	// Recreate the cache directory.
	if err := os.MkdirAll(cacheDir.LocalString(), 0755); err != nil {
		return fmt.Errorf("failed to recreate the cache directory\n\t%w", err)
	}

	return nil
}
