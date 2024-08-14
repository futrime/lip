package network

import (
	"fmt"
	"io"
	"net/http"
	"net/url"
	"os"

	"github.com/lippkg/lip/internal/path"
	"github.com/schollz/progressbar/v3"
)

// DownloadFile downloads a file from a url and saves it to a local path.
func DownloadFile(url *url.URL, proxyURL *url.URL, filePath path.Path, enableProgressBar bool) error {
	httpClient := getProxiedHTTPClient(proxyURL)

	resp, err := httpClient.Get(url.String())
	if err != nil {
		return fmt.Errorf("cannot send HTTP request\n\t%w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return fmt.Errorf("cannot download file (HTTP %v): %v", resp.Status, url)
	}

	// Create the file
	file, err := os.Create(filePath.LocalString() + ".tmp")
	if err != nil {
		return fmt.Errorf("cannot create file\n\t%w", err)
	}
	defer os.Rename(filePath.LocalString() + ".tmp", filePath.LocalString())
	defer file.Close()

	var writer io.Writer = file

	if enableProgressBar {
		bar := progressbar.NewOptions64(
			resp.ContentLength,
			progressbar.OptionClearOnFinish(),
			progressbar.OptionShowBytes(true),
			progressbar.OptionShowCount(),
		)
		writer = io.MultiWriter(file, bar)
	}

	if _, err := io.Copy(writer, resp.Body); err != nil {
		return fmt.Errorf("cannot download file from %v\n\t%w", url, err)
	}
	return nil
}

// GetContent gets the content at once of a URL.
func GetContent(url *url.URL, proxyURL *url.URL) ([]byte, error) {
	httpClient := getProxiedHTTPClient(proxyURL)

	resp, err := httpClient.Get(url.String())
	if err != nil {
		return nil, fmt.Errorf("cannot send HTTP request\n\t%w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("cannot get content (HTTP %v): %v", resp.Status, url)
	}

	content, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("cannot read HTTP response\n\t%w", err)
	}

	return content, nil
}

func getProxiedHTTPClient(proxyURL *url.URL) *http.Client {
	if proxyURL.String() == "" {
		return http.DefaultClient
	}

	transport := http.DefaultTransport.(*http.Transport).Clone()
	transport.Proxy = http.ProxyURL(proxyURL)
	return &http.Client{Transport: transport}
}
