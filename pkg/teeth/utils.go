package teeth

import (
	"bufio"
	"bytes"
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"

	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/downloading"
	"github.com/lippkg/lip/pkg/versions"
)

// CheckIsToothInstalled checks if a tooth is installed.
func CheckIsToothInstalled(ctx contexts.Context, toothRepo string) (bool, error) {
	var err error

	metadataList, err := GetAllInstalledToothMetadata(ctx)
	if err != nil {
		return false, fmt.Errorf(
			"failed to list all installed tooth metadata: %w", err)
	}

	for _, metadata := range metadataList {
		if metadata.Tooth() == toothRepo {
			return true, nil
		}
	}

	return false, nil
}

// CheckIsToothManuallyInstalled checks if a tooth is manually installed.
func CheckIsToothManuallyInstalled(ctx contexts.Context,
	toothRepo string) (bool, error) {
	var err error

	// TODO: Check if the tooth is manually installed.

	return false, err
}

// CheckIsValidToothRepo returns true if the tooth repository is valid.
func CheckIsValidToothRepo(toothRepo string) bool {
	reg := regexp.MustCompile(`^[a-z0-9][a-z0-9-_\.\/]*$`)

	return reg.FindString(toothRepo) == toothRepo
}

// GetAllInstalledToothMetadata lists all installed tooth metadata.
func GetAllInstalledToothMetadata(ctx contexts.Context) ([]Metadata, error) {
	var err error

	var metadataList []Metadata

	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return nil, fmt.Errorf("failed to get metadata directory: %w", err)
	}

	filePaths, err := filepath.Glob(filepath.Join(metadataDir, "*.json"))
	if err != nil {
		return nil, fmt.Errorf("failed to list metadata files: %w", err)
	}

	for _, filePath := range filePaths {
		jsonBytes, err := os.ReadFile(filePath)
		if err != nil {
			return nil, fmt.Errorf("failed to read metadata file: %w", err)
		}

		metadata, err := NewMetadata(jsonBytes)
		if err != nil {
			return nil, fmt.Errorf("failed to parse metadata file: %w", err)
		}

		metadataList = append(metadataList, metadata)
	}

	// Sort the metadata list in case-insensitive ascending order of the tooth
	// repository.
	sort.Slice(metadataList, func(i, j int) bool {
		return strings.ToLower(metadataList[i].Tooth()) < strings.ToLower(
			metadataList[j].Tooth())
	})

	return metadataList, nil
}

// GetInstalledToothMetadata finds the installed tooth metadata.
func GetInstalledToothMetadata(ctx contexts.Context, toothRepo string) (Metadata,
	error) {
	var err error

	metadataList, err := GetAllInstalledToothMetadata(ctx)
	if err != nil {
		return Metadata{}, fmt.Errorf(
			"failed to list all installed tooth metadata: %w", err)
	}

	for _, metadata := range metadataList {
		if metadata.Tooth() == toothRepo {
			return metadata, nil
		}
	}

	return Metadata{}, fmt.Errorf("cannot find installed tooth metadata: %v",
		toothRepo)
}

// GetToothAvailableVersionList fetches the version list of a tooth repository.
func GetToothAvailableVersionList(ctx contexts.Context, repoPath string) ([]versions.Version,
	error) {
	var err error
	if !CheckIsValidToothRepo(repoPath) {
		return nil, fmt.Errorf("invalid repository path: %v", repoPath)
	}

	urlPath := repoPath + "/@v/list"

	// To lowercases.
	urlPath = strings.ToLower(urlPath)

	content, err := downloading.GetContentFromAllGoproxies(ctx, urlPath)
	if err != nil {
		return nil, fmt.Errorf("failed to fetch version list: %w", err)
	}

	reader := bytes.NewReader(content)

	// Each line is a version.
	var versionList []versions.Version
	scanner := bufio.NewScanner(reader)
	for scanner.Scan() {
		versionString := scanner.Text()
		versionString = strings.TrimPrefix(versionString, "v")
		versionString = strings.TrimSuffix(versionString, "+incompatible")
		version, err := versions.NewFromString(versionString)
		if err != nil {
			continue
		}
		versionList = append(versionList, version)
	}

	// Sort the version list in descending order.
	sort.Slice(versionList, func(i, j int) bool {
		return versions.GreaterThan(versionList[i], versionList[j])
	})

	return versionList, nil
}

// GetToothLatestVersion returns the correct version of the tooth specified by the
// specifier.
func GetToothLatestVersion(ctx contexts.Context,
	toothRepo string) (versions.Version, error) {
	// TODO
	return versions.Version{}, nil
}

// ValidateVersion checks if the version of the tooth repository is valid.
func ValidateVersion(ctx contexts.Context, repoPath string, version versions.Version) error {
	if !CheckIsValidToothRepo(repoPath) {
		return fmt.Errorf("invalid repository path: %v", repoPath)
	}

	// Check if the version is valid.
	urlPathSuffix := "+incompatible.info"
	if strings.HasPrefix(version.String(), "0.") || strings.HasPrefix(
		version.String(), "1.") {
		urlPathSuffix = ".info"
	}
	urlPath := repoPath + "/@v/v" + version.String() + urlPathSuffix

	// To lower case.
	urlPath = strings.ToLower(urlPath)

	_, err := downloading.GetContentFromAllGoproxies(ctx, urlPath)
	if err != nil {
		return fmt.Errorf("failed to access version %v of %v: %w", version.String(),
			repoPath, err)
	}

	return nil
}
