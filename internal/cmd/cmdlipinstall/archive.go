package cmdlipinstall

import (
	"fmt"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/install"
	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/tooth"
	log "github.com/sirupsen/logrus"
)

func filterInstalledToothArchives(ctx *context.Context, archives []tooth.Archive, upgradeFlag bool,
	forceReinstallFlag bool) ([]tooth.Archive, error) {

	if forceReinstallFlag {
		return archives, nil
	}

	filteredArchives := make([]tooth.Archive, 0)
	for _, archive := range archives {
		isInstalled, err := tooth.IsInstalled(ctx, archive.Metadata().ToothRepoPath())
		if err != nil {
			return nil, fmt.Errorf("failed to check if tooth is installed: %w", err)
		}

		if !isInstalled {
			filteredArchives = append(filteredArchives, archive)
		} else if upgradeFlag {
			currentMetadata, err := tooth.GetMetadata(ctx, archive.Metadata().ToothRepoPath())
			if err != nil {
				return nil, fmt.Errorf("failed to find installed tooth metadata: %w", err)
			}

			if archive.Metadata().Version().GT(currentMetadata.Version()) {
				filteredArchives = append(filteredArchives, archive)
			} else {
				log.Infof("Tooth %v is already up-to-date", archive.Metadata().ToothRepoPath())
			}
		} else {
			log.Infof("Tooth %v is already installed", archive.Metadata().ToothRepoPath())
		}
	}

	return filteredArchives, nil
}

// installToothArchive installs the tooth archive.
func installToothArchive(ctx *context.Context, archive tooth.Archive, forceReinstall bool, upgrade bool) error {
	debugLogger := log.WithFields(log.Fields{
		"package": "cmdlipinstall",
		"method":  "installToothArchive",
	})

	isInstalled, err := tooth.IsInstalled(ctx, archive.Metadata().ToothRepoPath())
	if err != nil {
		return fmt.Errorf("failed to check if tooth is installed: %w", err)
	}

	shouldInstall := false
	shouldUninstall := false

	if isInstalled && forceReinstall {
		log.Infof("Reinstalling tooth %v", archive.Metadata().ToothRepoPath())

		shouldInstall = true
		shouldUninstall = true

	} else if isInstalled && upgrade {
		currentMetadata, err := tooth.GetMetadata(ctx,
			archive.Metadata().ToothRepoPath())
		if err != nil {
			return fmt.Errorf("failed to find installed tooth metadata: %w", err)
		}

		if archive.Metadata().Version().GT(currentMetadata.Version()) {
			log.Infof("Upgrading tooth %v", archive.Metadata().ToothRepoPath())

			shouldInstall = true
			shouldUninstall = true
		} else {
			log.Infof("Tooth %v is already up-to-date", archive.Metadata().ToothRepoPath())

			shouldInstall = false
			shouldUninstall = false
		}

	} else if isInstalled {
		log.Infof("Tooth %v is already installed", archive.Metadata().ToothRepoPath())

		shouldInstall = false
		shouldUninstall = false

	} else {
		log.Infof("Installing tooth %v", archive.Metadata().ToothRepoPath())

		shouldInstall = true
		shouldUninstall = false
	}

	if shouldUninstall {
		err := install.Uninstall(ctx, archive.Metadata().ToothRepoPath())
		if err != nil {
			return fmt.Errorf("failed to uninstall tooth: %w", err)
		}
		debugLogger.Debugf("Uninstalled tooth %v", archive.Metadata().ToothRepoPath())
	}

	if shouldInstall {
		assetURL, err := archive.Metadata().AssetURL()
		if err != nil {
			return fmt.Errorf("failed to get asset URL: %w", err)
		}

		assetArchiveFilePath := path.MakeEmpty()
		if assetURL.String() != "" {
			cachePath, err := getCachePath(ctx, assetURL)
			if err != nil {
				return fmt.Errorf("failed to get cache path: %w", err)
			}

			assetArchiveFilePath = cachePath
		}

		archiveWithAssets, err := archive.ToAssetArchiveAttached(assetArchiveFilePath)
		if err != nil {
			return fmt.Errorf("failed to attach asset archive %v: %w", assetArchiveFilePath.LocalString(), err)
		}

		if err := install.Install(ctx, archiveWithAssets); err != nil {
			return fmt.Errorf("failed to install tooth archive %v: %w", archiveWithAssets.FilePath().LocalString(), err)
		}
		debugLogger.Debugf("Installed tooth archive %v", archiveWithAssets.FilePath().LocalString())
	}

	return nil
}

// topoSortToothArchives sorts tooth archives by dependence with topological sort.
func topoSortToothArchives(archiveList []tooth.Archive) ([]tooth.Archive, error) {
	// Make a map from tooth path to tooth archive.
	archiveMap := make(map[string]tooth.Archive)
	for _, archive := range archiveList {
		archiveMap[archive.Metadata().ToothRepoPath()] = archive
	}

	preVisited := make(map[string]bool)
	visited := make(map[string]bool)
	sorted := make([]tooth.Archive, 0)
	for _, toothArchive := range archiveList {
		err := topoSortVisit(toothArchive, archiveMap, preVisited, visited, &sorted)

		if err != nil {
			return nil, err
		}
	}

	return sorted, nil
}

// topoSortVisit visits a tooth archive and its dependencies.
func topoSortVisit(archive tooth.Archive, archiveMap map[string]tooth.Archive,
	preVisited map[string]bool, visited map[string]bool, sorted *[]tooth.Archive) error {

	if visited[archive.Metadata().ToothRepoPath()] {
		return nil
	}

	if preVisited[archive.Metadata().ToothRepoPath()] && !visited[archive.Metadata().ToothRepoPath()] {
		return fmt.Errorf("tooth %s has a circular dependency", archive.Metadata().ToothRepoPath())
	}

	preVisited[archive.Metadata().ToothRepoPath()] = true
	for depToothPath := range archive.Metadata().Dependencies() {
		// Find the tooth archive of the dependency.
		dep, ok := archiveMap[depToothPath]
		if !ok {
			// Ignore the dependency if it is not in the tooth archive list.
			// sortToothArchives only sorts the tooth archives in the tooth archive list.
			continue
		}

		err := topoSortVisit(dep, archiveMap, preVisited, visited, sorted)

		if err != nil {
			return err
		}
	}
	*sorted = append(*sorted, archive)
	visited[archive.Metadata().ToothRepoPath()] = true
	return nil
}

// validateToothArchive validates the archive.
func validateToothArchive(archive tooth.Archive, toothRepoPath string, version semver.Version) error {
	if archive.Metadata().ToothRepoPath() != toothRepoPath {
		return fmt.Errorf("tooth name mismatch: %v != %v", archive.Metadata().ToothRepoPath(), toothRepoPath)
	}

	if archive.Metadata().Version().NE(version) {
		return fmt.Errorf("tooth version mismatch: %v != %v", archive.Metadata().Version(), version)
	}

	return nil
}
