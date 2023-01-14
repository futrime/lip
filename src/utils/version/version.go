// Package version provides version related functions.
package version

import (
	"errors"
	"fmt"
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
func (v Version) New(
	major int, minor int, patch int,
	preReleaseName string, preReleaseNumber int) (Version, error) {
	// The major, minor, and patch versions must be greater than or equal to 0.
	if major < 0 || minor < 0 || patch < 0 {
		return Version{},
			errors.New("major, minor, and patch versions must be greater than or equal to 0")
	}

	// The pre-release name must not be empty if the pre-release number is not
	// less than 0.
	if preReleaseName == "" && preReleaseNumber >= 0 {
		return Version{},
			errors.New("pre-release name must not be empty if the pre-release number is not less than 0")
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
		return fmt.Sprintf("%d.%d.%d-%s", v.major, v.minor, v.patch, v.preReleaseName)
	}

	return fmt.Sprintf(
		"%d.%d.%d-%s.%d",
		v.major,
		v.minor,
		v.patch,
		v.preReleaseName,
		v.preReleaseNumber)
}
