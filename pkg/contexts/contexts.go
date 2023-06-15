package contexts

import (
	"fmt"
	"net/url"
	"os"
	"path/filepath"

	"github.com/lippkg/lip/pkg/versions"
)

// Context is the context of the application.
type Context struct {
	globalDotLipDir string
	goProxyList     []string
	lipVersion      versions.Version
	pluginSet       map[string]struct{}
	workspaceDir    string
}

// New creates a new context.
func New(globalDotLipDir string, goProxyList []string, lipVersion versions.Version,
	pluginSet map[string]struct{}, workspaceDir string) Context {
	return Context{
		globalDotLipDir: globalDotLipDir,
		goProxyList:     goProxyList,
		lipVersion:      lipVersion,
		pluginSet:       pluginSet,
		workspaceDir:    workspaceDir,
	}
}

// CacheDir returns the cache directory.
func (ctx Context) CacheDir() (string, error) {
	var err error

	globalDotLipDir, err := ctx.GlobalDotLipDir()
	if err != nil {
		return "", fmt.Errorf("cannot get global .lip directory: %w", err)
	}

	path := filepath.Join(globalDotLipDir, "cache")

	err = createDirIfNotExist(path)
	if err != nil {
		return "", fmt.Errorf("cannot create cache directory: %w", err)
	}

	return path, nil
}

// CalculateCachePath calculates the cache path of a file downloaded from a URL.
func (ctx Context) CalculateCachePath(fileURL string) (string, error) {
	var err error

	fileName := url.QueryEscape(fileURL)

	cacheDir, err := ctx.CacheDir()
	if err != nil {
		return "", fmt.Errorf("cannot get cache directory: %w", err)
	}

	cachePath := filepath.Join(cacheDir, fileName)

	return cachePath, nil
}

// CalculateMetadataPath calculates the recorded metadata file path of a tooth.
func (ctx Context) CalculateMetadataPath(toothRepo string) (string, error) {
	var err error

	fileName := url.QueryEscape(toothRepo) + ".json"

	metadataDir, err := ctx.MetadataDir()
	if err != nil {
		return "", fmt.Errorf("cannot get metadata directory: %w", err)
	}

	recordPath := filepath.Join(metadataDir, fileName)

	return recordPath, nil
}

// GlobalDotLipDir returns the global .lip directory.
func (ctx Context) GlobalDotLipDir() (string, error) {
	err := createDirIfNotExist(ctx.globalDotLipDir)
	if err != nil {
		return "", fmt.Errorf("cannot create global .lip directory: %w", err)
	}

	return ctx.globalDotLipDir, nil
}

// GoProxyList returns the Go Proxy URL.
func (ctx Context) GoProxyList() []string {
	return ctx.goProxyList
}

// LipVersion returns the Lip version.
func (ctx Context) LipVersion() versions.Version {
	return ctx.lipVersion
}

// MetadataDir returns the metadata directory.
func (ctx Context) MetadataDir() (string, error) {
	var err error

	globalDotLipDir, err := ctx.GlobalDotLipDir()
	if err != nil {
		return "", fmt.Errorf("cannot get global .lip directory: %w", err)
	}

	path := filepath.Join(globalDotLipDir, "metadata")

	err = createDirIfNotExist(path)
	if err != nil {
		return "", fmt.Errorf("cannot create metadata directory: %w", err)
	}

	return path, nil
}

// PluginDir returns the plugin directory.
func (ctx Context) PluginDir() (string, error) {
	var err error

	workspaceDotLipDir, err := ctx.WorkspaceDotLipDir()
	if err != nil {
		return "", fmt.Errorf("cannot get workspace .lip directory: %w", err)
	}

	path := filepath.Join(workspaceDotLipDir, "plugins")

	err = createDirIfNotExist(path)
	if err != nil {
		return "", fmt.Errorf("cannot create plugin directory: %w", err)
	}

	return path, nil
}

// PluginSet returns the plugin set.
func (ctx Context) PluginSet() map[string]struct{} {
	return ctx.pluginSet
}

// WorkspaceDir returns the workspace directory.
func (ctx Context) WorkspaceDir() (string, error) {
	err := createDirIfNotExist(ctx.workspaceDir)
	if err != nil {
		return "", fmt.Errorf("cannot create workspace directory: %w", err)
	}

	return ctx.workspaceDir, nil
}

// WorkspaceDotLipDir returns the workspace .lip directory.
func (ctx Context) WorkspaceDotLipDir() (string, error) {
	workspaceDir, err := ctx.WorkspaceDir()
	if err != nil {
		return "", fmt.Errorf("cannot get workspace directory: %w", err)
	}

	path := filepath.Join(workspaceDir, ".lip")

	err = createDirIfNotExist(path)
	if err != nil {
		return "", fmt.Errorf("cannot create workspace .lip directory: %w", err)
	}

	return path, nil
}

// ---------------------------------------------------------------------

// createDirIfNotExist creates a directory if it does not exist.
func createDirIfNotExist(dir string) error {
	_, err := os.Stat(dir)
	if os.IsNotExist(err) {
		err = os.MkdirAll(dir, 0755)
		if err != nil {
			return fmt.Errorf("cannot create directory: %w", err)
		}

	} else if err != nil {
		return fmt.Errorf("cannot get directory info: %w", err)
	}

	return nil
}
