package tooth

import (
	"encoding/json"
	"fmt"
	"net/url"
	"strings"

	gopath "path"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/tooth/migration/v1tov2"
	"github.com/xeipuuv/gojsonschema"

	log "github.com/sirupsen/logrus"
)

type Metadata struct {
	rawMetadata RawMetadata
}

type Info struct {
	Name        string
	Description string
	Author      string
	Tags        []string
}
type Commands struct {
	PreInstall    []string
	PostInstall   []string
	PreUninstall  []string
	PostUninstall []string
}

type Files struct {
	Place    []FilesPlaceItem
	Preserve []path.Path
	Remove   []path.Path
}

type FilesPlaceItem struct {
	Src  path.Path
	Dest path.Path
}

const expectedFormatVersion = 2

// MakeMetadata parses the given jsonBytes and returns a Metadata.
func MakeMetadata(jsonBytes []byte) (Metadata, error) {
	// Migrate if needed.
	formatVersion, err := parseFormatVersion(jsonBytes)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to get format version\n\t%w", err)
	}

	isMigrationNeeded := false
	switch formatVersion {
	case 1:
		migratedJSONBytes, err := v1tov2.Migrate(jsonBytes)
		if err != nil {
			return Metadata{}, fmt.Errorf("failed to migrate metadata\n\t%w", err)
		}

		isMigrationNeeded = true
		jsonBytes = migratedJSONBytes
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
		return Metadata{}, fmt.Errorf("failed to validate raw metadata\n\t%w", err)
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
		return Metadata{}, fmt.Errorf("failed to unmarshal raw metadata\n\t%w", err)
	}

	metadata, err := MakeMetadataFromRaw(rawMetadata)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to make metadata\n\t%w", err)
	}

	// Warn for obsolete tooth.json.
	if isMigrationNeeded {
		log.Warnf("tooth.json format version %v of %v is deprecated. This tooth might be obsolete.",
			formatVersion, metadata.ToothRepoPath())
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
		return Metadata{}, fmt.Errorf("failed to parse version\n\t%w", err)
	}

	return Metadata{rawMetadata}, nil
}

func (m Metadata) ToothRepoPath() string {
	return m.rawMetadata.Tooth
}

func (m Metadata) Version() semver.Version {
	return semver.MustParse(m.rawMetadata.Version)
}

func (m Metadata) Info() Info {
	return Info(m.rawMetadata.Info)
}

func (m Metadata) AssetURL() (*url.URL, error) {
	return url.Parse(m.rawMetadata.AssetURL)
}

func (m Metadata) Commands() Commands {
	return Commands(m.rawMetadata.Commands)
}

func (m Metadata) Dependencies() (map[string]semver.Range, error) {
	dependencies := make(map[string]semver.Range)

	for toothRepoPath, dep := range m.rawMetadata.Dependencies {
		versionRange, err := semver.ParseRange(dep)
		if err != nil {
			return nil, fmt.Errorf("failed to parse version range\n\t%w", err)
		}

		dependencies[toothRepoPath] = versionRange
	}

	return dependencies, nil
}

func (m Metadata) DependenciesAsStrings() map[string]string {
	dependencies := make(map[string]string)

	for toothRepoPath, dep := range m.rawMetadata.Dependencies {
		dependencies[toothRepoPath] = dep
	}

	return dependencies
}

func (m Metadata) Prerequisites() (map[string]semver.Range, error) {
	prerequisites := make(map[string]semver.Range)

	for toothRepoPath, prereq := range m.rawMetadata.Prerequisites {
		versionRange, err := semver.ParseRange(prereq)
		if err != nil {
			return nil, fmt.Errorf("failed to parse version range\n\t%w", err)
		}

		prerequisites[toothRepoPath] = versionRange
	}

	return prerequisites, nil
}

func (m Metadata) PrerequisitesAsStrings() map[string]string {
	prerequisites := make(map[string]string)

	for toothRepoPath, prereq := range m.rawMetadata.Prerequisites {
		prerequisites[toothRepoPath] = prereq
	}

	return prerequisites
}

func (m Metadata) Files() (Files, error) {
	if !m.IsWildcardPopulated() {
		return Files{}, fmt.Errorf("wildcard is not populated")
	}

	place := make([]FilesPlaceItem, 0)
	for _, placeItem := range m.rawMetadata.Files.Place {
		src, err := path.Parse(placeItem.Src)
		if err != nil {
			return Files{}, fmt.Errorf("failed to parse source path\n\t%w", err)
		}

		dest, err := path.Parse(placeItem.Dest)
		if err != nil {
			return Files{}, fmt.Errorf("failed to parse destination path\n\t%w", err)
		}

		place = append(place, FilesPlaceItem{
			Src:  src,
			Dest: dest,
		})
	}

	preserve := make([]path.Path, 0)
	for _, preserveItem := range m.rawMetadata.Files.Preserve {
		preservePath, err := path.Parse(preserveItem)
		if err != nil {
			return Files{}, fmt.Errorf("failed to parse preserve path\n\t%w", err)
		}

		preserve = append(preserve, preservePath)
	}

	remove := make([]path.Path, 0)
	for _, removeItem := range m.rawMetadata.Files.Remove {
		removePath, err := path.Parse(removeItem)
		if err != nil {
			return Files{}, fmt.Errorf("failed to parse remove path\n\t%w", err)
		}

		remove = append(remove, removePath)
	}

	return Files{
		Place:    place,
		Preserve: preserve,
		Remove:   remove,
	}, nil
}

