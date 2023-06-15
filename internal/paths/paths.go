package paths

import (
	"fmt"
	"path/filepath"
	"strings"
)

// Equal returns true if path1 and path2 are identical.
func Equal(path1 string, path2 string) (bool, error) {
	var err error

	path1 = filepath.Clean(path1)
	path2 = filepath.Clean(path2)

	path1, err = filepath.Abs(path1)
	if err != nil {
		return false, fmt.Errorf("cannot get absolute path: %w", err)
	}

	path2, err = filepath.Abs(path2)
	if err != nil {
		return false, fmt.Errorf("cannot get absolute path: %w", err)
	}

	return path1 == path2, nil
}

// IsAncesterOf returns true if ancestor is an ancestor of path.
func IsAncesterOf(ancestor string, path string) (bool, error) {
	var err error

	// If ancestor equals to path, return false.
	isEqual, err := Equal(ancestor, path)
	if err != nil {
		return false, fmt.Errorf("cannot compare paths: %w", err)
	}
	if isEqual {
		return false, nil
	}

	ancestor = filepath.Clean(ancestor)
	path = filepath.Clean(path)

	ancestor, err = filepath.Abs(ancestor)
	if err != nil {
		return false, fmt.Errorf("cannot get absolute path: %w", err)
	}

	path, err = filepath.Abs(path)
	if err != nil {
		return false, fmt.Errorf("cannot get absolute path: %w", err)
	}

	relativePath, err := filepath.Rel(ancestor, path)
	if err != nil {
		// If failed to get relative path, return false.
		return false, nil
	}

	relativePath = filepath.ToSlash(relativePath)

	return !strings.HasPrefix(relativePath, "../") && relativePath != "..", nil
}
