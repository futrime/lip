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

	"github.com/lippkg/lip/internal/contexts"
	"github.com/lippkg/lip/internal/downloading"
	"github.com/lippkg/lip/internal/versions"
)

// FetchVersionList fetches the version list of a tooth repository.
func FetchVersionList(ctx contexts.Context, repoPath string) ([]versions.Version, error) {
	var err error
	if !IsValidToothRepo(repoPath) {
		return nil, fmt.Errorf("invalid repository path: %v", repoPath)
	}

	urlPath := repoPath + "/@v/list"

	// To lowercases.
	urlPath = strings.ToLower(urlPath)

	content, err := GetContentFromAllGoproxies(ctx, urlPath)
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

// FindInstalledToothMetadata finds the installed tooth metadata.
func FindInstalledToothMetadata(ctx contexts.Context, toothRepo string) (Metadata, error) {
	var err error

	metadataList, err := ListAllInstalledToothMetadata(ctx)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to list all installed tooth metadata: %w", err)
	}

	for _, metadata := range metadataList {
		if metadata.Tooth() == toothRepo {
			return metadata, nil
		}
	}

	return Metadata{}, fmt.Errorf("cannot find installed tooth metadata: %v", toothRepo)
}

// GetContentFromAllGoproxies gets the content from all Go proxies.
func GetContentFromAllGoproxies(ctx contexts.Context, urlPath string) ([]byte, error) {
	var errList []error

	for _, goProxy := range ctx.GoProxyList() {
		url := filepath.Join(strings.TrimSuffix(goProxy, "/"), urlPath)

		content, err := downloading.GetContent(url)
		if err != nil {
			errList = append(errList, fmt.Errorf("cannot get content from %v: %w", url, err))
			continue
		}

		return content, nil
	}

	return nil, fmt.Errorf("cannot get content from all Go proxies: %v", errList)
}

// IsValidToothRepo returns true if the tooth repository is valid.
func IsValidToothRepo(toothRepo string) bool {
	reg := regexp.MustCompile(`^[a-z0-9][a-z0-9-_\.\/]*$`)

	return reg.FindString(toothRepo) == toothRepo
}

// ListAllInstalledToothMetadata lists all installed tooth metadata.
func ListAllInstalledToothMetadata(ctx contexts.Context) ([]Metadata, error) {
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

	// Sort the metadata list in case-insensitive ascending order of the tooth repository.
	sort.Slice(metadataList, func(i, j int) bool {
		return strings.ToLower(metadataList[i].Tooth()) < strings.ToLower(metadataList[j].Tooth())
	})

	return metadataList, nil
}

// ValidateVersion checks if the version of the tooth repository is valid.
func ValidateVersion(ctx contexts.Context, repoPath string, version versions.Version) error {
	if !IsValidToothRepo(repoPath) {
		return fmt.Errorf("invalid repository path: %v", repoPath)
	}

	// Check if the version is valid.
	urlPathSuffix := "+incompatible.info"
	if strings.HasPrefix(version.String(), "0.") || strings.HasPrefix(version.String(), "1.") {
		urlPathSuffix = ".info"
	}
	urlPath := repoPath + "/@v/v" + version.String() + urlPathSuffix

	// To lower case.
	urlPath = strings.ToLower(urlPath)

	_, err := GetContentFromAllGoproxies(ctx, urlPath)
	if err != nil {
		return fmt.Errorf("failed to access version %v of %v: %w", version.String(), repoPath, err)
	}

	return nil
}
