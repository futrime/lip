package paths

import (
	"path/filepath"
	"strings"
)

// IsAncesterOf returns true if ancestor is an ancestor of path.
func IsAncesterOf(ancestor string, path string) bool {
	var err error

	ancestor = filepath.FromSlash(ancestor)
	ancestor, err = filepath.Abs(ancestor)
	if err != nil {
		return false
	}

	path = filepath.FromSlash(path)
	path, err = filepath.Abs(path)
	if err != nil {
		return false
	}

	relativePath, err := filepath.Rel(ancestor, path)
	if err != nil {
		return false
	}

	// Convert to slash for Windows.
	relativePath = filepath.ToSlash(relativePath)

	return !strings.HasPrefix(relativePath, "../") && relativePath != ".." && !IsIdentical(ancestor, path)
}

// IsIdentical returns true if path1 and path2 are identical.
func IsIdentical(path1 string, path2 string) bool {
	var err error

	path1 = filepath.FromSlash(path1)
	path1, err = filepath.Abs(path1)
	if err != nil {
		return false
	}

	path2 = filepath.FromSlash(path2)
	path2, err = filepath.Abs(path2)
	if err != nil {
		return false
	}

	return path1 == path2
}
