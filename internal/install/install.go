package install

import (
	"archive/zip"
	"fmt"
	"io"
	"net/url"
	"os"
	"path/filepath"
	"strings"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/tooth"
	log "github.com/sirupsen/logrus"
)

// Install installs a tooth archive with an asset archive. If assetArchiveFilePath is empty,
// will use the tooth archive as the asset archive.
func Install(ctx *context.Context, archive tooth.Archive) error {
	debugLogger := log.WithFields(log.Fields{
		"package": "install",
		"method":  "Install",
	})

	commandEnvirons := make(map[string]string)

	proxyURL, err := ctx.ProxyURL()
	if err != nil {
		return fmt.Errorf("failed to get proxy URL\n\t%w", err)
	}

	if proxyURL.String() != "" {
		commandEnvirons = map[string]string{
			"HTTP_PROXY":  proxyURL.String(),
			"HTTPS_PROXY": proxyURL.String(),
		}
	}

	// 1. Check if the tooth is already installed.

	if installed, err := tooth.IsInstalled(ctx, archive.Metadata().ToothRepoPath()); err != nil {
		return fmt.Errorf("failed to check if tooth is installed\n\t%w", err)
	} else if installed {
		return fmt.Errorf("tooth %v is already installed", archive.Metadata().ToothRepoPath())
	}
	debugLogger.Debug("Checked if tooth is already installed")

	// 2. Run pre-install commands.

	if err := runCommands(archive.Metadata().Commands().PreInstall, commandEnvirons); err != nil {
		return fmt.Errorf("failed to run pre-install commands\n\t%w", err)
	}
	debugLogger.Debug("Ran pre-install commands")

	// 3. Extract and place files.

	assetFilePath, err := archive.AssetFilePath()
	if err != nil {
		return fmt.Errorf("failed to get asset file path of archive %v\n\t%w", archive.FilePath().LocalString(), err)
	}

	if err := placeFiles(ctx, archive.Metadata(), assetFilePath); err != nil {
		return fmt.Errorf("failed to place files\n\t%w", err)
	}
	debugLogger.Debug("Placed files")

	// 4. Run post-install commands.

	if err := runCommands(archive.Metadata().Commands().PostInstall, commandEnvirons); err != nil {
		return fmt.Errorf("failed to run post-install commands\n\t%w", err)
	}
	debugLogger.Debug("Ran post-install commands")

	// 5. Create metadata file.

	jsonBytes, err := archive.Metadata().MarshalJSON()
	if err != nil {
		return fmt.Errorf("failed to marshal metadata\n\t%w", err)
	}

	metadataFileName := url.QueryEscape(archive.Metadata().ToothRepoPath()) + ".json"
	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return fmt.Errorf("failed to get metadata directory\n\t%w", err)
	}

	metadataPath := metadataDir.Join(path.MustParse(metadataFileName))

	if err := os.WriteFile(metadataPath.LocalString(), jsonBytes, 0644); err != nil {
		return fmt.Errorf("failed to create metadata file\n\t%w", err)
	}

	debugLogger.Debugf("Created metadata file %v", metadataPath.LocalString())

	return nil
}

// placeFiles places the files of the tooth.
func placeFiles(ctx *context.Context, metadata tooth.Metadata, assetArchiveFilePath path.Path) error {
	debugLogger := log.WithFields(log.Fields{
		"package": "install",
		"method":  "placeFiles",
	})

	workspaceDirStr, err := os.Getwd()
	if err != nil {
		return err
	}

	workspaceDir, err := path.Parse(workspaceDirStr)
	if err != nil {
		return err
	}

	// Open the archive.
	r, err := zip.OpenReader(assetArchiveFilePath.LocalString())
	if err != nil {
		return fmt.Errorf("failed to open zip reader\n\t%w", err)
	}
	defer r.Close()

	files, err := metadata.Files()
	if err != nil {
		return fmt.Errorf("failed to get files from metadata\n\t%w", err)
	}

	for _, place := range files.Place {
		relDest := place.Dest

		// Check if the destination exists.
		if _, err := os.Stat(relDest.LocalString()); err == nil {
			return fmt.Errorf("destination %v already exists", relDest.LocalString())
		}

		dest := workspaceDir.Join(relDest)

		// Create the destination directory.
		if err := os.MkdirAll(filepath.Dir(dest.LocalString()), 0755); err != nil {
			return fmt.Errorf("failed to create destination directory\n\t%w", err)
		}
		debugLogger.Debugf("Created destination directory %v", filepath.Dir(dest.LocalString()))

		// Iterate through the files in the archive,
		// and find the source file.
		for _, f := range r.File {
			// Skip directories.
			if strings.HasSuffix(f.Name, "/") {
				debugLogger.Debugf("Skipped %v because it is a directory", f.Name)

				continue
			}

			filePath, err := path.Parse(f.Name)
			if err != nil {
				return fmt.Errorf("failed to parse file path from %v\n\t%w", f.Name, err)
			}

			if filePath.Equal(place.Src) {
				// Open the source file.
				rc, err := f.Open()
				if err != nil {
					return fmt.Errorf("failed to open source file\n\t%w", err)
				}

				fw, err := os.Create(dest.LocalString())
				if err != nil {
					return fmt.Errorf("failed to create destination file\n\t%w", err)
				}

				// Copy the file.
				if _, err := io.Copy(fw, rc); err != nil {
					return fmt.Errorf("failed to copy file\n\t%w", err)
				}

				// Close the files.
				rc.Close()
				fw.Close()

				debugLogger.Debugf("Placed file %v to %v", f.Name, dest.LocalString())
			}
		}
	}

	return nil
}
