package teeth

import (
	"encoding/json"
	"fmt"
	"runtime"
	"strings"

	"github.com/lippkg/lip/internal/teeth/migration/v1tov2"
	"github.com/lippkg/lip/internal/versionmatches"
	"github.com/lippkg/lip/internal/versions"
)

type Metadata struct {
	rawMetadata RawMetadata
}

func NewMetadata(jsonBytes []byte) (Metadata, error) {
	var err error

	formatVersion, err := getFormatVersion(jsonBytes)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to get format version: %w", err)
	}

	switch formatVersion {
	case 1:
		jsonBytes, err = v1tov2.Migrate(jsonBytes)
		if err != nil {
			return Metadata{}, fmt.Errorf("failed to migrate metadata: %w", err)
		}
	}

	rawMetadata, err := NewRawMetadata(jsonBytes)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to parse raw metadata: %w", err)
	}

	return NewMetadataFromRawMetadata(rawMetadata)
}

func NewMetadataFromRawMetadata(rawMetadata RawMetadata) (Metadata, error) {
	_, err := versions.NewFromString(rawMetadata.Version)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to parse version: %w", err)
	}

	for _, platform := range rawMetadata.Platforms {
		// If the platform is not the same as the current platform, skip it.
		// However, if the platform is empty, we want to include it.
		if platform.GOARCH != "" && platform.GOARCH != runtime.GOARCH {
			continue
		}

		// If the platform is not the same as the current platform, skip it.
		if platform.GOOS != runtime.GOOS {
			continue
		}

		// If the platform is the same as the current platform, replace the content.
		// Note that if duplicate keys exist, the last one wins.
		rawMetadata.Commands = platform.Commands
		rawMetadata.Dependencies = platform.Dependencies
		rawMetadata.Prerequisites = platform.Prerequisites
		rawMetadata.Files = platform.Files
	}
	rawMetadata.Platforms = nil

	for _, dep := range rawMetadata.Dependencies {
		_, err := versionmatches.NewGroupFromString(dep)
		if err != nil {
			return Metadata{},
				fmt.Errorf("failed to parse dependency %v: %w", dep, err)
		}
	}

	for _, dep := range rawMetadata.Prerequisites {
		_, err := versionmatches.NewGroupFromString(dep)
		if err != nil {
			return Metadata{},
				fmt.Errorf("failed to parse prerequisite %v: %w", dep, err)
		}
	}

	return Metadata{
		rawMetadata: rawMetadata,
	}, nil
}

func (m Metadata) JSON(indent bool) ([]byte, error) {
	return m.rawMetadata.JSON(indent)
}

func (m Metadata) Raw() RawMetadata {
	return m.rawMetadata
}

func (m Metadata) Tooth() string {
	// To lower case to make it case insensitive.
	return strings.ToLower(m.rawMetadata.Tooth)
}

func (m Metadata) Version() versions.Version {
	version, err := versions.NewFromString(m.rawMetadata.Version)
	if err != nil {
		panic(err)
	}

	return version
}

func (m Metadata) Info() RawMetadataInfo {
	return m.rawMetadata.Info
}

func (m Metadata) Commands() RawMetadataCommands {
	return m.rawMetadata.Commands
}

func (m Metadata) Dependencies() map[string]versionmatches.Group {
	dependencies := make(map[string]versionmatches.Group)

	for toothRepo, dep := range m.rawMetadata.Dependencies {
		versionMatch, err := versionmatches.NewGroupFromString(dep)
		if err != nil {
			panic(err)
		}

		// To lower case to make it case insensitive.
		dependencies[strings.ToLower(toothRepo)] = versionMatch
	}

	return dependencies
}

func (m Metadata) Prerequisites() map[string]versionmatches.Group {
	prerequisites := make(map[string]versionmatches.Group)

	for toothRepo, prereq := range m.rawMetadata.Prerequisites {
		versionMatch, err := versionmatches.NewGroupFromString(prereq)
		if err != nil {
			panic(err)
		}

		// To lower case to make it case insensitive.
		prerequisites[strings.ToLower(toothRepo)] = versionMatch
	}

	return prerequisites
}

func (m Metadata) Files() RawMetadataFiles {
	return m.rawMetadata.Files
}

// ---------------------------------------------------------------------

func getFormatVersion(jsonBytes []byte) (int, error) {
	var jsonData map[string]interface{}
	err := json.Unmarshal(jsonBytes, &jsonData)
	if err != nil {
		return 0, fmt.Errorf("failed to parse json: %w", err)
	}

	formatVersion, ok := jsonData["format_version"]
	if !ok {
		return 0, fmt.Errorf("missing format_version")
	}

	formatVersionFloat64, ok := formatVersion.(float64)
	if !ok {
		return 0, fmt.Errorf("format_version is not an int")
	}

	return int(formatVersionFloat64), nil
}
