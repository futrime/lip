package tooth

import (
	"encoding/json"
	"fmt"
	"strings"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/tooth/migration/v1tov2"
	"github.com/xeipuuv/gojsonschema"

	log "github.com/sirupsen/logrus"
)

type Metadata struct {
	rawMetadata RawMetadata
}

const expectedFormatVersion = 2

// MakeMetadata parses the given jsonBytes and returns a Metadata.
func MakeMetadata(jsonBytes []byte) (Metadata, error) {
	// Migrate if needed.
	formatVersion, err := parseFormatVersion(jsonBytes)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to get format version: %w", err)
	}

	isMigrationNeeded := false
	switch formatVersion {
	case 1:
		jsonBytes, err = v1tov2.Migrate(jsonBytes)
		if err != nil {
			return Metadata{}, fmt.Errorf("failed to migrate metadata: %w", err)
		}

		isMigrationNeeded = true
		fallthrough

	case expectedFormatVersion:
		// Do nothing.

	default:
		return Metadata{}, fmt.Errorf("unsupported format version: %v", formatVersion)
	}

	// Validate JSON against schema
	schemaLoader := gojsonschema.NewStringLoader(metadataJSONSchema)
	documentLoader := gojsonschema.NewBytesLoader(jsonBytes)

	validationResult, err := gojsonschema.Validate(schemaLoader, documentLoader)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to validate raw metadata: %w", err)
	}

	if !validationResult.Valid() {
		errors := make([]string, 0)
		for _, err := range validationResult.Errors() {
			errors = append(errors, err.String())
		}
		return Metadata{}, fmt.Errorf("raw metadata is invalid: %v",
			strings.Join(errors, ", "))
	}

	// Unmarshal JSON
	var rawMetadata RawMetadata
	if err := json.Unmarshal(jsonBytes, &rawMetadata); err != nil {
		return Metadata{}, fmt.Errorf("failed to unmarshal raw metadata: %w", err)
	}

	metadata, err := MakeMetadataFromRaw(rawMetadata)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to make metadata: %w", err)
	}

	// Warn for obsolete tooth.json.
	if isMigrationNeeded {
		log.Warnf("tooth.json format of %v is deprecated. This tooth might be obsolete.", rawMetadata.Tooth)
	}

	return metadata, nil
}

// MakeMetadataFromRaw returns a Metadata from the given RawMetadata.
func MakeMetadataFromRaw(rawMetadata RawMetadata) (Metadata, error) {
	// Validate metadata.
	if rawMetadata.FormatVersion != expectedFormatVersion {
		return Metadata{}, fmt.Errorf("unsupported format version: %v", rawMetadata.FormatVersion)
	}

	if !IsValidToothRepoPath(rawMetadata.Tooth) {
		return Metadata{}, fmt.Errorf("invalid tooth repo path %v", rawMetadata.Tooth)
	}

	if _, err := semver.Parse(rawMetadata.Version); err != nil {
		return Metadata{}, fmt.Errorf("failed to parse version: %w", err)
	}

	return Metadata{rawMetadata}, nil
}

func (m Metadata) Raw() RawMetadata {
	return m.rawMetadata
}

func (m Metadata) ToothRepoPath() string {
	return m.rawMetadata.Tooth
}

func (m Metadata) Version() semver.Version {
	return semver.MustParse(m.rawMetadata.Version)
}

func (m Metadata) Info() RawMetadataInfo {
	return m.rawMetadata.Info
}

func (m Metadata) AssetURL() string {
	return m.rawMetadata.AssetURL
}

func (m Metadata) Commands() RawMetadataCommands {
	return m.rawMetadata.Commands
}

func (m Metadata) Dependencies() map[string]semver.Range {
	dependencies := make(map[string]semver.Range)

	for toothRepoPath, dep := range m.rawMetadata.Dependencies {
		versionRange, err := semver.ParseRange(dep)
		if err != nil {
			panic(err)
		}

		dependencies[toothRepoPath] = versionRange
	}

	return dependencies
}

func (m Metadata) Prerequisites() map[string]semver.Range {
	prerequisites := make(map[string]semver.Range)

	for toothRepoPath, prereq := range m.rawMetadata.Prerequisites {
		versionRange, err := semver.ParseRange(prereq)
		if err != nil {
			panic(err)
		}

		prerequisites[toothRepoPath] = versionRange
	}

	return prerequisites
}

func (m Metadata) Files() RawMetadataFiles {
	return m.rawMetadata.Files
}

func (m Metadata) MarshalJSON() ([]byte, error) {
	jsonBytes, err := json.MarshalIndent(m.rawMetadata, "", "    ")

	if err != nil {
		return nil, fmt.Errorf("failed to marshal raw metadata: %w", err)
	}

	return jsonBytes, nil
}

func parseFormatVersion(jsonBytes []byte) (int, error) {
	jsonData := make(map[string]interface{})
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
