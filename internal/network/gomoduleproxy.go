package network

import (
	"fmt"
	"net/url"
	"path"

	"github.com/blang/semver/v4"
	"golang.org/x/mod/module"
)

// GenerateGoModuleVersionListURL generates the URL of the version list of a Go
// module.
func GenerateGoModuleVersionListURL(goModulePath string, goProxyURL *url.URL) (*url.URL, error) {
	if err := module.CheckPath(goModulePath); err != nil {
		return nil, fmt.Errorf("not a Go module path: %v", goModulePath)
	}

	resultURL, err := goProxyURL.Parse(path.Join(goModulePath, "@v", "list"))
	if err != nil {
		return nil, fmt.Errorf("cannot parse Go proxy URL: %w", err)
	}

	return resultURL, nil
}

// GenerateGoModuleZipFileURL generates the URL of a Go module zip file.
func GenerateGoModuleZipFileURL(goModulePath string, version semver.Version, goProxyURL *url.URL) (*url.URL, error) {
	if err := module.CheckPath(goModulePath); err != nil {
		return nil, fmt.Errorf("not a Go module path: %v", goModulePath)
	}

	zipFileName, err := generateGoModuleZipFileName(version)
	if err != nil {
		return nil, fmt.Errorf("cannot generate Go module zip file name: %w", err)
	}

	resultURL, err := goProxyURL.Parse(path.Join(goModulePath, "@v", zipFileName))
	if err != nil {
		return nil, fmt.Errorf("cannot parse Go proxy URL: %w", err)
	}

	return resultURL, nil
}

func generateGoModuleZipFileName(version semver.Version) (string, error) {
	// To ensure that the version is a canonical version. Reference:
	// https://go.dev/ref/mod#glos-canonical-version
	if len(version.Build) > 0 {
		return "", fmt.Errorf("cannot generate zip file name for a version with build metadata: %v", version)
	}

	// Reference: https://go.dev/ref/mod#non-module-compat
	if version.Major >= 2 {
		return fmt.Sprintf("v%v+incompatible.zip", version.String()), nil
	} else {
		return fmt.Sprintf("v%v.zip", version.String()), nil
	}
}
