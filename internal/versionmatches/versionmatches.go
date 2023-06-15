// Package versionmatch provides version match functionality.
package versionmatches

import (
	"fmt"
	"regexp"
	"strings"

	"github.com/lippkg/lip/internal/versions"
)

// MatchType is an enum that represents the type of a version match.
type MatchType int

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
	version versions.Version
	// The match type.
	matchType MatchType
}

// New creates a new version match.
func New(version versions.Version, matchType MatchType) (VersionMatch, error) {
	// The match type must be valid.
	if matchType < EqualMatchType || matchType > CompatibleMatchType {
		return VersionMatch{}, fmt.Errorf("invalid match type")
	}

	return VersionMatch{
		version:   version,
		matchType: matchType,
	}, nil
}

// NewFromString creates a new version match from a string.
func NewFromString(versionMatchString string) (VersionMatch, error) {
	if !IsValidVersionMatchString(versionMatchString) {
		return VersionMatch{}, fmt.Errorf("invalid version match string")
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
		versionMatchString = versionMatchString[:len(versionMatchString)-1] + "0"
	} else {
		matchType = EqualMatchType
	}

	// Create the version.
	version, err := versions.NewFromString(versionMatchString)
	if err != nil {
		return VersionMatch{}, fmt.Errorf("cannot parse version: %w", err)
	}

	return New(version, matchType)
}

// Match matches the version to the version match.
func (vm VersionMatch) Match(version versions.Version) bool {
	switch vm.matchType {
	case EqualMatchType:
		return versions.Equal(version, vm.version)
	case InequalMatchType:
		return !versions.Equal(version, vm.version)
	case GreaterThanMatchType:
		return versions.GreaterThan(version, vm.version)
	case GreaterThanOrEqualMatchType:
		return versions.GreaterThanOrEqual(version, vm.version)
	case LessThanMatchType:
		return versions.LessThan(version, vm.version)
	case LessThanOrEqualMatchType:
		return versions.LessThanOrEqual(version, vm.version)
	case CompatibleMatchType:
		return versions.Compatible(version, vm.version)
	}

	// This should never happen.
	return false
}

// String returns the string representation of the version match.
func (vm VersionMatch) String() string {
	switch vm.matchType {
	case EqualMatchType:
		return vm.version.String()
	case InequalMatchType:
		return "!" + vm.version.String()
	case GreaterThanMatchType:
		return ">" + vm.version.String()
	case GreaterThanOrEqualMatchType:
		return ">=" + vm.version.String()
	case LessThanMatchType:
		return "<" + vm.version.String()
	case LessThanOrEqualMatchType:
		return "<=" + vm.version.String()
	case CompatibleMatchType:
		return strings.TrimSuffix(vm.version.String(), "0") + "x"
	}

	// This should never happen.
	return ""
}

// ---------------------------------------------------------------------

// IsValidVersionMatchString returns true if the version match string is valid.
func IsValidVersionMatchString(versionMatchString string) bool {
	reg := regexp.MustCompile(`^(>|>=|<|<=|!)?(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*|x)$`)
	if !reg.MatchString(versionMatchString) {
		return false
	}

	// If there is a prefix, the last character must be a digit.
	if strings.HasSuffix(versionMatchString, "x") &&
		strings.ContainsAny(versionMatchString, "<>=!") {
		return false
	}

	return true
}
