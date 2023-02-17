package paths

import (
	"path/filepath"
	"strings"
)

// IsAncesterOf returns true if ancestor is an ancestor of path.
func IsAncesterOf(ancestor string, path string) bool {
	ancestor, err := filepath.Abs(ancestor)
	if err != nil {
		return false
	}

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

	return !strings.HasPrefix(relativePath, "../") && relativePath != ".."
}
