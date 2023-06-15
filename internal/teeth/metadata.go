package teeth

import (
	"fmt"
	"runtime"

	"github.com/lippkg/lip/pkg/versionmatches"
	"github.com/lippkg/lip/pkg/versions"
)

type Metadata struct {
	rawMetadata RawMetadata
}

func NewMetadata(jsonBytes []byte) (Metadata, error) {
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

	return Metadata{
		rawMetadata: rawMetadata,
	}, nil
}

func (m Metadata) JSON() ([]byte, error) {
	return m.rawMetadata.JSON()
}

func (m Metadata) Raw() RawMetadata {
	return m.rawMetadata
}

func (m Metadata) Tooth() string {
	return m.rawMetadata.Tooth
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

		dependencies[toothRepo] = versionMatch
	}

	return dependencies
}

func (m Metadata) Files() RawMetadataFiles {
	return m.rawMetadata.Files
}
