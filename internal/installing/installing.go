package installing

import (
	"archive/zip"
	"fmt"
	"io"
	"net/url"
	"os"
	"path/filepath"
	"strings"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/paths"
	"github.com/lippkg/lip/internal/teeth"
)

// Install installs a tooth archive.
func Install(ctx context.Context, archive teeth.Archive) error {
	var err error

	// 1. Check if the tooth is already installed.

	if installed, err := teeth.CheckIsToothInstalled(ctx, archive.Metadata().Tooth()); err != nil {
		return fmt.Errorf("failed to check if tooth is installed: %w", err)
	} else if installed {
		return fmt.Errorf("tooth %v is already installed", archive.Metadata().Tooth())
	}

	// 2. Run pre-install commands.

	runCommands(archive.Metadata().Commands().PreInstall)

	// 3. Extract and place files.

	placeFiles(ctx, archive)

	// 4. Run post-install commands.

	runCommands(archive.Metadata().Commands().PostInstall)

	// 5. Create metadata file.

	jsonBytes, err := archive.Metadata().JSON(true)
	if err != nil {
		return fmt.Errorf("failed to marshal metadata: %w", err)
	}

	metadataFileName := url.QueryEscape(archive.Metadata().Tooth()) + ".json"
	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return fmt.Errorf("failed to get metadata directory: %w", err)
	}

	metadataPath, err := filepath.Join(metadataDir, metadataFileName), nil
	if err != nil {
		return err
	}

	err = os.WriteFile(metadataPath, jsonBytes, 0644)
	if err != nil {
		return fmt.Errorf("failed to create metadata file: %w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// extractAllFilePaths extracts all file paths from a zip archive.
func extractAllFilePaths(r *zip.ReadCloser) []string {
	filePathList := make([]string, len(r.File))

	for i, file := range r.File {
		filePathList[i] = file.Name
	}

	return filePathList
}

// placeFiles places the files of the tooth.
func placeFiles(ctx context.Context, archive teeth.Archive) error {
	var err error

	workspaceDir, err := os.Getwd()
	if err != nil {
		return err
	}

	// Open the archive.
	r, err := zip.OpenReader(archive.Path())
	if err != nil {
		return fmt.Errorf("failed to open archive: %w", err)
	}
	defer r.Close()

	prefix := paths.ExtractCommonAncestor(extractAllFilePaths(r))

	for _, place := range archive.Metadata().Files().Place {
		// Check if the destination exists.
		if _, err := os.Stat(place.Dest); err == nil {
			return fmt.Errorf("destination %v already exists", place.Dest)
		}

		fullDest := filepath.Join(workspaceDir, place.Dest)

		// Create the destination directory.
		err = os.MkdirAll(filepath.Dir(fullDest), 0755)
		if err != nil {
			return fmt.Errorf("failed to create destination directory: %w", err)
		}

		// Iterate through the files in the archive,
		// and find the source file.
		for _, f := range r.File {
			// Do not copy directories.
			if strings.HasSuffix(f.Name, "/") {
				continue
			}

			if f.Name == prefix+place.Src {
				// Open the source file.
				rc, err := f.Open()
				if err != nil {
					return fmt.Errorf("failed to open source file: %w", err)
				}

				fw, err := os.Create(fullDest)
				if err != nil {
					return fmt.Errorf("failed to create destination file: %w", err)
				}

				// Copy the file.
				_, err = io.Copy(fw, rc)
				if err != nil {
					return fmt.Errorf("failed to copy file: %w", err)
				}

				// Close the files.
				rc.Close()
				fw.Close()
			}
		}
	}

	return nil
}
