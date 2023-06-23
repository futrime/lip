package cmdliptoothpack

import (
	"archive/zip"
	"compress/flate"
	"errors"
	"flag"
	"fmt"
	"io"
	"os"
	"path/filepath"

	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/logging"
	"github.com/lippkg/lip/pkg/teeth"
)

type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip tooth pack [options] <output path>

Description:
  Pack the tooth into a .tth file.

Options:
  -h, --help                  Show help.
`

func Run(ctx contexts.Context, args []string) error {
	var err error

	flagSet := flag.NewFlagSet("pack", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	err = flagSet.Parse(args)
	if err != nil {
		return fmt.Errorf("failed to parse flags: %w", err)
	}

	// Help flag has the highest priority.
	if flagDict.helpFlag {
		logging.Info(helpMessage)
		return nil
	}

	// Exactly one argument is required.
	if flagSet.NArg() != 1 {
		return fmt.Errorf("expected exactly one argument")
	}

	// Validate tooth.json.
	err = validateToothJSON(ctx)
	if err != nil {
		return fmt.Errorf("failed to validate tooth.json: %w", err)
	}

	// Pack the tooth.
	outputPath := flagSet.Arg(0)
	err = packTooth(ctx, outputPath)
	if err != nil {
		return fmt.Errorf("failed to pack tooth: %w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// copyFile copies a file from sourcePath to destinationPath.
func copyFile(sourcePath, destinationPath string) error {
	source, err := os.Open(sourcePath)
	if err != nil {
		return err
	}
	defer source.Close()

	destination, err := os.Create(destinationPath)
	if err != nil {
		return err
	}
	defer destination.Close()

	buf := make([]byte, 1024*1024)
	for {
		n, err := source.Read(buf)
		if err != nil && err != io.EOF {
			return err
		}
		if n == 0 {
			break
		}

		if _, err := destination.Write(buf[:n]); err != nil {
			return err
		}
	}

	return nil
}

// packFilesToTemp packs files to a temporary zip file.
func packFilesToTemp(fileList []string) (string, error) {
	zipFile, err := os.CreateTemp("", "*")
	zipFilePath := zipFile.Name()
	if err != nil {
		return "", errors.New("failed to create zip file: " + err.Error())
	}
	defer zipFile.Close()

	zipWriter := zip.NewWriter(zipFile)
	defer zipWriter.Close()

	// Set compression level.
	zipWriter.RegisterCompressor(zip.Deflate, func(out io.Writer) (io.WriteCloser, error) {
		return flate.NewWriter(out, flate.BestCompression)
	})

	// Write files to the zip file.
	for _, file := range fileList {
		logging.Info("packing " + file + " ...")

		writer, err := zipWriter.Create(filepath.ToSlash(file))
		if err != nil {
			return "", errors.New("failed to create zip writer for " + file + ": " + err.Error())
		}

		reader, err := os.Open(file)
		if err != nil {
			return "", errors.New("failed to open " + file + ": " + err.Error())
		}

		_, err = io.Copy(writer, reader)
		if err != nil {
			return "", errors.New("failed to copy " + file + ": " + err.Error())
		}

		err = reader.Close()
		if err != nil {
			return "", errors.New("failed to close " + file + ": " + err.Error())
		}
	}

	return zipFilePath, nil
}

// packTooth packs the tooth into a .tth file.
func packTooth(ctx contexts.Context, outputPath string) error {
	var err error

	if filepath.Ext(outputPath) != ".tth" {
		return errors.New("output path must have .tth extension")
	}

	_, err = os.Stat(outputPath)
	if err == nil {
		return errors.New("output path already exists")
	} else if !os.IsNotExist(err) {
		return errors.New("failed to stat output path: " + err.Error())
	}

	workspaceDir, err := ctx.WorkspaceDir()
	if err != nil {
		return errors.New("failed to get workspace directory: " + err.Error())
	}

	fileList, err := walkDirectory(workspaceDir)
	if err != nil {
		return errors.New("failed to walk workspace directory: " + err.Error())
	}

	// Pack files to a temporary zip file.
	zipFilePath, err := packFilesToTemp(fileList)
	if err != nil {
		return errors.New("failed to pack files to temp: " + err.Error())
	}

	// Copy the zip file to the output path.
	err = copyFile(zipFilePath, outputPath)
	if err != nil {
		return errors.New("failed to copy zip file to output path: " + err.Error())
	}

	return nil
}

// validateToothJSON validates tooth.json.
func validateToothJSON(ctx contexts.Context) error {
	var err error

	workspaceDir, err := ctx.WorkspaceDir()
	if err != nil {
		return errors.New("failed to get workspace directory: " + err.Error())
	}

	jsonBytes, err := os.ReadFile(filepath.Join(workspaceDir, "tooth.json"))
	if err != nil {
		return errors.New("failed to read tooth.json: " + err.Error())
	}

	_, err = teeth.NewMetadata(jsonBytes)
	if err != nil {
		return errors.New("failed to parse tooth.json: " + err.Error())
	}

	return nil
}

// walkDirectory walks the directory and returns a list of files.
func walkDirectory(dir string) ([]string, error) {
	var err error

	var ignoredDirs = []string{
		".git",
		".lip",
	}

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
		return nil, errors.New(
			"failed to walk through the current directory: " + err.Error())
	}

	return fileList, nil
}
