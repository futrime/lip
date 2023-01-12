package main

import (
	"os"

	cmd "github.com/liteldev/lip/cmd"
	cmdinstall "github.com/liteldev/lip/cmd/install"
)

func main() {
	// If no subcommand...
	if len(os.Args) < 2 {
		cmd.Run()
		return
	}

	switch os.Args[1] {
	case "install":
		cmdinstall.Run()
	default:
		cmd.Run()
	}
}
