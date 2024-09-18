package cmdlip

import (
	"fmt"
	"os"

	nested "github.com/antonfisher/nested-logrus-formatter"
	"github.com/lippkg/lip/internal/cmd/cmdlipcache"
	"github.com/lippkg/lip/internal/cmd/cmdlipconfig"
	"github.com/lippkg/lip/internal/cmd/cmdlipinstall"
	"github.com/lippkg/lip/internal/cmd/cmdliplist"
	"github.com/lippkg/lip/internal/cmd/cmdlipshow"
	"github.com/lippkg/lip/internal/cmd/cmdliptooth"
	"github.com/lippkg/lip/internal/cmd/cmdlipuninstall"
	"github.com/lippkg/lip/internal/context"
	"github.com/urfave/cli/v2"

	log "github.com/sirupsen/logrus"
)

func Run(ctx *context.Context, args []string) error {
	cli.AppHelpTemplate = `
   {{.Name}} - {{.Usage}}{{if .Version}} - {{.Version}} {{end}}

Usage:
   {{.HelpName}} {{if .VisibleFlags}}[global options]{{end}}{{if .Commands}} command [command options]{{end}} {{if .ArgsUsage}}{{.ArgsUsage}}{{else}}[arguments...]{{end}}
   {{if len .Authors}}
Author:
   {{range .Authors}}{{ . }}{{end}}
   {{end}}{{if .Commands}}
Commands:
{{range .Commands}}{{if not .HideHelp}}   {{join .Names ", "}}{{ "\t"}}{{.Usage}}{{ "\n" }}{{end}}{{end}}{{end}}{{if .VisibleFlags}}
Global Options:
   {{range .VisibleFlags}}{{.}}
   {{end}}{{end}}{{if .Copyright }}
Copyright:
   {{.Copyright}}
   {{end}}
`
	cli.CommandHelpTemplate = `
{{.Usage}}

Usage:
   {{template "usageTemplate" .}}{{if .Category}}

Catrogy:
   {{.Category}}{{end}}{{if .Description}}

Description:
   {{template "descriptionTemplate" .}}{{end}}{{if .VisibleFlagCategories}}

Options:{{template "visibleFlagCategoryTemplate" .}}{{else if .VisibleFlags}}

Options:{{template "visibleFlagTemplate" .}}{{end}}
`
	cli.SubcommandHelpTemplate = `
{{.Usage}}

Usage:
   {{template "usageTemplate" .}}{{if .Category}}

Catrogy:
   {{.Category}}{{end}}{{if .Description}}

Description:
   {{template "descriptionTemplate" .}}{{end}}{{if .VisibleCommands}}

Commands:{{template "visibleCommandCategoryTemplate" .}}{{end}}{{if .VisibleFlagCategories}}

Options:{{template "visibleFlagCategoryTemplate" .}}{{else if .VisibleFlags}}

Options:{{template "visibleFlagTemplate" .}}{{end}}
`
	cli.VersionFlag = &cli.BoolFlag{
		Name:               "verison",
		Aliases:            []string{"V"},
		Usage:              "print the version",
		DisableDefaultText: true,
	}
	cli.VersionPrinter = func(cCtx *cli.Context) {
		fmt.Printf("lip %v from %v\n", ctx.LipVersion().String(), os.Args[0])
	}
	return (&cli.App{
		Name:    "lip",
		Usage:   "A general package installer",
		Version: ctx.LipVersion().String(),
		Flags: []cli.Flag{
			&cli.BoolFlag{
				Name:               "verbose",
				Aliases:            []string{"v"},
				Usage:              "show verbose output",
				DisableDefaultText: true,
			},
			&cli.BoolFlag{
				Name:               "quiet",
				Aliases:            []string{"q"},
				Usage:              "show only errors",
				DisableDefaultText: true,
			},
			&cli.BoolFlag{
				Name:               "no-color",
				Usage:              "disable color output",
				DisableDefaultText: true,
			},
		},
		Before: func(cCtx *cli.Context) error {
			if cCtx.Bool("no-color") {
				log.SetFormatter(&nested.Formatter{NoColors: true})
			}
			if cCtx.Bool("verbose") {
				log.SetLevel(log.DebugLevel)
			} else if cCtx.Bool("quiet") {
				log.SetLevel(log.ErrorLevel)
			} else {
				log.SetLevel(log.InfoLevel)
			}

			if cCtx.Bool("verbose") && cCtx.Bool("quiet") {
				return fmt.Errorf("verbose and quiet flags are mutually exclusive")
			}
			return nil
		},
		Commands: []*cli.Command{
			cmdlipcache.Command(ctx),
			cmdlipconfig.Command(ctx),
			cmdlipinstall.Command(ctx),
			cmdlipuninstall.Command(ctx),
			cmdliplist.Command(ctx),
			cmdlipshow.Command(ctx),
			cmdliptooth.Command(ctx),
		},
		CommandNotFound: func(cCtx *cli.Context, command string) {
			log.Errorf("unknown command: lip %v", command)
		},
	}).Run(args)

}