func (m Metadata) IsWildcardPopulated() bool {
	for _, placeItem := range m.rawMetadata.Files.Place {
		if strings.Contains(placeItem.Src, "*") {
			return false
		}
	}

	return true
}

func (m Metadata) MarshalJSON() ([]byte, error) {
	jsonBytes, err := json.MarshalIndent(m.rawMetadata, "", "    ")

	if err != nil {
		return nil, fmt.Errorf("failed to marshal raw metadata\n\t%w", err)
	}

	return jsonBytes, nil
}

func (m Metadata) ToPlatformSpecific(goos string, goarch string) (Metadata, error) {
	raw := m.rawMetadata

	if raw.Dependencies == nil {
		raw.Dependencies = make(map[string]string)
	}
	if raw.Prerequisites == nil {
		raw.Prerequisites = make(map[string]string)
	}
	raw.Platforms = nil

	for _, platformItem := range m.rawMetadata.Platforms {
		if platformItem.GOOS != goos {
			continue
		}

		if platformItem.GOARCH != "" && platformItem.GOARCH != goarch {
			continue
		}

		if platformItem.AssetURL != "" {
			raw.AssetURL = platformItem.AssetURL
		}

		raw.Commands.PreInstall = append(raw.Commands.PreInstall, platformItem.Commands.PreInstall...)
		raw.Commands.PostInstall = append(raw.Commands.PostInstall, platformItem.Commands.PostInstall...)
		raw.Commands.PreUninstall = append(raw.Commands.PreUninstall, platformItem.Commands.PreUninstall...)
		raw.Commands.PostUninstall = append(raw.Commands.PostUninstall, platformItem.Commands.PostUninstall...)

		for toothRepoPath, dep := range platformItem.Dependencies {
			raw.Dependencies[toothRepoPath] = dep
		}

		for toothRepoPath, prereq := range platformItem.Prerequisites {
			raw.Prerequisites[toothRepoPath] = prereq
		}

		raw.Files.Place = append(raw.Files.Place, platformItem.Files.Place...)
		raw.Files.Preserve = append(raw.Files.Preserve, platformItem.Files.Preserve...)
		raw.Files.Remove = append(raw.Files.Remove, platformItem.Files.Remove...)
	}

	return MakeMetadataFromRaw(raw)
}

// ToFilePathPrefixPrepended prepends the given prefix to files.place field of metadata.
func (m Metadata) ToFilePathPrefixPrepended(prefix path.Path) Metadata {
	newRaw := m.rawMetadata

	newPlace := make([]RawMetadataFilesPlaceItem, 0)

	for _, placeItem := range m.rawMetadata.Files.Place {
		newPlace = append(newPlace, RawMetadataFilesPlaceItem{
			Src:  gopath.Join(prefix.String(), placeItem.Src),
			Dest: placeItem.Dest,
		})
	}

	newRaw.Files.Place = newPlace

	return Metadata{newRaw}
}

// ToWildcardPopulated populates wildcards in files.place field of metadata.
func (m Metadata) ToWildcardPopulated(filePaths []path.Path) (Metadata, error) {
	debugLogger := log.WithFields(log.Fields{
		"package": "tooth",
		"method":  "Metadata.ToWildcardPopulated",
	})

	newRaw := m.rawMetadata

	newPlace := make([]RawMetadataFilesPlaceItem, 0)

	for _, placeItem := range m.rawMetadata.Files.Place {
		// If not wildcard, just append.
		if !strings.HasSuffix(placeItem.Src, "*") {
			newPlace = append(newPlace, placeItem)
			continue
		}

		sourcePathPrefix, err := path.Parse(strings.TrimSuffix(placeItem.Src, "*"))
		if err != nil {
			return Metadata{}, fmt.Errorf("failed to parse source path prefix\n\t%w", err)
		}

		destPathPrefix, err := path.Parse(placeItem.Dest)
		if err != nil {
			return Metadata{}, fmt.Errorf("failed to parse destination path prefix\n\t%w", err)
		}

		for _, filePath := range filePaths {
			if !filePath.HasPrefix(sourcePathPrefix) {
				continue
			}

			relFilePath := filePath.TrimPrefix(sourcePathPrefix)

			newPlace = append(newPlace, RawMetadataFilesPlaceItem{
				Src:  filePath.String(),
				Dest: destPathPrefix.Join(relFilePath).String(),
			})

			debugLogger.Debugf("Populated %v to %v", filePath, destPathPrefix.Join(relFilePath))
		}
	}

	newRaw.Files.Place = newPlace

	newMetadata := Metadata{newRaw}

	return newMetadata, nil
}

func parseFormatVersion(jsonBytes []byte) (int, error) {
	jsonData := make(map[string]interface{})
	err := json.Unmarshal(jsonBytes, &jsonData)
	if err != nil {
		return 0, fmt.Errorf("failed to parse json\n\t%w", err)
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
