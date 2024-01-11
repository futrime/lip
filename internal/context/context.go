package context

import (
	"encoding/json"
	"fmt"
	"net/url"
	"os"
	"path/filepath"

	"github.com/blang/semver/v4"
)

// Context is the context of the application.
type Context struct {
	config     Config
	lipVersion semver.Version
}

// Make creates a new context.
func Make(config Config, version semver.Version) Context {
	return Context{
		config:     config,
		lipVersion: version,
	}
}

// GitHubMirrorURL returns the GitHub mirror URL.
func (ctx Context) GitHubMirrorURL() (*url.URL, error) {
	gitHubMirrorURL, err := url.Parse(ctx.config.GitHubMirrorURL)
	if err != nil {
		return nil, fmt.Errorf("cannot parse GitHub mirror URL: %w", err)
	}

	return gitHubMirrorURL, nil
}

// GoModuleProxyURL returns the go module proxy URL.
func (ctx Context) GoModuleProxyURL() (*url.URL, error) {
	goModuleProxyURL, err := url.Parse(ctx.config.GoModuleProxyURL)
	if err != nil {
		return nil, fmt.Errorf("cannot parse go module proxy URL: %w", err)
	}

	return goModuleProxyURL, nil
}

// LipVersion returns the lip version.
func (ctx Context) LipVersion() semver.Version {
	return ctx.lipVersion
}

// GlobalDotLipDir returns the global .lip directory.
func (ctx Context) GlobalDotLipDir() (string, error) {
	var err error

	userHomeDir, err := os.UserHomeDir()
	if err != nil {
		return "", fmt.Errorf("cannot get user home directory: %w", err)
	}

	globalDotLipDir := filepath.Join(userHomeDir, ".lip")

	return globalDotLipDir, nil
}

// LocalDotLipDir returns the local .lip directory.
func (ctx Context) LocalDotLipDir() (string, error) {
	var err error

	workspaceDir, err := os.Getwd()
	if err != nil {
		return "", fmt.Errorf("cannot get workspace directory: %w", err)
	}

	path := filepath.Join(workspaceDir, ".lip")

	return path, nil
}

// CacheDir returns the cache directory.
func (ctx Context) CacheDir() (string, error) {
	var err error

	globalDotLipDir, err := ctx.GlobalDotLipDir()
	if err != nil {
		return "", fmt.Errorf("cannot get global .lip directory: %w", err)
	}

	path := filepath.Join(globalDotLipDir, "cache")

	return path, nil
}

// MetadataDir returns the metadata directory.
func (ctx Context) MetadataDir() (string, error) {
	var err error

	localDotLipDir, err := ctx.LocalDotLipDir()
	if err != nil {
		return "", fmt.Errorf("cannot get local .lip directory: %w", err)
	}

	path := filepath.Join(localDotLipDir, "metadata")

	return path, nil
}

// CreateDirStructure creates the directory structure.
func (ctx Context) CreateDirStructure() error {
	var err error

	globalDotLipDir, err := ctx.GlobalDotLipDir()
	if err != nil {
		return fmt.Errorf("cannot get global .lip directory: %w", err)
	}

	err = os.MkdirAll(globalDotLipDir, 0755)
	if err != nil {
		return fmt.Errorf("cannot create global .lip directory: %w", err)
	}

	localDotLipDir, err := ctx.LocalDotLipDir()
	if err != nil {
		return fmt.Errorf("cannot get local .lip directory: %w", err)
	}

	err = os.MkdirAll(localDotLipDir, 0755)
	if err != nil {
		return fmt.Errorf("cannot create local .lip directory: %w", err)
	}

	cacheDir, err := ctx.CacheDir()
	if err != nil {
		return fmt.Errorf("cannot get cache directory: %w", err)
	}

	err = os.MkdirAll(cacheDir, 0755)
	if err != nil {
		return fmt.Errorf("cannot create cache directory: %w", err)
	}

	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return fmt.Errorf("cannot get metadata directory: %w", err)
	}

	err = os.MkdirAll(metadataDir, 0755)
	if err != nil {
		return fmt.Errorf("cannot create metadata directory: %w", err)
	}

	return nil
}

// LoadOrCreateConfigFile loads or creates the config file.
func (ctx Context) LoadOrCreateConfigFile() error {
	var err error

	globalDotLipDir, err := ctx.GlobalDotLipDir()
	if err != nil {
		return fmt.Errorf("cannot get global .lip directory: %w", err)
	}

	err = os.MkdirAll(globalDotLipDir, 0755)
	if err != nil {
		return fmt.Errorf("cannot create global .lip directory: %w", err)
	}

	configFilePath := filepath.Join(globalDotLipDir, "config.json")

	_, err = os.Stat(configFilePath)
	if os.IsNotExist(err) {
		jsonBytes, err := json.MarshalIndent(ctx.config, "", "  ")
		if err != nil {
			return fmt.Errorf("cannot marshal config: %w", err)
		}

		err = os.WriteFile(configFilePath, jsonBytes, 0644)
		if err != nil {
			return fmt.Errorf("cannot write config file: %w", err)
		}

	} else if err != nil {
		return fmt.Errorf("cannot get config file info: %w", err)

	} else {
		jsonBytes, err := os.ReadFile(configFilePath)
		if err != nil {
			return fmt.Errorf("cannot read config file: %w", err)
		}

		err = json.Unmarshal(jsonBytes, &ctx.config)
		if err != nil {
			return fmt.Errorf("cannot unmarshal config: %w", err)
		}
	}

	return nil
}
