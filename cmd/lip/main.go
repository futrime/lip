package main

import (
	"os"

	nested "github.com/antonfisher/nested-logrus-formatter"
	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/cmd/cmdlip"
	"github.com/lippkg/lip/internal/context"

	log "github.com/sirupsen/logrus"
)

var defaultConfig context.Config = context.Config{
	GitHubMirrorURL:  "https://github.com",
	GoModuleProxyURL: "https://goproxy.io",
}

var lipVersion semver.Version = semver.MustParse("0.18.0")

func main() {
	var err error

	log.SetFormatter(&nested.Formatter{})

	ctx := context.Make(defaultConfig, lipVersion)

	err = ctx.CreateDirStructure()
	if err != nil {
		log.Errorf("cannot create directory structure: %s", err.Error())
		return
	}

	err = ctx.LoadOrCreateConfigFile()
	if err != nil {
		log.Errorf("cannot load or create config file: %s", err.Error())
		return
	}

	err = cmdlip.Run(ctx, os.Args[1:])
	if err != nil {
		log.Error(err.Error())
		return
	}
}
