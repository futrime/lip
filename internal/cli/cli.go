package cli

import (
	"os"

	"github.com/lippkg/lip/internal/cli/cmdlip"
	"github.com/lippkg/lip/internal/contexts"
)

func Run(context contexts.Context) error {
	err := cmdlip.Run(context, os.Args)
	return err
}
