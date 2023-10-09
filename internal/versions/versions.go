// Package version provides version related functions.
package versions

import (
	"fmt"
	"regexp"
	"strings"
)

// Version is a version number. The version number is split into three parts: the
// major version, the minor version, and the patch version. The major version is
// incremented when a backwards incompatible change is made. The minor version is
// incremented when a backwards compatible change is made. The patch version is
// incremented when a bug fix is made. The version number is also split into a
// pre-release version and a stable version. The pre-release version is
// incremented when a pre-release version is made. The pre-release version is
// only used for pre-release versions. The stable version is only used for stable
// versions. The pre-release version is only used when the version is not a
// stable version.
type Version struct {
	major int
	minor int
	patch int

	// PreRelease is the pre-release version. If this is set, the version is not
	// considered to be a stable release.
	preReleaseName string

	// preReleaseNumber is the pre-release version number. This is only used when
	// the version is not a stable version.
	// If the pre-release number is less than 0, the pre-release number is not
	// used.
	preReleaseNumber int
}

// New creates a new version.
// If the version is stable, the pre-release name should be empty and the
// pre-release number should -1.
func New(
	major int, minor int, patch int,
	preReleaseName string, preReleaseNumber int) (Version, error) {
	// The major, minor, and patch versions must be greater than or equal to 0.
	if major < 0 || minor < 0 || patch < 0 {
		return Version{},
			fmt.Errorf(
				"major, minor, and patch versions must be greater than or equal to 0")
	}

	// The patch version must be 0 if the pre-release name is not empty.
	if patch != 0 && preReleaseName != "" {
		return Version{},
			fmt.Errorf(
				"patch version must be 0 if the pre-release name is not empty")
	}

	// The pre-release name must not be empty if the pre-release number is not
	// less than 0.
	if preReleaseName == "" && preReleaseNumber >= 0 {
		return Version{},
			fmt.Errorf(
				"pre-release name must not be empty if the pre-release number is not less than 0")
	}

	// The pre-release number should be set to -1 if the pre-release name is
	// empty or if the pre-release number is less than 0 but not -1
	if preReleaseName == "" || preReleaseNumber < 0 {
		preReleaseNumber = -1
	}

	return Version{
		major:            major,
		minor:            minor,
		patch:            patch,
		preReleaseName:   preReleaseName,
		preReleaseNumber: preReleaseNumber,
	}, nil
}

// NewFromString creates a new version from a version string.
func NewFromString(versionString string) (Version, error) {
	if !IsValidVersionString(versionString) {
		return Version{}, fmt.Errorf("invalid version string %v", versionString)
	}

	var major, minor, patch int
	preReleaseName := ""
	preReleaseNumber := -1

	// Split versionString into parts using the "-" and "." character.
	versionStringParts := strings.FieldsFunc(versionString, func(r rune) bool {
		return r == '-' || r == '.'
	})

	// Parse major, minor, and patch versions.
	fmt.Sscanf(versionStringParts[0], "%d", &major)
	fmt.Sscanf(versionStringParts[1], "%d", &minor)
	fmt.Sscanf(versionStringParts[2], "%d", &patch)

	// Parse pre-release name and pre-release number.
	if len(versionStringParts) > 3 {
		preReleaseName = versionStringParts[3]
	}

	if len(versionStringParts) > 4 {
		fmt.Sscanf(versionStringParts[4], "%d", &preReleaseNumber)
	}

	return New(major, minor, patch, preReleaseName, preReleaseNumber)
}

// Major returns the major version.
func (v Version) Major() int {
	return v.major
}

// Minor returns the minor version.
func (v Version) Minor() int {
	return v.minor
}

// Patch returns the patch version.
func (v Version) Patch() int {
	return v.patch
}

// PreReleaseName returns the pre-release name.
func (v Version) PreReleaseName() string {
	return v.preReleaseName
}

// PreReleaseNumber returns the pre-release number.
func (v Version) PreReleaseNumber() int {
	return v.preReleaseNumber
}

func (v Version) IsStable() bool {
	return v.preReleaseName == ""
}

// String returns the string representation of the version.
func (v Version) String() string {
	if v.preReleaseName == "" {
		return fmt.Sprintf("%d.%d.%d", v.major, v.minor, v.patch)
	}

	if v.preReleaseNumber < 0 {
		return fmt.Sprintf("%d.%d.%d-%v", v.major, v.minor, v.patch, v.preReleaseName)
	}

	return fmt.Sprintf(
		"%d.%d.%d-%v.%d",
		v.major,
		v.minor,
		v.patch,
		v.preReleaseName,
		v.preReleaseNumber)
}

// ---------------------------------------------------------------------

// IsValidVersionString returns true if the version string is valid.
func IsValidVersionString(versionString string) bool {
	reg := regexp.MustCompile(
		`^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(-[a-z]+(\.(0|[1-9]\d*))?)?$`)
	if !reg.MatchString(versionString) {
		return false
	}

	// Split the version string into numeric and pre-release parts.
	versionParts := strings.Split(versionString, "-")

	// If the version string has a pre-release part, the patch version must be 0.
	if len(versionParts) == 2 {
		patchVersion := strings.Split(versionParts[0], ".")[2]
		if patchVersion != "0" {
			return false
		}
	}

	return true
}
