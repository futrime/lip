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

// installToothArchive installs the tooth archive.
func installToothArchive(ctx context.Context, archive tooth.Archive, forceReinstall bool, upgrade bool) error {
	isInstalled, err := tooth.IsInstalled(ctx, archive.Metadata().ToothRepoPath())
	if err != nil {
		return fmt.Errorf("failed to check if tooth is installed: %w", err)
	}

	shouldInstall := false
	shouldUninstall := false

	if isInstalled && forceReinstall {
		log.Infof("Reinstalling tooth %v...", archive.Metadata().ToothRepoPath())

		shouldInstall = true
		shouldUninstall = true

	} else if isInstalled && upgrade {
		currentMetadata, err := tooth.GetMetadata(ctx,
			archive.Metadata().ToothRepoPath())
		if err != nil {
			return fmt.Errorf("failed to find installed tooth metadata: %w", err)
		}

		if archive.Metadata().Version().GT(currentMetadata.Version()) {
			log.Infof("Upgrading tooth %v...", archive.Metadata().ToothRepoPath())

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
		log.Infof("Installing tooth %v...", archive.Metadata().ToothRepoPath())

		shouldInstall = true
		shouldUninstall = false
	}

	if shouldUninstall {
		err := install.Uninstall(ctx, archive.Metadata().ToothRepoPath())
		if err != nil {
			return fmt.Errorf("failed to uninstall tooth: %w", err)
		}
	}

	if shouldInstall {
		err := install.Install(ctx, archive, path.MakeEmpty())
		if err != nil {
			return fmt.Errorf("failed to install tooth: %w", err)
		}
	}

	return nil
}

// installToothArchives installs the tooth archive list.
func installToothArchives(ctx context.Context,
	archives []tooth.Archive, forceReinstall bool, upgrade bool) error {
	for _, archive := range archives {
		if err := installToothArchive(ctx, archive, forceReinstall, upgrade); err != nil {
			return err
		}
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
