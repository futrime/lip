// Package versionmatch provides version match functionality.
package versionmatch

import (
	"errors"
	"strings"

	versionutils "github.com/liteldev/lip/utils/version"
)

type MatchType int

// The match types.
const (
	EqualMatchType MatchType = iota
	InequalMatchType
	GreaterThanMatchType
	GreaterThanOrEqualMatchType
	LessThanMatchType
	LessThanOrEqualMatchType
	CompatibleMatchType
)

// VersionMatch is a version match. The version match is used to match a version
// to a version range.
type VersionMatch struct {
	version versionutils.Version
	// The match type.
	matchType MatchType
}

// New creates a new version match.
func New(version versionutils.Version, matchType MatchType) (VersionMatch, error) {
	// The match type must be valid.
	if matchType < EqualMatchType || matchType > CompatibleMatchType {
		return VersionMatch{}, errors.New("invalid match type")
	}

	return VersionMatch{
		version:   version,
		matchType: matchType,
	}, nil
}

// NewFromString creates a new version match from a string.
func NewFromString(versionMatchString string) (VersionMatch, error) {
	if !IsValidVersionMatchString(versionMatchString) {
		return VersionMatch{}, errors.New("invalid version match string")
	}

	// Get the match type.
	var matchType MatchType
	if strings.HasPrefix(versionMatchString, "!") {
		matchType = InequalMatchType
		versionMatchString = versionMatchString[1:]
	} else if strings.HasPrefix(versionMatchString, ">=") {
		matchType = GreaterThanOrEqualMatchType
		versionMatchString = versionMatchString[2:]
	} else if strings.HasPrefix(versionMatchString, ">") {
		matchType = GreaterThanMatchType
		versionMatchString = versionMatchString[1:]
	} else if strings.HasPrefix(versionMatchString, "<=") {
		matchType = LessThanOrEqualMatchType
		versionMatchString = versionMatchString[2:]
	} else if strings.HasPrefix(versionMatchString, "<") {
		matchType = LessThanMatchType
		versionMatchString = versionMatchString[1:]
	} else if strings.HasSuffix(versionMatchString, "x") {
		matchType = CompatibleMatchType
		versionMatchString = versionMatchString[:len(versionMatchString)-2] + "0"
	} else {
		matchType = EqualMatchType
	}

	// Create the version.
	version, err := versionutils.NewFromString(versionMatchString)
	if err != nil {
		return VersionMatch{}, err
	}

	return New(version, matchType)
}

// Match matches the version to the version match.
func (vm VersionMatch) Match(version versionutils.Version) bool {
	switch vm.matchType {
	case EqualMatchType:
		return versionutils.Equal(version, vm.version)
	case InequalMatchType:
		return !versionutils.Equal(version, vm.version)
	case GreaterThanMatchType:
		return versionutils.GreaterThan(version, vm.version)
	case GreaterThanOrEqualMatchType:
		return versionutils.GreaterThanOrEqual(version, vm.version)
	case LessThanMatchType:
		return versionutils.LessThan(version, vm.version)
	case LessThanOrEqualMatchType:
		return versionutils.LessThanOrEqual(version, vm.version)
	case CompatibleMatchType:
		return versionutils.Compatible(version, vm.version)
	}

	// This should never happen.
	return false
}
