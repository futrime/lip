package cmdlipinstall

import (
	"errors"
	"fmt"
	"net/http"
	"os"
	"regexp"
	"strings"

	"github.com/liteldev/lip/utils/version"
)

// SpecifierType is an enum that represents the type of a specifier.
type SpecifierType int

const (
	ToothFileSpecifierType SpecifierType = iota
	ToothURLSpecifierType
	RequirementSpecifierType
)

// Specifier is a type that can be used to specify a tooth url/file or a requirement.
type Specifier struct {
	specifierType SpecifierType
	toothPath     string
	toothURL      string
	toothRepo     string
	toothVersion  version.Version
}

// NewSpecifier creates a new specifier.
func NewSpecifier(specifierString string) (Specifier, error) {
	specifierType := getSpecifierType(specifierString)

	switch specifierType {
	case ToothFileSpecifierType:
		// Check if the tooth file exists.
		_, err := os.Stat(specifierString)

		if err != nil {
			return Specifier{}, errors.New("cannot access tooth file: " + specifierString)
		}

		return Specifier{
			specifierType: specifierType,
			toothPath:     specifierString,
		}, nil

	case ToothURLSpecifierType:
		// Check if the tooth url can be accessed.
		resp, err := http.Head(specifierString)

		if err != nil || resp.StatusCode != 200 {
			return Specifier{}, errors.New("cannot access tooth file URL: " + specifierString)
		}

		return Specifier{
			specifierType: specifierType,
			toothURL:      specifierString,
		}, nil

	case RequirementSpecifierType:
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

		var toothVersion version.Version
		var err error
		if len(splittedSpecifier) == 2 {
			toothVersion, err = version.NewFromString(splittedSpecifier[1])
			if err != nil {
				return Specifier{}, err
			}

			// Check if the tooth version is valid.
			err := validateToothRepoVersion(toothRepo, toothVersion)
			if err != nil {
				return Specifier{}, err
			}
		} else {
			// Fetch the latest version of the tooth repo.
			toothVersionList, err := fetchVersionList(toothRepo)
			if err != nil {
				return Specifier{}, err
			}

			if len(toothVersionList) == 0 {
				return Specifier{}, errors.New("no tooth version found for repo: " + toothRepo)
			}

			toothVersion = toothVersionList[len(toothVersionList)-1]
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
func (s Specifier) Type() SpecifierType {
	return s.specifierType
}

// String returns the string representation of the specifier.
func (s Specifier) String() string {
	switch s.specifierType {
	case ToothFileSpecifierType:
		return s.toothPath
	case ToothURLSpecifierType:
		return s.toothURL
	case RequirementSpecifierType:
		return s.toothRepo + "@" + s.toothVersion.String()
	}

	return ""
}

// ToothPath returns the path of the tooth file.
func (s Specifier) ToothPath() string {
	return s.toothPath
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
func (s Specifier) ToothVersion() version.Version {
	return s.toothVersion
}

// getSpecifierType gets the type of the requirement specifier.
func getSpecifierType(specifier string) SpecifierType {
	if strings.HasSuffix(specifier, ".tth") {
		if strings.HasPrefix(specifier, "http://") || strings.HasPrefix(specifier, "https://") {
			return ToothURLSpecifierType
		} else {
			return ToothFileSpecifierType
		}
	} else {
		return RequirementSpecifierType
	}
}
