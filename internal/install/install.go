package install

import (
	gozip "archive/zip"
	"fmt"
	"io"
	"net/url"
	"os"
	"path/filepath"
	"strings"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/tooth"
	"github.com/lippkg/lip/internal/zip"
)

// Install installs a tooth archive.
func Install(ctx context.Context, archive tooth.Archive) error {
	var err error

	// 1. Check if the tooth is already installed.

	if installed, err := tooth.IsToothInstalled(ctx, archive.Metadata().Tooth()); err != nil {
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

	jsonBytes, err := archive.Metadata().MarshalJSON()
	if err != nil {
		return fmt.Errorf("failed to marshal metadata: %w", err)
	}

	metadataFileName := url.QueryEscape(archive.Metadata().Tooth()) + ".json"
	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return fmt.Errorf("failed to get metadata directory: %w", err)
	}

	metadataPath := metadataDir.Join(path.MustParse(metadataFileName))

	err = os.WriteFile(metadataPath.LocalString(), jsonBytes, 0644)
	if err != nil {
		return fmt.Errorf("failed to create metadata file: %w", err)
	}

	return nil
}

// placeFiles places the files of the tooth.
func placeFiles(ctx context.Context, archive tooth.Archive) error {
	var err error

	workspaceDirStr, err := os.Getwd()
	if err != nil {
		return err
	}

	workspaceDir, err := path.Parse(workspaceDirStr)
	if err != nil {
		return err
	}

	// Open the archive.
	r, err := gozip.OpenReader(archive.FilePath().LocalString())
	if err != nil {
		return fmt.Errorf("failed to open archive: %w", err)
	}
	defer r.Close()

	filePaths, err := zip.GetFilePaths(r)
	if err != nil {
		return fmt.Errorf("failed to extract file paths: %w", err)
	}

	filePathRoot := path.ExtractLongestCommonPath(filePaths...)

	for _, place := range archive.Metadata().Files().Place {
		relDest, err := path.Parse(place.Dest)
		if err != nil {
			return fmt.Errorf("failed to parse destination path: %w", err)
		}

		// Check if the destination exists.
		if _, err := os.Stat(relDest.LocalString()); err == nil {
			return fmt.Errorf("destination %v already exists", relDest.LocalString())
		}

		dest := workspaceDir.Join(relDest)

		// Create the destination directory.
		err = os.MkdirAll(filepath.Dir(dest.LocalString()), 0755)
		if err != nil {
			return fmt.Errorf("failed to create destination directory: %w", err)
		}

		relSrc, err := path.Parse(place.Src)
		if err != nil {
			return fmt.Errorf("failed to parse source path: %w", err)
		}

		src := filePathRoot.Join(relSrc)

		// Iterate through the files in the archive,
		// and find the source file.
		for _, f := range r.File {
			// Skip directories.
			if strings.HasSuffix(f.Name, "/") {
				continue
			}

			if f.Name == src.String() {
				// Open the source file.
				rc, err := f.Open()
				if err != nil {
					return fmt.Errorf("failed to open source file: %w", err)
				}

				fw, err := os.Create(dest.LocalString())
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
