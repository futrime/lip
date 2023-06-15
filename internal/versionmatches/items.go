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

// A version match item is a version match for a single criterion.
type Item struct {
	version   versions.Version
	matchType MatchType
}

// NewItem creates a new version match item.
func NewItem(version versions.Version, matchType MatchType) (Item, error) {
	// The version should not have a prerelease.
	if matchType == CompatibleMatchType && (!version.IsStable() || version.Patch() != 0) {
		return Item{}, fmt.Errorf("cannot create a compatible version match for a prerelease or a patch version")
	}

	// The match type must be valid.
	if matchType < EqualMatchType || matchType > CompatibleMatchType {
		return Item{}, fmt.Errorf("invalid match type")
	}

	return Item{
		version:   version,
		matchType: matchType,
	}, nil
}

// NewItemFromString creates a new version match item from a string.
func NewItemFromString(versionMatchString string) (Item, error) {
	if !isValidVersionMatchString(versionMatchString) {
		return Item{}, fmt.Errorf("invalid version match string")
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
		return Item{}, fmt.Errorf("cannot parse version: %w", err)
	}

	return NewItem(version, matchType)
}

// Match matches the version to the version match.
func (item Item) Match(version versions.Version) bool {
	switch item.matchType {
	case EqualMatchType:
		return versions.Equal(version, item.version)
	case InequalMatchType:
		return !versions.Equal(version, item.version)
	case GreaterThanMatchType:
		return versions.GreaterThan(version, item.version)
	case GreaterThanOrEqualMatchType:
		return versions.GreaterThanOrEqual(version, item.version)
	case LessThanMatchType:
		return versions.LessThan(version, item.version)
	case LessThanOrEqualMatchType:
		return versions.LessThanOrEqual(version, item.version)
	case CompatibleMatchType:
		return versions.Compatible(version, item.version)
	}

	panic("unreachable")
}

// String returns the string representation of the version match.
func (item Item) String() string {
	switch item.matchType {
	case EqualMatchType:
		return item.version.String()
	case InequalMatchType:
		return "!" + item.version.String()
	case GreaterThanMatchType:
		return ">" + item.version.String()
	case GreaterThanOrEqualMatchType:
		return ">=" + item.version.String()
	case LessThanMatchType:
		return "<" + item.version.String()
	case LessThanOrEqualMatchType:
		return "<=" + item.version.String()
	case CompatibleMatchType:
		return strings.TrimSuffix(item.version.String(), "0") + "x"
	}

	panic("unreachable")
}

// ---------------------------------------------------------------------

// isValidVersionMatchString returns true if the version match string is valid.
func isValidVersionMatchString(versionMatchString string) bool {
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
