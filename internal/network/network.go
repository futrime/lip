package network

import (
	"fmt"
	"io"
	"net/http"
	"net/url"
	"os"

	"github.com/lippkg/lip/internal/path"
	"github.com/schollz/progressbar/v3"
	"golang.org/x/net/proxy"
)

func DownloadFile(url *url.URL, filePath path.Path, enableProgressBar bool, proxyURL *url.URL) error {
	var httpClient *http.Client
	if proxyURL != nil {
		if proxyURL.Scheme == "http" || proxyURL.Scheme == "https" {
			httpClient = &http.Client{
				Transport: &http.Transport{
					Proxy: http.ProxyURL(proxyURL),
				},
			}
		} else if proxyURL.Scheme == "socks5" {
			dialer, err := proxy.SOCKS5("tcp", proxyURL.Host, nil, proxy.Direct)
			if err != nil {
				return fmt.Errorf("cannot create SOCKS5 dialer: %w", err)
			}
			httpClient = &http.Client{
				Transport: &http.Transport{
					Dial: dialer.Dial,
				},
			}
		} else {
			httpClient = http.DefaultClient
		}
	} else {
		httpClient = http.DefaultClient
	}

	resp, err := httpClient.Get(url.String())
	if err != nil {
		return fmt.Errorf("cannot send HTTP request: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return fmt.Errorf("cannot download file (HTTP %v): %v", resp.Status, url)
	}

	// Create the file
	file, err := os.Create(filePath.LocalString())
	if err != nil {
		return fmt.Errorf("cannot create file: %w", err)
	}
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
		return fmt.Errorf("cannot download file from %v: %w", url, err)
	}
	return nil
}

// GetContent gets the content at once of a URL.
func GetContent(url *url.URL, proxyURL *url.URL) ([]byte, error) {

	var httpClient *http.Client
	if proxyURL != nil {
		if proxyURL.Scheme == "http" || proxyURL.Scheme == "https" {
			httpClient = &http.Client{
				Transport: &http.Transport{
					Proxy: http.ProxyURL(proxyURL),
				},
			}
		} else if proxyURL.Scheme == "socks5" {
			dialer, err := proxy.SOCKS5("tcp", proxyURL.Host, nil, proxy.Direct)
			if err != nil {
				return nil, fmt.Errorf("cannot create SOCKS5 dialer: %w", err)
			}
			httpClient = &http.Client{
				Transport: &http.Transport{
					Dial: dialer.Dial,
				},
			}
		} else {
			httpClient = http.DefaultClient
		}
	} else {
		httpClient = http.DefaultClient
	}

	resp, err := httpClient.Get(url.String())
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
