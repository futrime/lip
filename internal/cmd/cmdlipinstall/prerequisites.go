package cmdlipinstall

import (
	"fmt"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/tooth"
)

// findMissingPrerequisites finds missing prerequisites of the tooth specified
// by the specifier and returns the map of missing prerequisites.
func findMissingPrerequisites(ctx context.Context,
	archiveList []tooth.Archive) (map[string]semver.Range, map[string]string, error) {
	missingPrerequisiteMap := make(map[string]semver.Range)
	missingPrerequisitesAsStrings := make(map[string]string)

	for _, archive := range archiveList {
		prerequisites := archive.Metadata().Prerequisites()
		prerequisitesAsStrings := archive.Metadata().PrerequisitesAsStrings()

		for prerequisite, versionRange := range prerequisites {
			isInstalled, err := tooth.IsInstalled(ctx, prerequisite)
			if err != nil {
				return nil, nil, fmt.Errorf("failed to check if tooth is installed: %w", err)
			}

			if isInstalled {
				currentMetadata, err := tooth.GetMetadata(ctx, prerequisite)
				if err != nil {
					return nil, nil, fmt.Errorf("failed to find installed tooth metadata: %w", err)
				}

				if !versionRange(currentMetadata.Version()) {
					missingPrerequisiteMap[prerequisite] = versionRange
					missingPrerequisitesAsStrings[prerequisite] = prerequisitesAsStrings[prerequisite]
				}

				break
			} else {
				// Check if the tooth is in the archive list.
				isInArchiveList := false
				for _, archive := range archiveList {
					if archive.Metadata().ToothRepoPath() == prerequisite && versionRange(archive.Metadata().Version()) {
						isInArchiveList = true
						break
					}
				}

				if !isInArchiveList {
					missingPrerequisiteMap[prerequisite] = versionRange
					missingPrerequisitesAsStrings[prerequisite] = prerequisitesAsStrings[prerequisite]
				}
			}
		}
	}

	return missingPrerequisiteMap, missingPrerequisitesAsStrings, nil
}
