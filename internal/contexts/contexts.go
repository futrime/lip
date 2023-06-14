package contexts

import (
	"path/filepath"

	"github.com/lippkg/lip/internal/paths"
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
	var err error

	path := filepath.Join(ctx.globalDotLipDir, "cache")
	path, err = paths.Regularize(path)
	if err != nil {
		return "", err
	}

	return path, nil
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
	var err error

	path := ctx.workspaceDir
	path, err = paths.Regularize(path)
	if err != nil {
		return "", err
	}

	return path, nil
}

// WorkspaceDotLipDir returns the workspace .lip directory.
func (ctx Context) WorkspaceDotLipDir() (string, error) {
	var err error

	path := filepath.Join(ctx.workspaceDir, ".lip")
	path, err = paths.Regularize(path)
	if err != nil {
		return "", err
	}

	return path, nil
}
