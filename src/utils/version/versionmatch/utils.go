package versionmatch

import (
	"regexp"
	"strings"
)

// IsValidVersionMatchString returns true if the version match string is valid.
func IsValidVersionMatchString(versionMatchString string) bool {
	reg := regexp.MustCompile(`^(>|>=|<|<=|!)?[0-9]\d*\.[0-9]\d*\.([0-9]\d*|x)$`)
	if !reg.MatchString(versionMatchString) {
		return false
	}

	// If there is a prefix, the last character must be a digit.
	if versionMatchString[len(versionMatchString)-1] == 'x' &&
		strings.ContainsAny(versionMatchString, "<>=!") {
		return false
	}

	return true
}
