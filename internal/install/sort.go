package install

import (
	"fmt"

	"github.com/lippkg/lip/internal/tooth"
)

// SortToothArchives sorts tooth archives by dependence with topological sort.
func SortToothArchives(archiveList []tooth.Archive) ([]tooth.Archive, error) {

	// Make a map from tooth path to tooth archive.
	archiveMap := make(map[string]tooth.Archive)
	for _, archive := range archiveList {
		archiveMap[archive.Metadata().ToothRepoPath()] = archive
	}

	preVisited := make(map[string]bool)
	visited := make(map[string]bool)
	sorted := make([]tooth.Archive, 0)
	for _, toothArchive := range archiveList {
		err := visit(toothArchive, archiveMap, preVisited, visited, &sorted)

		if err != nil {
			return nil, err
		}
	}

	return sorted, nil
}

// visit visits a tooth archive and its dependencies.
func visit(archive tooth.Archive, archiveMap map[string]tooth.Archive,
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

		err := visit(dep, archiveMap, preVisited, visited, sorted)

		if err != nil {
			return err
		}
	}
	*sorted = append(*sorted, archive)
	visited[archive.Metadata().ToothRepoPath()] = true
	return nil
}
