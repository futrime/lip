package main

import (
	"os"

	"github.com/liteldev/lip/cmd"
)

func main() {
	// If no subcommand...
	if len(os.Args) < 2 {
		cmd.CmdLip()
		return
	}

	switch os.Args[1] {
	case "install":
		cmd.CmdInstall()
	default:
		cmd.CmdLip()
	}
}
