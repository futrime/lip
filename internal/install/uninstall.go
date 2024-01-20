package install

import (
	"fmt"
	"net/url"
	"os"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/tooth"

	log "github.com/sirupsen/logrus"
)

func Uninstall(ctx *context.Context, toothRepoPath string) error {
	debugLogger := log.WithFields(log.Fields{
		"package": "install",
		"method":  "Uninstall",
	})

	metadata, err := tooth.GetMetadata(ctx, toothRepoPath)
	if err != nil {
		return err
	}

	// 1. Run pre-uninstall commands.

	if err := runCommands(metadata.Commands().PreUninstall); err != nil {
		return fmt.Errorf("failed to run pre-uninstall commands: %w", err)
	}
	debugLogger.Debug("Ran pre-uninstall commands")

	// 2. Delete files.

	if err := removeToothFiles(ctx, metadata); err != nil {
		return fmt.Errorf("failed to delete files: %w", err)
	}
	debugLogger.Debug("Deleted files")

	// 3. Run post-uninstall commands.

	if err := runCommands(metadata.Commands().PostUninstall); err != nil {
		return fmt.Errorf("failed to run post-uninstall commands: %w", err)
	}
	debugLogger.Debug("Ran post-uninstall commands")

	// 4. Delete the metadata file.

	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return fmt.Errorf("failed to get metadata directory: %w", err)
	}

	metadataFileName := fmt.Sprintf("%v.json", url.QueryEscape(toothRepoPath))
	metadataPath := metadataDir.Join(path.MustParse(metadataFileName))

	if err := os.Remove(metadataPath.LocalString()); err != nil {
		return fmt.Errorf("failed to delete metadata file: %w", err)
	}

	debugLogger.Debugf("Deleted metadata file %v", metadataPath.LocalString())

	return nil
}

// removeToothFiles removes the files of the tooth.
func removeToothFiles(ctx *context.Context, metadata tooth.Metadata) error {
	debugLogger := log.WithFields(log.Fields{
		"package": "install",
		"method":  "removeToothFiles",
	})

	workspaceDirStr, err := os.Getwd()
	if err != nil {
		return err
	}

	workspaceDir, err := path.Parse(workspaceDirStr)
	if err != nil {
		return fmt.Errorf("failed to parse workspace directory: %w", err)
	}

	for _, place := range metadata.Files().Place {
		// Files marked as "preserve" will not be deleted.
		isPreserved := false
		for _, preserve := range metadata.Files().Preserve {
			if place.Dest == preserve {
				isPreserved = true
				break
			}
		}
		if isPreserved {
			debugLogger.Debugf("Preserved file %v", place.Dest)
			continue
		}

		relDest, err := path.Parse(place.Dest)
		if err != nil {
			return fmt.Errorf("failed to parse destination path: %w", err)
		}

		dest := workspaceDir.Join(relDest)

		// Delete the file.
		if err := os.RemoveAll(dest.LocalString()); err != nil {
			return fmt.Errorf("failed to delete file: %w", err)
		}
		debugLogger.Debugf("Deleted file %v", dest.LocalString())

		// Delete all ancestor directories if they are empty until the workspace directory.
		dir := dest
		for {
			dir, err := dir.Dir()
			if err != nil {
				return fmt.Errorf("failed to parse directory: %w", err)
			}

			if dir.Equal(workspaceDir) {
				break
			}

			isInWorkspaceDir := workspaceDir.IsAncestorOf(dir)
			if !isInWorkspaceDir {
				break
			}

			fileList, err := os.ReadDir(dir.LocalString())
			if err != nil {
				// If the directory does not exist, we can ignore the error.
				break
			}

			if len(fileList) != 0 {
				break
			}

			if err := os.Remove(dir.LocalString()); err != nil {
				return fmt.Errorf("failed to delete directory: %w", err)
			}
			debugLogger.Debugf("Deleted directory %v", dir.LocalString())
		}
	}

	// Files marked as "remove" will be deleted regardless of whether they are marked as "preserve".
	for _, removal := range metadata.Files().Remove {
		removalPath, err := path.Parse(removal)
		if err != nil {
			return fmt.Errorf("failed to parse removal path: %w", err)
		}

		if err := os.RemoveAll(workspaceDir.Join(removalPath).LocalString()); err != nil {
			return fmt.Errorf("failed to delete file: %w", err)
		}
		debugLogger.Debugf("Deleted file %v that is marked as \"remove\"", workspaceDir.Join(removalPath).LocalString())
	}

	return nil
}
