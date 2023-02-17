package specifiers

import (
	"errors"
	"fmt"
	"net/http"
	"os"
	"regexp"
	"strings"

	"github.com/liteldev/lip/registry"
	"github.com/liteldev/lip/tooth/toothrepo"
	"github.com/liteldev/lip/utils/versions"
)

// SpecifierKind is an enum that represents the type of a specifier.
type SpecifierKind int

const (
	ToothFileKind SpecifierKind = iota
	ToothURLKind
	RequirementKind
)

// Specifier is a type that can be used to specify a tooth url/file or a requirement.
type Specifier struct {
	specifierType SpecifierKind
	toothFilePath string
	toothURL      string
	toothRepo     string
	toothVersion  versions.Version
}

// New creates a new specifier.
func New(specifierString string) (Specifier, error) {
	var err error

	specifierType := getSpecifierType(specifierString)

	switch specifierType {
	case ToothFileKind:
		// Check if the tooth file exists.
		_, err := os.Stat(specifierString)

		if err != nil {
			return Specifier{}, errors.New("cannot access tooth file: " + specifierString)
		}

		return Specifier{
			specifierType: specifierType,
			toothFilePath: specifierString,
		}, nil

	case ToothURLKind:
		// Check if the tooth url can be accessed.
		resp, err := http.Head(specifierString)

		if err != nil || resp.StatusCode != 200 {
			return Specifier{}, errors.New("cannot access tooth file URL: " + specifierString)
		}

		return Specifier{
			specifierType: specifierType,
			toothURL:      specifierString,
		}, nil

	case RequirementKind:
		// Specifier string should be lower case.
		specifierString = strings.ToLower(specifierString)

		reg := regexp.MustCompile(`^[a-z0-9][a-z0-9-_\.\/]*(@\d+\.\d+\.\d+(-[a-z]+(\.\d+)?)?)?$`)

		// If not matched or the matched string is not the same as the specifier, it is an
		// invalid requirement specifier.
		if reg.FindString(specifierString) != specifierString {
			return Specifier{}, errors.New("invalid requirement specifier: " + specifierString)
		}

		// Parse the tooth repo and version.
		splittedSpecifier := strings.Split(specifierString, "@")

		toothRepo := splittedSpecifier[0]

		if !strings.Contains(toothRepo, "/") {
			toothRepo, err = registry.LookupAlias(toothRepo)
			if err != nil {
				return Specifier{}, err
			}
		}

		var toothVersion versions.Version

		if len(splittedSpecifier) == 2 {
			toothVersion, err = versions.NewFromString(splittedSpecifier[1])
			if err != nil {
				return Specifier{}, err
			}

			// Check if the tooth version is valid.
			err := toothrepo.ValidateVersion(toothRepo, toothVersion)
			if err != nil {
				return Specifier{}, err
			}
		} else {
			// Fetch the latest version of the tooth repo.
			toothVersionList, err := toothrepo.FetchVersionList(toothRepo)
			if err != nil {
				return Specifier{}, err
			}

			if len(toothVersionList) == 0 {
				return Specifier{}, errors.New("no tooth version found for repo: " + toothRepo)
			}

			// toothVersionList is sorted in descending order.
			// Find the first stable version
			isFound := false
			for _, v := range toothVersionList {
				if v.IsStable() {
					toothVersion = v
					isFound = true
					break
				}
			}

			// If no stable version is found, return an error.
			if !isFound {
				return Specifier{}, errors.New("no stable tooth version found for repo: " + toothRepo + ". You must specify a version manually")
			}
		}

		return Specifier{
			specifierType: specifierType,
			toothRepo:     toothRepo,
			toothVersion:  toothVersion,
		}, nil
	}

	// If the specifier type is not valid, return an error.
	return Specifier{}, errors.New("invalid specifier type" + fmt.Sprintf("%d", specifierType))
}

// Type returns the type of the specifier.
func (s Specifier) Type() SpecifierKind {
	return s.specifierType
}

// String returns the string representation of the specifier.
func (s Specifier) String() string {
	switch s.specifierType {
	case ToothFileKind:
		return s.toothFilePath
	case ToothURLKind:
		return s.toothURL
	case RequirementKind:
		return s.toothRepo + "@" + s.toothVersion.String()
	}

	return ""
}

// ToothFilePath returns the path of the tooth file.
func (s Specifier) ToothFilePath() string {
	return s.toothFilePath
}

// ToothRepo returns the tooth repo of the specifier.
func (s Specifier) ToothRepo() string {
	return s.toothRepo
}

// ToothURL returns the url of the tooth file.
func (s Specifier) ToothURL() string {
	return s.toothURL
}

// ToothVersion returns the version of the tooth.
func (s Specifier) ToothVersion() versions.Version {
	return s.toothVersion
}

// getSpecifierType gets the type of the requirement specifier.
func getSpecifierType(specifier string) SpecifierKind {
	if strings.HasSuffix(specifier, ".tth") {
		if strings.HasPrefix(specifier, "http://") || strings.HasPrefix(specifier, "https://") {
			return ToothURLKind
		} else {
			return ToothFileKind
		}
	} else {
		return RequirementKind
	}
}
