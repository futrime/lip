package specifier

import (
	"fmt"
	"strings"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/tooth"
)

// KindType is an enum that represents the type of a specifier.
type KindType int

const (
	ToothArchiveKind KindType = iota
	ToothRepoKind
)

// Specifier is a type that can be used to specify a tooth url/file or a requirement.
type Specifier struct {
	kind             KindType
	toothArchivePath path.Path
	toothRepoPath    string

	isToothVersionSpecified bool
	toothVersion            semver.Version
}

// Parse creates a new specifier from the given string.
func Parse(specifierString string) (Specifier, error) {

	specifierType := getSpecifierType(specifierString)

	switch specifierType {
	case ToothArchiveKind:
		toothArchivePath, err := path.Parse(specifierString)
		if err != nil {
			return Specifier{}, fmt.Errorf("invalid requirement specifier %v: %w",
				specifierString, err)
		}

		return Specifier{
			kind:             specifierType,
			toothArchivePath: toothArchivePath,
		}, nil

	case ToothRepoKind:
		// Parse the tooth repo and version.
		splittedSpecifier := strings.Split(specifierString, "@")

		toothRepoPath := splittedSpecifier[0]

		if !tooth.IsValidToothRepoPath(toothRepoPath) {
			return Specifier{}, fmt.Errorf("invalid requirement specifier %v: invalid tooth repo path",
				specifierString)
		}

		if len(splittedSpecifier) == 2 {
			toothVersion, err := semver.Parse(splittedSpecifier[1])
			if err != nil {
				return Specifier{}, fmt.Errorf("invalid requirement specifier %v: %w",
					specifierString, err)
			}

			return Specifier{
				kind:                    specifierType,
				toothRepoPath:           toothRepoPath,
				isToothVersionSpecified: true,
				toothVersion:            toothVersion,
			}, nil

		} else if len(splittedSpecifier) == 1 {
			return Specifier{
				kind:                    specifierType,
				toothRepoPath:           toothRepoPath,
				isToothVersionSpecified: false,
			}, nil
		} else {
			return Specifier{}, fmt.Errorf("invalid requirement specifier: %v: too many \"@\"s",
				specifierString)
		}
	}

	// Never reached.
	panic("unreachable")
}

// Kind returns the type of the specifier.
func (s Specifier) Kind() KindType {
	return s.kind
}

// ToothArchivePath returns the path of the tooth archive.
func (s Specifier) ToothArchivePath() (path.Path, error) {
	if s.Kind() != ToothArchiveKind {
		return path.Path{}, fmt.Errorf("specifier is not a tooth archive")
	}

	return s.toothArchivePath, nil
}

// ToothRepoPath returns the tooth repo of the specifier.
func (s Specifier) ToothRepoPath() (string, error) {
	if s.Kind() != ToothRepoKind {
		return "", fmt.Errorf("specifier is not a tooth repo")
	}

	return s.toothRepoPath, nil
}

// IsToothVersionSpecified returns whether the specifier has a tooth version.
func (s Specifier) IsToothVersionSpecified() (bool, error) {
	if s.Kind() != ToothRepoKind {
		return false, fmt.Errorf("specifier is not a tooth repo")
	}

	return s.isToothVersionSpecified, nil
}

// ToothVersion returns the version of the tooth.
func (s Specifier) ToothVersion() (semver.Version, error) {
	if s.Kind() != ToothRepoKind {
		return semver.Version{}, fmt.Errorf("specifier is not a tooth repo")
	}

	if !s.isToothVersionSpecified {
		return semver.Version{}, fmt.Errorf("tooth version is not specified")
	}

	return s.toothVersion, nil
}

// String returns the string representation of the specifier.
func (s Specifier) String() string {
	switch s.kind {
	case ToothArchiveKind:
		return s.toothArchivePath.LocalString()

	case ToothRepoKind:
		if s.isToothVersionSpecified {
			return s.toothRepoPath + "@" + s.toothVersion.String()
		} else {
			return s.toothRepoPath
		}
	}

	// Never reached.
	panic("unreachable")
}

func getSpecifierType(specifier string) KindType {
	// Prefer tooth repo specifier over tooth archive specifier.
	// This means that if a specifier is both a tooth repo specifier and a tooth archive
	// specifier, it will be treated as a tooth repo specifier.
	splittedSpecifier := strings.Split(specifier, "@")
	if len(splittedSpecifier) == 1 && tooth.IsValidToothRepoPath(specifier) {
		return ToothRepoKind
	} else if len(splittedSpecifier) == 2 && tooth.IsValidToothRepoPath(splittedSpecifier[0]) {
		return ToothRepoKind
	}

	return ToothArchiveKind
}
