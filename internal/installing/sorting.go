package installing

import (
	"errors"

	"github.com/lippkg/lip/internal/teeth"
)

type SortingOrder int

const (
	AscendingOrder SortingOrder = iota
	DescendingOrder
)

// SortToothArchives sorts tooth archives by dependence with topological sort.
func SortToothArchives(archiveList []teeth.Archive) ([]teeth.Archive, error) {
	var err error

	// Make a map from tooth path to tooth archive.
	archiveMap := make(map[string]teeth.Archive)
	for _, archive := range archiveList {
		archiveMap[archive.Metadata().Tooth()] = archive
	}

	preVisited := make(map[string]bool)
	visited := make(map[string]bool)
	sorted := make([]teeth.Archive, 0)
	for _, toothArchive := range archiveList {
		err = visit(toothArchive, archiveMap, preVisited, visited, &sorted)

		if err != nil {
			return nil, err
		}
	}

	return sorted, nil
}

// visit visits a tooth archive and its dependencies.
func visit(archive teeth.Archive, archiveMap map[string]teeth.Archive,
	preVisited map[string]bool, visited map[string]bool, sorted *[]teeth.Archive) error {

	var err error

	if visited[archive.Metadata().Tooth()] {
		return nil
	}

	if preVisited[archive.Metadata().Tooth()] && !visited[archive.Metadata().Tooth()] {
		return errors.New("circular dependency detected")
	}

	preVisited[archive.Metadata().Tooth()] = true
	for depToothPath := range archive.Metadata().Dependencies() {
		// Find the tooth archive of the dependency.
		dep, ok := archiveMap[depToothPath]
		if !ok {
			// Ignore the dependency if it is not in the tooth archive list.
			// sortToothArchives only sorts the tooth archives in the tooth archive list.
			continue
		}

		err = visit(dep, archiveMap, preVisited, visited, sorted)

		if err != nil {
			return err
		}
	}
	*sorted = append(*sorted, archive)
	visited[archive.Metadata().Tooth()] = true
	return nil
}
