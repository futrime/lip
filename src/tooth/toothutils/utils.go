package toothutils

import (
	"regexp"
)

// IsValidToothPath returns true if the tooth path is valid.
func IsValidToothPath(toothPath string) bool {
	reg := regexp.MustCompile(`^[a-z0-9][a-z0-9-_\.\/]*$`)

	return reg.FindString(toothPath) == toothPath
}
