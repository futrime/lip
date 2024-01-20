package cmdliptoothpack

import (
	"archive/zip"
	"compress/flate"
	"flag"
	"fmt"
	"io"
	"os"
	"path/filepath"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/path"
	log "github.com/sirupsen/logrus"

	"github.com/lippkg/lip/internal/tooth"
)

type FlagDict struct {
	helpFlag bool
}

const helpMessage = `
Usage:
  lip tooth pack [options] <output path>

Description:
  Pack the tooth into a tooth archive.

Options:
  -h, --help                  Show help.
`

func Run(ctx *context.Context, args []string) error {

	flagSet := flag.NewFlagSet("pack", flag.ContinueOnError)

	// Rewrite the default usage message.
	flagSet.Usage = func() {
		// Do nothing.
	}

	var flagDict FlagDict
	flagSet.BoolVar(&flagDict.helpFlag, "help", false, "")
	flagSet.BoolVar(&flagDict.helpFlag, "h", false, "")
	err := flagSet.Parse(args)
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
	if err := validateToothJSON(ctx); err != nil {
		return fmt.Errorf("failed to validate tooth.json: %w", err)
	}

	// Pack the tooth.
	outputPath, err := path.Parse(flagSet.Arg(0))
	if err != nil {
		return fmt.Errorf("failed to parse output path %v: %w", flagSet.Arg(0), err)
	}

	if err := packTooth(ctx, outputPath); err != nil {
		return fmt.Errorf("failed to pack tooth: %w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// copyFile copies a file from sourcePath to destinationPath.
func copyFile(sourcePath, destinationPath path.Path) error {
	source, err := os.Open(sourcePath.LocalString())
	if err != nil {
		return err
	}
	defer source.Close()

	destination, err := os.Create(destinationPath.LocalString())
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
		return path.Path{}, fmt.Errorf("failed to create a temporary zip file: %w", err)
	}
	defer zipFile.Close()

	zipFilePath, err := path.Parse(zipFile.Name())
	if err != nil {
		return path.Path{}, fmt.Errorf("failed to parse the temporary zip file path %v: %w", zipFile.Name(), err)
	}

	zipWriter := zip.NewWriter(zipFile)
	defer zipWriter.Close()

	// Set compression level.
	zipWriter.RegisterCompressor(zip.Deflate, func(out io.Writer) (io.WriteCloser, error) {
		return flate.NewWriter(out, flate.BestCompression)
	})

	// Write files to the zip file.
	for _, file := range fileList {
		log.Infof("Packing %v...", file.LocalString())

		writer, err := zipWriter.Create(file.String())
		if err != nil {
			return path.Path{}, fmt.Errorf("failed to create %v in zip file: %w", file.String(), err)
		}

		reader, err := os.Open(file.LocalString())
		if err != nil {
			return path.Path{}, fmt.Errorf("failed to open %v: %w", file.LocalString(), err)
		}

		if _, err := io.Copy(writer, reader); err != nil {
			return path.Path{}, fmt.Errorf("failed to copy %v: %w", file.LocalString(), err)
		}

		if err := reader.Close(); err != nil {
			return path.Path{}, fmt.Errorf("failed to close %v: %w", file.LocalString(), err)
		}
	}

	return zipFilePath, nil
}

// packTooth packs the tooth into a tooth archive.
func packTooth(ctx *context.Context, outputPath path.Path) error {
	_, err := os.Stat(outputPath.LocalString())
	if err == nil {
		return fmt.Errorf("output path %v already exists", outputPath.LocalString())
	} else if !os.IsNotExist(err) {
		return fmt.Errorf("failed to stat output path %v: %w", outputPath.LocalString(), err)
	}

	workspaceDirStr, err := os.Getwd()
	if err != nil {
		return fmt.Errorf("failed to get workspace directory: %w", err)
	}

	workspaceDir, err := path.Parse(workspaceDirStr)
	if err != nil {
		return fmt.Errorf("failed to parse workspace directory: %w", err)
	}

	fileList, err := walkDirectory(workspaceDir)
	if err != nil {
		return fmt.Errorf("failed to walk through the current directory: %w", err)
	}

	// Pack files to a temporary zip file.
	zipFilePath, err := packFilesToTemp(fileList)
	if err != nil {
		return fmt.Errorf("failed to pack files to a temporary zip file: %w", err)
	}

	// Copy the zip file to the output path.

	if err := copyFile(zipFilePath, outputPath); err != nil {
		return fmt.Errorf("failed to copy the zip file from %v to %v: %w",
			zipFilePath.LocalString(), outputPath.LocalString(), err)
	}

	return nil
}

// validateToothJSON validates tooth.json.
func validateToothJSON(ctx *context.Context) error {

	workspaceDirStr, err := os.Getwd()
	if err != nil {
		return fmt.Errorf("failed to get workspace directory: %w", err)
	}

	workspaceDir, err := path.Parse(workspaceDirStr)
	if err != nil {
		return fmt.Errorf("failed to parse workspace directory: %w", err)
	}

	jsonBytes, err := os.ReadFile(workspaceDir.Join(path.MustParse("tooth.json")).String())
	if err != nil {
		return fmt.Errorf("failed to read tooth.json: %w", err)
	}

	if _, err := tooth.MakeMetadata(jsonBytes); err != nil {
		return fmt.Errorf("failed to parse tooth.json: %w", err)
	}

	return nil
}

// walkDirectory walks the directory and returns a list of files.
func walkDirectory(dir path.Path) ([]path.Path, error) {

	ignoredDirNames := []string{
		".git",
		".lip",
	}

	fileList := make([]path.Path, 0)
	err := filepath.WalkDir(".", func(pathStr string, d os.DirEntry, err error) error {
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
				return fmt.Errorf("failed to parse path: %w", err)
			}

			fileList = append(fileList, path)
		}

		return nil
	})
	if err != nil {
		return nil, fmt.Errorf("failed to walk through the directory: %w", err)
	}

	return fileList, nil
}
