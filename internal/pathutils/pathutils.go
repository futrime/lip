package pathutils

import (
	"path/filepath"
	"strings"
)

// Equal returns true if path1 and path2 are identical.
func Equal(path1 string, path2 string) (bool, error) {
	var err error

	path1, err = Regularize(path1)
	if err != nil {
		return false, err
	}

	path2, err = Regularize(path2)
	if err != nil {
		return false, err
	}

	return path1 == path2, nil
}

// IsAncesterOf returns true if ancestor is an ancestor of path.
func IsAncesterOf(ancestor string, path string) (bool, error) {
	var err error

	// If ancestor equals to path, return false.
	isEqual, err := Equal(ancestor, path)
	if err != nil {
		return false, err
	}

	if isEqual {
		return false, nil
	}

	ancestor, err = Regularize(ancestor)
	if err != nil {
		return false, err
	}

	path, err = Regularize(path)
	if err != nil {
		return false, err
	}

	relativePath, err := filepath.Rel(ancestor, path)
	if err != nil {
		// If failed to get relative path, return false.
		return false, nil
	}

	relativePath = filepath.ToSlash(relativePath)

	return !strings.HasPrefix(relativePath, "../") && relativePath != "..", nil
}

// Regularize converts path to standard path, which is slash-separated absolute path.
func Regularize(path string) (string, error) {
	var err error

	path = filepath.FromSlash(path)
	path, err = filepath.Abs(path)
	if err != nil {
		return "", err
	}

	// Convert to slash for Windows.
	path = filepath.ToSlash(path)

	return path, nil
}
