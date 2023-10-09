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

func ExtractCommonAncestor(paths []string) string {
	if len(paths) == 0 {
		return ""
	}

	filePathCommonPrefix := extractCommonPrefix(paths)

	// If the common prefix contains a slash but is not the first character,
	// remove everything after it.
	if slashIndex := strings.LastIndex(filePathCommonPrefix, "/"); slashIndex > 0 {
		filePathCommonPrefix = filePathCommonPrefix[:slashIndex+1]
	} else {
		filePathCommonPrefix = ""
	}

	return filePathCommonPrefix
}

// CheckIsAncesterOf returns true if ancestor is an ancestor of path.
func CheckIsAncesterOf(ancestor string, path string) (bool, error) {
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

// ---------------------------------------------------------------------

// extractCommonPrefix returns the common prefix of a list of strings.
func extractCommonPrefix(strList []string) string {
	if len(strList) == 0 {
		return ""
	}

	commonPrefix := strList[0]

	for _, str := range strList {
		commonPrefix = extractCommonPrefix2(commonPrefix, str)
	}

	return commonPrefix
}

// extractCommonPrefix2 returns the common prefix of two strings.
func extractCommonPrefix2(str1, str2 string) string {
	commonPrefix := ""

	for i := 0; i < len(str1) && i < len(str2); i++ {
		if str1[i] != str2[i] {
			break
		}

		commonPrefix += string(str1[i])
	}

	return commonPrefix
}
