package cli

import (
	"os"

	"github.com/lippkg/lip/pkg/cli/cmdlip"
	"github.com/lippkg/lip/pkg/contexts"
)

func Run(context contexts.Context) error {
	err := cmdlip.Run(context, os.Args)
	return err
}
