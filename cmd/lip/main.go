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
	ProxyURL:         "",
}

var lipVersion semver.Version = semver.MustParse("0.21.2")

func main() {
	if os.Getenv("NO_COLOR") != "" {
		log.SetFormatter(&nested.Formatter{NoColors: true})
	} else {
		log.SetFormatter(&nested.Formatter{})
	}

	ctx := context.New(defaultConfig, lipVersion)

	if err := ctx.CreateDirStructure(); err != nil {
		log.Errorf("\n\tcannot create directory structure\n\t%v", err.Error())
		return
	}

	if err := ctx.LoadOrCreateConfigFile(); err != nil {
		log.Errorf("\n\tcannot load or create config file\n\t%v", err.Error())
		return
	}

	if err := cmdlip.Run(ctx, os.Args[1:]); err != nil {
		log.Errorf("\n\t%v", err.Error())
		return
	}
}
