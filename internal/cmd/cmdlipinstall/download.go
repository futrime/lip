package cmdlipinstall

import (
	"fmt"
	"net/url"
	"os"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/network"
	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/tooth"
	log "github.com/sirupsen/logrus"
)

// downloadToothArchiveIfNotCached downloads the tooth archive from the Go module proxy
// if it is not cached, and returns the path to the downloaded tooth archive.
func downloadToothArchiveIfNotCached(ctx context.Context, toothRepoPath string,
	toothVersion semver.Version) (tooth.Archive, error) {

	goModuleProxyURL, err := ctx.GoModuleProxyURL()
	if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to get Go module proxy URL: %w", err)
	}

	downloadURL, err := network.GenerateGoModuleZipFileURL(toothRepoPath, toothVersion, goModuleProxyURL)
	if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to generate Go module zip file URL: %w", err)
	}

	cacheDir, err := ctx.CacheDir()
	if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to get cache directory: %w", err)
	}

	cacheFileName := url.QueryEscape(downloadURL.String())
	cachePath := cacheDir.Join(path.MustParse(cacheFileName))

	// Skip downloading if the tooth is already in the cache.
	if _, err := os.Stat(cachePath.LocalString()); os.IsNotExist(err) {
		log.Infof("Downloading %v...", downloadURL)

		var enableProgressBar bool
		if log.GetLevel() == log.PanicLevel || log.GetLevel() == log.FatalLevel ||
			log.GetLevel() == log.ErrorLevel || log.GetLevel() == log.WarnLevel {
			enableProgressBar = false
		} else {
			enableProgressBar = true
		}

		if err := network.DownloadFile(downloadURL, cachePath, enableProgressBar); err != nil {
			return tooth.Archive{}, fmt.Errorf("failed to download file: %w", err)
		}

	} else if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to check if file exists: %w", err)
	}

	archive, err := tooth.MakeArchive(cachePath)
	if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to open archive %v: %w", cachePath, err)
	}

	if err := validateToothArchive(archive, toothRepoPath, toothVersion); err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to validate archive: %w", err)
	}

	return archive, nil
}
