package tooth

import (
	"bufio"
	"bytes"
	"fmt"
	"os"
	"path/filepath"
	"sort"
	"strings"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/network"

	"golang.org/x/mod/module"
)

// CheckIsToothInstalled checks if a tooth is installed.
func CheckIsToothInstalled(ctx context.Context, toothRepo string) (bool, error) {
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

// CheckIsValidToothRepo returns true if the tooth repository is valid.
func CheckIsValidToothRepo(toothRepo string) bool {
	return module.CheckPath(toothRepo) == nil
}

// GetAllInstalledToothMetadata lists all installed tooth metadata.
func GetAllInstalledToothMetadata(ctx context.Context) ([]Metadata, error) {
	var err error

	var metadataList []Metadata

	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return nil, fmt.Errorf("failed to get metadata directory: %w", err)
	}

	filePaths, err := filepath.Glob(filepath.Join(metadataDir.LocalString(), "*.json"))
	if err != nil {
		return nil, fmt.Errorf("failed to list metadata files: %w", err)
	}

	for _, filePath := range filePaths {
		jsonBytes, err := os.ReadFile(filePath)
		if err != nil {
			return nil, fmt.Errorf("failed to read metadata file: %w", err)
		}

		metadata, err := MakeMetadata(jsonBytes)
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
func GetInstalledToothMetadata(ctx context.Context, toothRepo string) (Metadata,
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

// GetToothAvailableVersions fetches the version list of a tooth repository.
// The version list is sorted in descending order.
func GetToothAvailableVersions(ctx context.Context, repoPath string) (semver.Versions,
	error) {
	var err error
	if !CheckIsValidToothRepo(repoPath) {
		return nil, fmt.Errorf("invalid repository path: %v", repoPath)
	}

	goModuleProxyURL, err := ctx.GoModuleProxyURL()
	if err != nil {
		return nil, fmt.Errorf("failed to get go module proxy URL: %w", err)
	}

	versionURL, err := network.GenerateGoModuleVersionListURL(repoPath, goModuleProxyURL)
	if err != nil {
		return nil, fmt.Errorf("failed to generate version list URL: %w", err)
	}

	content, err := network.GetContent(versionURL)
	if err != nil {
		return nil, fmt.Errorf("failed to fetch version list: %w", err)
	}

	reader := bytes.NewReader(content)

	// Each line is a version.
	var versionList semver.Versions
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

	semver.Sort(versionList)

	// Reverse the version list.
	for i, j := 0, len(versionList)-1; i < j; i, j = i+1, j-1 {
		versionList[i], versionList[j] = versionList[j], versionList[i]
	}

	return versionList, nil
}

// GetToothLatestStableVersion returns the correct version of the tooth
// specified by the specifier.
func GetToothLatestStableVersion(ctx context.Context,
	toothRepo string) (semver.Version, error) {

	var err error

	versionList, err := GetToothAvailableVersions(ctx, toothRepo)
	if err != nil {
		return semver.Version{}, fmt.Errorf(
			"failed to get available version list: %w", err)
	}

	for _, version := range versionList {
		if len(version.Pre) == 0 {
			return version, nil
		}
	}

	return semver.Version{}, fmt.Errorf("cannot find latest stable version")
}
