package main

import (
	"os"

	cmdlip "github.com/liteldev/lip/cmd"
	context "github.com/liteldev/lip/context"
)

func main() {
	// Set Goproxy if environment variable GOPROXY is set.
	if goproxy := os.Getenv("GOPROXY"); goproxy != "" {
		context.Goproxy = goproxy
	} else {
		context.Goproxy = context.DefaultGoproxy
	}

	cmdlip.Run()
}
