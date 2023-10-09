package installing

import (
	"fmt"
	"os"
	"path/filepath"

	"github.com/lippkg/lip/internal/contexts"
	"github.com/lippkg/lip/internal/paths"
	"github.com/lippkg/lip/internal/teeth"
)

func Uninstall(ctx contexts.Context, toothRepo string) error {
	var err error

	metadata, err := teeth.GetInstalledToothMetadata(ctx, toothRepo)
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
	metadataPath, err := ctx.CalculateMetadataPath(toothRepo)
	if err != nil {
		return err
	}

	err = os.Remove(metadataPath)
	if err != nil {
		return fmt.Errorf("failed to delete metadata file: %w", err)
	}

	return nil
}

// ---------------------------------------------------------------------

// removeToothFiles removes the files of the tooth.
func removeToothFiles(ctx contexts.Context, metadata teeth.Metadata) error {
	workspaceDir, err := ctx.WorkspaceDir()
	if err != nil {
		return err
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

		// Delete the file.
		err = os.RemoveAll(filepath.Join(workspaceDir, place.Dest))
		if err != nil {
			return fmt.Errorf("failed to delete file: %w", err)
		}

		// Delete all ancestor directories if they are empty until the workspace directory.
		for dir := filepath.Dir(place.Dest); dir != workspaceDir; dir = filepath.Dir(dir) {
			isInWorkspaceDir, err := paths.CheckIsAncesterOf(workspaceDir, dir)
			if err != nil {
				return fmt.Errorf("failed to check if the directory is an ancestor of the workspace directory: %w", err)
			}
			if !isInWorkspaceDir {
				break
			}

			fileList, err := os.ReadDir(dir)
			if err != nil {
				return fmt.Errorf("failed to read directory: %w", err)
			}

			if len(fileList) != 0 {
				break
			}

			err = os.Remove(dir)
			if err != nil {
				return fmt.Errorf("failed to delete directory: %w", err)
			}
		}
	}

	// Files marked as "remove" will be deleted.
	for _, removal := range metadata.Files().Remove {
		err = os.RemoveAll(filepath.Join(workspaceDir, removal))
		if err != nil {
			return fmt.Errorf("failed to delete file: %w", err)
		}
	}

	return nil
}
