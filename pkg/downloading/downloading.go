package downloading

import (
	"fmt"
	"io"
	"net/http"
	"os"
	"path/filepath"
	"strings"

	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/versions"
	"github.com/schollz/progressbar/v3"
)

type ProgressBarStyleType int

const (
	StyleDefault ProgressBarStyleType = iota
	StylePercentageOnly
	StyleNone
)

// CalculateDownloadURLViaGoProxy calculates the download URL of a tooth
// version via GoProxy.
func CalculateDownloadURLViaGoProxy(goProxy string, toothRepo string, version versions.Version) string {
	var suffix string
	if version.Major() == 0 || version.Major() == 1 {
		suffix = ".zip"
	} else {
		suffix = "+incompatible.zip"
	}

	url := fmt.Sprintf("%v/%v/@v/v%v%v", goProxy, toothRepo, version.String(), suffix)

	return url
}

// DownloadFile downloads a file from a url and saves it to a local path.
// Note that if the style is not StyleNone, the progress bar will be shown
// on the terminal.
func DownloadFile(url string, filePath string, progressBarStyle ProgressBarStyleType) error {
	var err error

	req, err := http.NewRequest("GET", url, nil)
	if err != nil {
		return fmt.Errorf("cannot create HTTP request: %w", err)
	}

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		return fmt.Errorf("cannot send HTTP request: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return fmt.Errorf("cannot download file (HTTP %v): %v", resp.Status, url)
	}

	// Create the file
	file, err := os.Create(filePath)
	if err != nil {
		return fmt.Errorf("cannot create file: %w", err)
	}
	defer file.Close()

	switch progressBarStyle {
	case StyleNone:
		_, err = io.Copy(file, resp.Body)
		if err != nil {
			return fmt.Errorf("cannot download file from %v: %w", url, err)
		}
		return nil

	case StylePercentageOnly:
		bar := progressbar.NewOptions64(
			resp.ContentLength,
			progressbar.OptionClearOnFinish(),
			progressbar.OptionSetElapsedTime(false),
			progressbar.OptionSetPredictTime(false),
			progressbar.OptionSetWidth(0),
		)
		_, err = io.Copy(io.MultiWriter(file, bar), resp.Body)
		if err != nil {
			return fmt.Errorf("cannot download file from %v: %w", url, err)
		}

		return nil

	case StyleDefault:
		bar := progressbar.NewOptions64(
			resp.ContentLength,
			progressbar.OptionClearOnFinish(),
			progressbar.OptionShowBytes(true),
			progressbar.OptionShowCount(),
		)
		_, err = io.Copy(io.MultiWriter(file, bar), resp.Body)
		if err != nil {
			return fmt.Errorf("cannot download file from %v: %w", url, err)
		}

		return nil
	}

	// Never reached.
	panic("unreachable")
}

// GetContent gets the content of a URL.
func GetContent(url string) ([]byte, error) {
	var err error

	req, err := http.NewRequest("GET", url, nil)
	if err != nil {
		return nil, fmt.Errorf("cannot create HTTP request: %w", err)
	}

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		return nil, fmt.Errorf("cannot send HTTP request: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("cannot get content (HTTP %v): %v", resp.Status, url)
	}

	content, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("cannot read HTTP response: %w", err)
	}

	return content, nil
}

// GetContentFromAllGoproxies gets the content from all Go proxies.
func GetContentFromAllGoproxies(ctx contexts.Context, urlPath string) ([]byte, error) {
	var errList []error

	for _, goProxy := range ctx.GoProxyList() {
		url := filepath.Join(strings.TrimSuffix(goProxy, "/"), urlPath)

		content, err := GetContent(url)
		if err != nil {
			errList = append(errList, fmt.Errorf("cannot get content from %v: %w",
				url, err))
			continue
		}

		return content, nil
	}

	return nil, fmt.Errorf("cannot get content from all Go proxies: %v", errList)
}
