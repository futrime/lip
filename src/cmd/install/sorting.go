package cmdlipinstall

import (
	"errors"

	"github.com/liteldev/lip/tooth/toothfile"
	"github.com/liteldev/lip/utils/logger"
)

// sortToothFiles sorts tooth files by dependence with topological sort in descending order.
func sortToothFiles(toothFileList []toothfile.ToothFile) ([]toothfile.ToothFile, error) {
	var err error

	// Make a map from tooth path to tooth file.
	toothFileMap := make(map[string]toothfile.ToothFile, len(toothFileList))
	for _, toothFile := range toothFileList {
		toothFileMap[toothFile.Metadata().ToothPath] = toothFile
	}

	preVisited := make(map[string]bool)
	visited := make(map[string]bool)
	sorted := make([]toothfile.ToothFile, 0, len(toothFileList))
	for _, toothFile := range toothFileList {
		// Skip the tooth file if it has been visited.
		if visited[toothFile.Metadata().ToothPath] {
			continue
		}

		err = visit(toothFile, toothFileMap, preVisited, visited, &sorted)

		if err != nil {
			return nil, err
		}
	}

	return sorted, nil
}

// visit visits a tooth file and its dependencies.
func visit(toothFile toothfile.ToothFile, toothFileMap map[string]toothfile.ToothFile, preVisited map[string]bool, visited map[string]bool, sorted *[]toothfile.ToothFile) error {
	var err error

	if visited[toothFile.Metadata().ToothPath] {
		return nil
	}
	if preVisited[toothFile.Metadata().ToothPath] && !visited[toothFile.Metadata().ToothPath] {
		return errors.New("circular dependency detected")
	}

	preVisited[toothFile.Metadata().ToothPath] = true
	for depToothPath := range toothFile.Metadata().Dependencies {
		// Find the tooth file of the dependency.
		dep, ok := toothFileMap[depToothPath]
		if !ok {
			// Ignore the dependency if it is not in the tooth file list.
			// sortToothFiles only sorts the tooth files in the tooth file list.
			continue
		}

		err = visit(dep, toothFileMap, preVisited, visited, sorted)

		if err != nil {
			return err
		}
	}
	*sorted = append(*sorted, toothFile)
	visited[toothFile.Metadata().ToothPath] = true
	return nil
}
