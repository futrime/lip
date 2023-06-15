package teeth

import (
	"fmt"
	"runtime"

	"github.com/lippkg/lip/internal/versions"
)

type Metadata struct {
	rawMetadata RawMetadata
	version     versions.Version
}

func NewMetadata(jsonBytes []byte) (Metadata, error) {
	rawMetadata, err := NewRawMetadata(jsonBytes)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to parse raw metadata: %w", err)
	}

	return NewMetadataFromRawMetadata(rawMetadata)
}

func NewMetadataFromRawMetadata(rawMetadata RawMetadata) (Metadata, error) {
	version, err := versions.NewFromString(rawMetadata.Version)
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

	return Metadata{
		rawMetadata: rawMetadata,
		version:     version,
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
	return m.version
}

func (m Metadata) Info() RawMetadataInfo {
	return m.rawMetadata.Info
}

func (m Metadata) Commands() RawMetadataCommands {
	return m.rawMetadata.Commands
}

func (m Metadata) Dependencies() map[string]string {
	return m.rawMetadata.Dependencies
}

func (m Metadata) Files() RawMetadataFiles {
	return m.rawMetadata.Files
}
