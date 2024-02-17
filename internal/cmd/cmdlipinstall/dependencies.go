package cmdlipinstall

import (
	"container/list"
	"fmt"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/tooth"
	log "github.com/sirupsen/logrus"
)

func getFixedToothAndVersionMap(ctx *context.Context, specifiedArchives []tooth.Archive, upgradeFlag bool,
	forceReinstallFlag bool) (map[string]semver.Version, error) {

	fixedTeethAndVersions := make(map[string]semver.Version)

	installedToothMetadataList, err := tooth.GetAllMetadata(ctx)
	if err != nil {
		return nil, fmt.Errorf("failed to get all installed tooth metadata\n\t%w", err)
	}

	for _, installedToothMetadata := range installedToothMetadataList {
		fixedTeethAndVersions[installedToothMetadata.ToothRepoPath()] = installedToothMetadata.Version()
	}

	for _, archive := range specifiedArchives {
		if fixedVersion, ok := fixedTeethAndVersions[archive.Metadata().ToothRepoPath()]; !ok {
			// If not installed, fix it.
			fixedTeethAndVersions[archive.Metadata().ToothRepoPath()] = archive.Metadata().Version()

		} else if forceReinstallFlag {
			// If to force reinstall, fix it.
			fixedTeethAndVersions[archive.Metadata().ToothRepoPath()] = archive.Metadata().Version()

		} else if upgradeFlag &&
			archive.Metadata().Version().GT(fixedTeethAndVersions[archive.Metadata().ToothRepoPath()]) {
			// If to upgrade and the version is newer, fix it.
			fixedTeethAndVersions[archive.Metadata().ToothRepoPath()] = archive.Metadata().Version()

		} else if fixedVersion.NE(archive.Metadata().Version()) {
			return nil, fmt.Errorf(
				"trying to fix tooth %v with version %v, but found version %v fixed",
				archive.Metadata().ToothRepoPath(), archive.Metadata().Version(), fixedVersion)
		}
	}

	return fixedTeethAndVersions, nil
}

// resolveDependencies resolves the dependencies of the tooth specified by the
// specifier and returns the paths to the downloaded teeth. rootArchiveList
// contains the root tooth archives to resolve dependencies.
// The first return value indicates whether the dependencies are resolved.
func resolveDependencies(ctx *context.Context, rootArchiveList []tooth.Archive,
	upgradeFlag bool, forceReinstallFlag bool) ([]tooth.Archive, error) {
	debugLogger := log.WithFields(log.Fields{
		"package": "cmdlipinstall",
		"method":  "resolveDependencies",
	})

	fixedToothAndVersionMap, err := getFixedToothAndVersionMap(ctx, rootArchiveList, upgradeFlag,
		forceReinstallFlag)
	if err != nil {
		return nil, fmt.Errorf("failed to get fixed tooth and version map\n\t%w", err)
	}

	notResolvedArchiveQueue := list.New()
	for _, rootArchive := range rootArchiveList {
		notResolvedArchiveQueue.PushBack(rootArchive)
	}

	resolvedArchiveList := make([]tooth.Archive, 0)

	for notResolvedArchiveQueue.Len() > 0 {
		archive := notResolvedArchiveQueue.Front().Value.(tooth.Archive)
		notResolvedArchiveQueue.Remove(notResolvedArchiveQueue.Front())

		depMap, err := archive.Metadata().Dependencies()
		if err != nil {
			return nil, fmt.Errorf("failed to get dependencies\n\t%w", err)
		}

		depStrMap := archive.Metadata().DependenciesAsStrings()

		for dep, versionRange := range depMap {
			if fixedVersion, ok := fixedToothAndVersionMap[dep]; ok {
				if !versionRange(fixedToothAndVersionMap[dep]) {
					return nil, fmt.Errorf("fixed tooth %v of version %v does not satisfy the version range %v",
						dep, fixedVersion.String(), depStrMap[dep])
				}

				// Avoid downloading the same tooth multiple times.
				debugLogger.Debugf("Dependency %v@%v is already fixed, skip", dep, fixedVersion)
				continue
			}

			targetVersion, err := tooth.GetLatestVersionInVersionRange(ctx, dep, versionRange)
			if err != nil {
				return nil, fmt.Errorf("no available version in %v found for dependency %v", depStrMap[dep], dep)
			}

			debugLogger.Debugf("Dependency %v of range %v is resolved to version %v", dep, depStrMap[dep], targetVersion)

			currentArchive, err := downloadToothArchiveIfNotCached(ctx, dep, targetVersion)
			if err != nil {
				return nil, fmt.Errorf("failed to download tooth\n\t%w", err)
			}

			debugLogger.Debugf("Downloaded tooth archive %v", currentArchive.FilePath().LocalString())

			notResolvedArchiveQueue.PushBack(currentArchive)

			fixedToothAndVersionMap[dep] = targetVersion
		}

		resolvedArchiveList = append(resolvedArchiveList, archive)
	}

	sortedArchives, err := topoSortToothArchives(resolvedArchiveList)
	if err != nil {
		return nil, fmt.Errorf("failed to sort teeth\n\t%w", err)
	}

	debugLogger.Debug("Topologically sorted teeth:")
	for _, archive := range sortedArchives {
		debugLogger.Debugf("  %v@%v: %v", archive.Metadata().ToothRepoPath(), archive.Metadata().Version(), archive.FilePath().LocalString())
	}

	return sortedArchives, nil
}
