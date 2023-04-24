package cmdliptoothpack

import (
	"archive/zip"
	"compress/flate"
	"errors"
	"flag"
	"io"
	"os"
	"path/filepath"

	"github.com/lippkg/lip/tooth/toothmetadata"
	"github.com/lippkg/lip/utils/logger"
)

// FlagDict is a dictionary of flags.
type FlagDict struct {
	helpFlag   bool
	outputFlag string
}

const helpMessage = `
Usage:
  lip tooth pack [options]

Description:
  Pack the tooth into a .tth file.

Options:
  -h, --help                  Show help.
  -o, --output <file>         Output file.`

var ignoredDirs = []string{
	".git",
	".lip",
}

// Run is the entry point.
func Run(args []string) {
	var err error

	flagSet := flag.NewFlagSet("pack", flag.ExitOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		logger.Info(helpMessage)
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	flagSet.StringVar(&flagDict.outputFlag, "output", "tooth.tth", "")
	flagSet.StringVar(&flagDict.outputFlag, "o", "tooth.tth", "")
	flagSet.Parse(args)

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logger.Info(helpMessage)
		return
	}

	// No other arguments are supported.
	if flagSet.NArg() > 0 {
		logger.Error("Too many arguments.")
		os.Exit(1)
	}

	// Validate tooth.json.
	err = validateToothJSON()
	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}

	// Pack the tooth.
	err = packTooth(flagDict.outputFlag)
	if err != nil {
		logger.Error(err.Error())
		os.Exit(1)
	}

	logger.Info("tooth packed successfully")
}

// packTooth packs the tooth into a .tth file.
func packTooth(output string) error {
	var err error

	if output == "" {
		return errors.New("output file cannot be empty")
	}

	// Report error if the output file already exists.
	if _, err := os.Stat(output); err == nil {
		return errors.New("output file already exists")
	}

	// Walk through the current directory.
	fileList := make([]string, 0)
	err = filepath.WalkDir(".", func(path string, d os.DirEntry, err error) error {
		if err != nil {
			return err
		}

		// Do not walk through special directories.
		for _, ignoredDir := range ignoredDirs {
			if d.Name() == ignoredDir {
				return filepath.SkipDir
			}
		}

		if !d.IsDir() {
			fileList = append(fileList, path)
		}

		return nil
	})
	if err != nil {
		return errors.New("failed to walk through the current directory: " + err.Error())
	}

	// Create a zip file.
	zipFile, err := os.Create(output)
	if err != nil {
		return errors.New("failed to create zip file: " + err.Error())
	}

	zipWriter := zip.NewWriter(zipFile)
	defer zipWriter.Close()

	// Set compression level.
	zipWriter.RegisterCompressor(zip.Deflate, func(out io.Writer) (io.WriteCloser, error) {
		return flate.NewWriter(out, flate.BestCompression)
	})

	// Write files to the zip file.
	for _, file := range fileList {
		logger.Info("packing " + file + " ...")

		writer, err := zipWriter.Create(filepath.ToSlash(file))
		if err != nil {
			return errors.New("failed to create zip writer for " + file + ": " + err.Error())
		}

		reader, err := os.Open(file)
		if err != nil {
			return errors.New("failed to open " + file + ": " + err.Error())
		}

		_, err = io.Copy(writer, reader)
		if err != nil {
			return errors.New("failed to copy " + file + ": " + err.Error())
		}

		err = reader.Close()
		if err != nil {
			return errors.New("failed to close " + file + ": " + err.Error())
		}
	}

	return nil
}

// validateToothJSON validates tooth.json.
func validateToothJSON() error {
	var err error

	jsonString, err := os.ReadFile("tooth.json")
	if err != nil {
		return errors.New("failed to read tooth.json: " + err.Error())
	}

	_, err = toothmetadata.NewFromJSON(jsonString)
	if err != nil {
		return errors.New("failed to parse tooth.json: " + err.Error())
	}

	return nil
}
