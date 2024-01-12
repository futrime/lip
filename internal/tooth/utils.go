package tooth

import (
	"bufio"
	"bytes"
	"fmt"
	"net/url"
	"os"
	"path/filepath"
	"strings"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/network"
	"github.com/lippkg/lip/internal/path"

	"golang.org/x/mod/module"
)

// GetAllMetadata lists all installed tooth metadata.
func GetAllMetadata(ctx context.Context) ([]Metadata, error) {
	metadataList := make([]Metadata, 0)

	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return nil, fmt.Errorf("failed to get metadata directory: %w", err)
	}

	filePathStrings, err := filepath.Glob(filepath.Join(metadataDir.LocalString(), "*.json"))
	if err != nil {
		return nil, fmt.Errorf("failed to list metadata files: %w", err)
	}

	for _, filePathString := range filePathStrings {
		filePath, err := path.Parse(filePathString)
		if err != nil {
			return nil, fmt.Errorf("failed to parse metadata file path: %w", err)
		}

		jsonBytes, err := os.ReadFile(filePath.LocalString())
		if err != nil {
			return nil, fmt.Errorf("failed to read metadata file: %w", err)
		}

		metadata, err := MakeMetadata(jsonBytes)
		if err != nil {
			return nil, fmt.Errorf("failed to parse metadata file: %w", err)
		}

		// Check if the metadata file name matches the tooth repo path in the metadata.
		expectedFileName := fmt.Sprintf("%v.json", url.QueryEscape(metadata.ToothRepoPath()))
		if filePath.Base() != expectedFileName {
			return nil, fmt.Errorf("metadata file name does not match: %v", filePath)
		}

		metadataList = append(metadataList, metadata)
	}

	return metadataList, nil
}

// GetAvailableVersions fetches the version list of a tooth repository.
func GetAvailableVersions(ctx context.Context, toothRepoPath string) (semver.Versions,
	error) {

	if !IsValidToothRepoPath(toothRepoPath) {
		return nil, fmt.Errorf("invalid repository path %v", toothRepoPath)
	}

	goModuleProxyURL, err := ctx.GoModuleProxyURL()
	if err != nil {
		return nil, fmt.Errorf("failed to get go module proxy URL: %w", err)
	}

	versionURL, err := network.GenerateGoModuleVersionListURL(toothRepoPath, goModuleProxyURL)
	if err != nil {
		return nil, fmt.Errorf("failed to generate version list URL: %w", err)
	}

	content, err := network.GetContent(versionURL)
	if err != nil {
		return nil, fmt.Errorf("failed to fetch version list: %w", err)
	}

	reader := bytes.NewReader(content)

	// Each line is a version.
	versionList := make(semver.Versions, 0)
	scanner := bufio.NewScanner(reader)
	for scanner.Scan() {
		versionString := scanner.Text()
		versionString = strings.TrimPrefix(versionString, "v")
		versionString = strings.TrimSuffix(versionString, "+incompatible")
		version, err := semver.Parse(versionString)
		if err != nil {
			continue
		}
		versionList = append(versionList, version)
	}

	return versionList, nil
}

// GetLatestVersion returns the latest =version of a tooth repository.
func GetLatestVersion(ctx context.Context,
	toothRepoPath string) (semver.Version, error) {

	versionList, err := GetAvailableVersions(ctx, toothRepoPath)
	if err != nil {
		return semver.Version{}, fmt.Errorf(
			"failed to get available version list: %w", err)
	}

	stableVersionList := make(semver.Versions, 0)
	for _, version := range versionList {
		if len(version.Pre) == 0 {
			stableVersionList = append(stableVersionList, version)
		}
	}

	semver.Sort(stableVersionList)

	if len(stableVersionList) >= 1 {
		return stableVersionList[len(stableVersionList)-1], nil
	}

	semver.Sort(versionList)

	if len(versionList) >= 1 {
		return versionList[len(versionList)-1], nil
	}

	return semver.Version{}, fmt.Errorf("no available version found")
}

// GetMetadata finds the installed tooth metadata.
func GetMetadata(ctx context.Context, toothRepoPath string) (Metadata,
	error) {

	metadataList, err := GetAllMetadata(ctx)
	if err != nil {
		return Metadata{}, fmt.Errorf(
			"failed to list all installed tooth metadata: %w", err)
	}

	for _, metadata := range metadataList {
		if metadata.ToothRepoPath() == toothRepoPath {
			return metadata, nil
		}
	}

	return Metadata{}, fmt.Errorf("cannot find installed tooth metadata: %v",
		toothRepoPath)
}

// IsInstalled checks if a tooth is installed.
func IsInstalled(ctx context.Context, toothRepoPath string) (bool, error) {

	metadataList, err := GetAllMetadata(ctx)
	if err != nil {
		return false, fmt.Errorf(
			"failed to list all installed tooth metadata: %w", err)
	}

	for _, metadata := range metadataList {
		if metadata.ToothRepoPath() == toothRepoPath {
			return true, nil
		}
	}

	return false, nil
}

// IsValidToothRepoPath checks if the tooth repository path is valid.
func IsValidToothRepoPath(toothRepoPath string) bool {
	if err := module.CheckPath(toothRepoPath); err != nil {
		return false
	}
	return true
}
