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
	GitHubMirrorURL:  "",
	GoModuleProxyURL: "https://goproxy.io",
}

var lipVersion semver.Version = semver.MustParse("0.19.0")

func main() {
	log.SetFormatter(&nested.Formatter{})

	ctx := context.New(defaultConfig, lipVersion)

	if err := ctx.CreateDirStructure(); err != nil {
		log.Errorf("cannot create directory structure: %s", err.Error())
		return
	}

	if err := ctx.LoadOrCreateConfigFile(); err != nil {
		log.Errorf("cannot load or create config file: %s", err.Error())
		return
	}

	if err := cmdlip.Run(ctx, os.Args[1:]); err != nil {
		log.Error(err.Error())
		return
	}
}
