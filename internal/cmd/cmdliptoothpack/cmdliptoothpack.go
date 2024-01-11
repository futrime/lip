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

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/path"
	log "github.com/sirupsen/logrus"

	"github.com/lippkg/lip/internal/teeth"
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

func Run(ctx context.Context, args []string) error {
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
		fmt.Print(helpMessage)
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
	outputPath, err := path.Parse(flagSet.Arg(0))
	if err != nil {
		return fmt.Errorf("failed to parse output path: %w", err)
	}

	err = packTooth(ctx, outputPath)
	if err != nil {
		return fmt.Errorf("failed to pack tooth: %w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// copyFile copies a file from sourcePath to destinationPath.
func copyFile(sourcePath, destinationPath path.Path) error {
	source, err := os.Open(sourcePath.String())
	if err != nil {
		return err
	}
	defer source.Close()

	destination, err := os.Create(destinationPath.String())
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
func packFilesToTemp(fileList []path.Path) (path.Path, error) {
	zipFile, err := os.CreateTemp("", "*")
	if err != nil {
		return path.Path{}, errors.New("failed to create zip file: " + err.Error())
	}
	defer zipFile.Close()

	zipFilePath, err := path.Parse(zipFile.Name())
	if err != nil {
		return path.Path{}, errors.New("failed to parse zip file path: " + err.Error())
	}

	zipWriter := zip.NewWriter(zipFile)
	defer zipWriter.Close()

	// Set compression level.
	zipWriter.RegisterCompressor(zip.Deflate, func(out io.Writer) (io.WriteCloser, error) {
		return flate.NewWriter(out, flate.BestCompression)
	})

	// Write files to the zip file.
	for _, file := range fileList {
		log.Infof("packing %v", file.String())

		writer, err := zipWriter.Create(file.String())
		if err != nil {
			return path.Path{}, fmt.Errorf("failed to create %v: %w", file.String(), err)
		}

		reader, err := os.Open(file.String())
		if err != nil {
			return path.Path{}, fmt.Errorf("failed to open %v: %w", file.String(), err)
		}

		_, err = io.Copy(writer, reader)
		if err != nil {
			return path.Path{}, fmt.Errorf("failed to copy %v: %w", file.String(), err)
		}

		err = reader.Close()
		if err != nil {
			return path.Path{}, fmt.Errorf("failed to close %v: %w", file.String(), err)
		}
	}

	return zipFilePath, nil
}

// packTooth packs the tooth into a .tth file.
func packTooth(ctx context.Context, outputPath path.Path) error {
	var err error

	if filepath.Ext(outputPath.String()) != ".tth" {
		return errors.New("output path must have .tth extension")
	}

	_, err = os.Stat(outputPath.String())
	if err == nil {
		return errors.New("output path already exists")
	} else if !os.IsNotExist(err) {
		return errors.New("failed to stat output path: " + err.Error())
	}

	workspaceDirStr, err := os.Getwd()
	if err != nil {
		return errors.New("failed to get workspace directory: " + err.Error())
	}

	workspaceDir, err := path.Parse(workspaceDirStr)
	if err != nil {
		return errors.New("failed to parse workspace directory: " + err.Error())
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
func validateToothJSON(ctx context.Context) error {
	var err error

	workspaceDirStr, err := os.Getwd()
	if err != nil {
		return errors.New("failed to get workspace directory: " + err.Error())
	}

	workspaceDir, err := path.Parse(workspaceDirStr)
	if err != nil {
		return errors.New("failed to parse workspace directory: " + err.Error())
	}

	jsonBytes, err := os.ReadFile(workspaceDir.Join(path.MustParse("tooth.json")).String())
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
func walkDirectory(dir path.Path) ([]path.Path, error) {
	var err error

	var ignoredDirNames = []string{
		".git",
		".lip",
	}

	fileList := make([]path.Path, 0)
	err = filepath.WalkDir(".", func(pathStr string, d os.DirEntry, err error) error {
		if err != nil {
			return err
		}

		// Do not walk through special directories.
		for _, ignoredDir := range ignoredDirNames {
			if d.Name() == ignoredDir {
				return filepath.SkipDir
			}
		}

		if !d.IsDir() {
			path, err := path.Parse(pathStr)
			if err != nil {
				return errors.New("failed to parse path: " + err.Error())
			}

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
