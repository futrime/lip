package installing

import (
	"fmt"
	"net/url"
	"os"

	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/tooth"
)

func Uninstall(ctx context.Context, toothRepo string) error {
	var err error

	metadata, err := tooth.GetInstalledToothMetadata(ctx, toothRepo)
	if err != nil {
		return err
	}

	// 1. Run pre-uninstall commands.
	err = runCommands(metadata.Commands().PreUninstall)
	if err != nil {
		return fmt.Errorf("failed to run pre-uninstall commands: %w", err)
	}

	// 2. Delete files.
	err = removeToothFiles(ctx, metadata)
	if err != nil {
		return fmt.Errorf("failed to delete files: %w", err)
	}

	// 3. Run post-uninstall commands.
	err = runCommands(metadata.Commands().PostUninstall)
	if err != nil {
		return fmt.Errorf("failed to run post-uninstall commands: %w", err)
	}

	// 4. Delete the metadata file.
	metadataFileName := url.QueryEscape(toothRepo) + ".json"
	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return fmt.Errorf("failed to get metadata directory: %w", err)
	}

	metadataPath := metadataDir.Join(path.MustParse(metadataFileName))

	err = os.Remove(metadataPath.LocalString())
	if err != nil {
		return fmt.Errorf("failed to delete metadata file: %w", err)
	}

	return nil
}

// removeToothFiles removes the files of the tooth.
func removeToothFiles(ctx context.Context, metadata tooth.Metadata) error {
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
			continue
		}

		relDest, err := path.Parse(place.Dest)
		if err != nil {
			return fmt.Errorf("failed to parse destination path: %w", err)
		}

		dest := workspaceDir.Join(relDest)

		// Delete the file.
		err = os.RemoveAll(dest.LocalString())
		if err != nil {
			return fmt.Errorf("failed to delete file: %w", err)
		}

		// Delete all ancestor directories if they are empty until the workspace directory.
		dir := dest
		for {
			dir, err = dir.Dir()
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

			err = os.Remove(dir.LocalString())
			if err != nil {
				return fmt.Errorf("failed to delete directory: %w", err)
			}
		}
	}

	// Files marked as "remove" will be deleted.
	for _, removal := range metadata.Files().Remove {
		removalPath, err := path.Parse(removal)
		if err != nil {
			return fmt.Errorf("failed to parse removal path: %w", err)
		}

		err = os.RemoveAll(workspaceDir.Join(removalPath).LocalString())
		if err != nil {
			return fmt.Errorf("failed to delete file: %w", err)
		}
	}

	return nil
}
