package main

import (
	"os"
	"reflect"
	"runtime"
	"strings"

	nested "github.com/antonfisher/nested-logrus-formatter"
	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/cmd/cmdlip"
	"github.com/lippkg/lip/internal/context"
	log "github.com/sirupsen/logrus"
	"golang.org/x/term"
)

var defaultConfig context.Config = context.Config{
	GitHubMirrorURL:  "https://github.com",
	GoModuleProxyURL: "https://goproxy.io",
	ProxyURL:         "",
}

var lipVersion semver.Version = semver.MustParse("0.24.0")

func IsStdoutAndStderrSupportAnsi() bool {
	if os.Getenv("NO_COLOR") != "" {
		return false
	}
	if os.Getenv("TERM") != "" && !strings.HasSuffix(os.Getenv("TERM"), "color") {
		return false
	}
	if !term.IsTerminal(int(os.Stdout.Fd())) || !term.IsTerminal(int(os.Stderr.Fd())) {
		return false
	}
	if runtime.GOOS == "windows" {
		{
			state, err := term.GetState(int(os.Stdout.Fd()))
			if err != nil {
				return false
			}
			if reflect.ValueOf(*state).Field(0).Field(0).Uint()&0x4 == 0 {
				return false
			}
		}
		{
			state, err := term.GetState(int(os.Stderr.Fd()))
			if err != nil {
				return false
			}
			if reflect.ValueOf(*state).Field(0).Field(0).Uint()&0x4 == 0 {
				return false
			}
		}
	}
	return true
}

func main() {
	if !IsStdoutAndStderrSupportAnsi() {
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

	if err := cmdlip.Run(ctx, os.Args); err != nil {
		log.Errorf("\n\t%v", err.Error())
		return
	}
}
