package registry

import (
	"encoding/json"
	"errors"
	"io"
	"net/http"
	"strconv"
	"strings"

	"github.com/liteldev/lip/context"
)

// LookupAlias looks up the alias in the registry.
// The alias is case-insensitive.
// The returned repo path is in lowercase.
func LookupAlias(alias string) (string, error) {
	// Get index.
	indexURL := strings.TrimSuffix(context.RegistryURL, "/") + "/index.json"
	resp, err := http.Get(indexURL)
	if err != nil {
		return "", errors.New("cannot access registry: " + indexURL)
	}
	defer resp.Body.Close()

	// Check server response.
	if resp.StatusCode != http.StatusOK {
		return "", errors.New("registry responded with status: " + resp.Status)
	}

	// Parse index.
	indexBytes, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", errors.New("cannot read registry response: " + err.Error())
	}

	index := make(map[string]interface{})
	err = json.Unmarshal(indexBytes, &index)
	if err != nil {
		return "", errors.New("cannot parse registry response: " + err.Error())
	}

	// format_version field should be 1.
	if int(index["format_version"].(float64)) != 1 {
		return "", errors.New("invalid registry format version: " + strconv.Itoa(int(index["format_version"].(float64))))
	}

	// Check if the alias exists.
	alias = strings.ToLower(alias)
	toothRepo, ok := index["index"].(map[string]interface{})[alias]
	if !ok {
		return "", errors.New("alias not found: " + alias)
	}

	repoPath := toothRepo.(map[string]interface{})["tooth"].(string)
	repoPath = strings.ToLower(repoPath)

	return repoPath, nil
}
