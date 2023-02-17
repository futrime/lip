package toothrepo

import (
	"bufio"
	"bytes"
	"errors"
	"regexp"
	"sort"
	"strings"

	"github.com/liteldev/lip/download"
	"github.com/liteldev/lip/utils/versions"
)

// FetchVersionList fetches the version list of a tooth repository.
func FetchVersionList(repoPath string) ([]versions.Version, error) {
	var err error
	if !isValidPath(repoPath) {
		return nil, errors.New("invalid repository path: " + repoPath)
	}

	urlPath := repoPath + "/@v/list"

	// To lowercases.
	urlPath = strings.ToLower(urlPath)

	content, err := download.GetGoproxyContent(urlPath)
	if err != nil {
		return nil, errors.New("Failed to fetch version list: " + err.Error())
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

// ValidateVersion checks if the version of the tooth repository is valid.
func ValidateVersion(repoPath string, version versions.Version) error {
	if !isValidPath(repoPath) {
		return errors.New("invalid repository path: " + repoPath)
	}

	// Check if the version is valid.
	urlPathSuffix := "+incompatible.info"
	if strings.HasPrefix(version.String(), "0.") || strings.HasPrefix(version.String(), "1.") {
		urlPathSuffix = ".info"
	}
	urlPath := repoPath + "/@v/v" + version.String() + urlPathSuffix

	// To lower case.
	urlPath = strings.ToLower(urlPath)

	_, err := download.GetGoproxyContent(urlPath)
	if err != nil {
		return errors.New("Failed to access version " + version.String() + " of " + repoPath + ": " + err.Error())
	}

	return nil
}

// isValidPath checks if the repoPath is valid.
func isValidPath(repoPath string) bool {
	reg := regexp.MustCompile(`^[a-zA-Z\d-_\.\/]*$`)

	// If not matched or the matched string is not the same as the specifier, it is an
	// invalid requirement specifier.
	return reg.FindString(repoPath) == repoPath
}
