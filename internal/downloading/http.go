package downloading

import (
	"fmt"
	"io"
	"net/http"
	"os"

	"github.com/schollz/progressbar/v3"
)

// DownloadFile downloads a file from a url and saves it to a local path.
func DownloadFile(url string, filePath string, enableProgressBar bool) error {
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

	if enableProgressBar {
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
	} else {
		_, err = io.Copy(file, resp.Body)
		if err != nil {
			return fmt.Errorf("cannot download file from %v: %w", url, err)
		}
		return nil
	}
}

// GetContent gets the content at once of a URL.
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
