package toothrepo

import (
	"bufio"
	"errors"
	"net/http"
	"regexp"
	"sort"
	"strconv"
	"strings"

	"github.com/liteldev/lip/context"
	"github.com/liteldev/lip/utils/versions"
)

// FetchVersionList fetches the version list of a tooth repository.
func FetchVersionList(repoPath string) ([]versions.Version, error) {
	if !isValidPath(repoPath) {
		return nil, errors.New("invalid repository path: " + repoPath)
	}

	url := context.Goproxy + "/" + repoPath + "/@v/list"

	// To lowercases.
	url = strings.ToLower(url)

	// Get the version list.
	resp, err := http.Get(url)
	if err != nil {
		return nil, errors.New("cannot access GOPROXY: " + context.Goproxy)
	}
	defer resp.Body.Close()

	if resp.StatusCode != 200 {
		return nil, errors.New("cannot access tooth repository (HTTP CODE " + strconv.Itoa(resp.StatusCode) + "): " + repoPath)
	}

	// Each line is a version.
	var versionList []versions.Version
	scanner := bufio.NewScanner(resp.Body)
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
	urlSuffix := "+incompatible.info"
	if strings.HasPrefix(version.String(), "0.") || strings.HasPrefix(version.String(), "1.") {
		urlSuffix = ".info"
	}
	url := context.Goproxy + "/" + repoPath + "/@v/v" + version.String() + urlSuffix

	// To lower case.
	url = strings.ToLower(url)

	// Get the version information.
	resp, err := http.Get(url)
	if err != nil {
		return errors.New("cannot access GOPROXY: " + context.Goproxy)
	}
	defer resp.Body.Close()

	// If the status code is 200, the version is valid.
	if resp.StatusCode != 200 {
		return errors.New("cannot access tooth (HTTP CODE " + strconv.Itoa(resp.StatusCode) + "): " + repoPath + "@" + version.String())
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
