package path

import (
	"fmt"
	gopath "path"
	"path/filepath"
	"regexp"
	"strings"

	"golang.org/x/mod/module"
)

type Path struct {
	pathItems []string
}

// Parse parses a path string into a Path.
func Parse(path string) (Path, error) {
	// Convert to forward slashes.
	path = gopath.Clean(path)
	path = filepath.ToSlash(path)

	pathItems := strings.Split(path, "/")

	// Remove the last empty path item if the path ends with a slash.
	if pathItems[len(pathItems)-1] == "" {
		pathItems = pathItems[:len(pathItems)-1]
	}

	for i, pathItem := range pathItems {
		if i == 0 && (pathItem == "" || regexp.MustCompile(`^[a-zA-Z]:$`).MatchString(pathItem)) {
			continue
		}

		if err := module.CheckFilePath(pathItem); err != nil {
			return Path{}, fmt.Errorf("invalid path item %v in path %v", pathItem, path)
		}
	}

	return Path{
		pathItems: pathItems,
	}, nil
}

// MustParse parses a path string into a Path. It panics if the path is invalid.
func MustParse(path string) Path {
	p, err := Parse(path)
	if err != nil {
		panic(err)
	}

	return p
}

// ExtractLongestCommonPath returns the longest common path of two paths.
func ExtractLongestCommonPath(paths ...Path) Path {
	if len(paths) == 0 {
		return Path{}
	}

	shortestPathItemCount := len(paths[0].pathItems)
	for _, other := range paths {
		if len(other.pathItems) < shortestPathItemCount {
			shortestPathItemCount = len(other.pathItems)
		}
	}

	longestCommonPathItems := make([]string, 0)
outerLoop:
	for i := 0; i < shortestPathItemCount; i++ {
		pathItem := paths[0].pathItems[i]

		for _, other := range paths {
			if other.pathItems[i] != pathItem {
				break outerLoop
			}
		}

		longestCommonPathItems = append(longestCommonPathItems, pathItem)
	}

	return Path{
		pathItems: longestCommonPathItems,
	}
}

// Base returns the base of the path.
func (f Path) Base() string {
	if len(f.pathItems) == 0 {
		return ""
	}

	return f.pathItems[len(f.pathItems)-1]
}

// Dir returns the directory of the path.
func (f Path) Dir() (Path, error) {
	if len(f.pathItems) == 0 {
		return Path{}, fmt.Errorf("cannot get directory of empty path")
	}

	return Path{
		pathItems: f.pathItems[:len(f.pathItems)-1],
	}, nil
}

// Equal checks if two paths are equal.
func (f Path) Equal(other Path) bool {
	if len(f.pathItems) != len(other.pathItems) {
		return false
	}

	for i, pathItem := range f.pathItems {
		if pathItem != other.pathItems[i] {
			return false
		}
	}

	return true
}

// HasPrefix checks if the path has the prefix.
func (f Path) HasPrefix(prefix Path) bool {
	if len(f.pathItems) < len(prefix.pathItems) {
		return false
	}

	for i := 0; i < len(prefix.pathItems); i++ {
		if f.pathItems[i] != prefix.pathItems[i] {
			return false
		}
	}

	return true
}

// HasSuffix checks if the path has the suffix.
func (f Path) HasSuffix(suffix Path) bool {
	if len(f.pathItems) < len(suffix.pathItems) {
		return false
	}

	for i := 0; i < len(suffix.pathItems); i++ {
		if f.pathItems[len(f.pathItems)-len(suffix.pathItems)+i] != suffix.pathItems[i] {
			return false
		}
	}

	return true
}

// IsAncestorOf returns true if the path is an ancestor of the other path.
func (f Path) IsAncestorOf(path Path) bool {
	longestCommonPath := ExtractLongestCommonPath(f, path)

	return longestCommonPath.Equal(f) && !longestCommonPath.Equal(path)
}

// IsEmpty returns true if the path is empty.
func (f Path) IsEmpty() bool {
	return len(f.pathItems) == 0
}

// Join joins two paths.
func (f Path) Join(other Path) Path {
	return Path{
		pathItems: append(f.pathItems, other.pathItems...),
	}
}

// TrimPrefix trims the prefix from the path.
func (f Path) TrimPrefix(prefix Path) Path {
	if !prefix.HasPrefix(f) {
		return f
	}

	return Path{
		pathItems: f.pathItems[len(prefix.pathItems):],
	}
}

// TrimSuffix trims the suffix from the path.
func (f Path) TrimSuffix(suffix Path) Path {
	if !suffix.HasSuffix(f) {
		return f
	}

	return Path{
		pathItems: f.pathItems[:len(f.pathItems)-len(suffix.pathItems)],
	}
}

// String returns the string representation of a Path.
func (f Path) String() string {
	return gopath.Join(f.pathItems...)
}

// LocalString returns the local string representation of a Path.
func (f Path) LocalString() string {
	return filepath.FromSlash(f.String())
}
