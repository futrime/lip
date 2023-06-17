package cmd

import (
	"os"

	"github.com/lippkg/lip/pkg/cmd/cmdlip"
	"github.com/lippkg/lip/pkg/contexts"
)

func Run(context contexts.Context) error {
	err := cmdlip.Run(context, os.Args)
	return err
}
