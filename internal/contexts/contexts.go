package contexts

import (
	"fmt"
	"net/url"
	"path/filepath"

	"github.com/lippkg/lip/internal/versions"
)

// Context is the context of the application.
type Context struct {
	globalDotLipDir string
	goProxyList     []string
	lipVersion      versions.Version
	workspaceDir    string
}

// New creates a new context.
func New(globalDotLipDir string, goProxyList []string, lipVersion versions.Version, workspaceDir string) Context {
	return Context{
		globalDotLipDir: globalDotLipDir,
		goProxyList:     goProxyList,
		lipVersion:      lipVersion,
		workspaceDir:    workspaceDir,
	}
}

// CacheDir returns the cache directory.
func (ctx Context) CacheDir() (string, error) {
	path := filepath.Join(ctx.globalDotLipDir, "cache")

	return path, nil
}

// CalculateCachePath calculates the cache path of a file downloaded from a URL.
func (ctx Context) CalculateCachePath(fileURL string) (string, error) {
	var err error

	encodedURL := url.QueryEscape(fileURL)

	cacheDir, err := ctx.CacheDir()
	if err != nil {
		return "", fmt.Errorf("cannot get cache directory: %w", err)
	}

	cachePath := filepath.Join(cacheDir, encodedURL)

	return cachePath, nil
}

// GoProxyURL returns the Go Proxy URL.
func (ctx Context) GoProxyURL() []string {
	return ctx.goProxyList
}

// LipVersion returns the Lip version.
func (ctx Context) LipVersion() versions.Version {
	return ctx.lipVersion
}

// WorkspaceDir returns the workspace directory.
func (ctx Context) WorkspaceDir() (string, error) {
	path := ctx.workspaceDir

	return path, nil
}

// WorkspaceDotLipDir returns the workspace .lip directory.
func (ctx Context) WorkspaceDotLipDir() (string, error) {
	path := filepath.Join(ctx.workspaceDir, ".lip")

	return path, nil
}
