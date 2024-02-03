package network

import (
	"fmt"
	"net/url"
)

// GenerateGitHubMirrorURL generates a GitHub mirror URL from a GitHub URL.
func GenerateGitHubMirrorURL(url *url.URL, gitHubMirrorURL *url.URL) (*url.URL, error) {
	if !IsGitHubDirectDownloadURL(url) {
		return nil, fmt.Errorf("not a GitHub URL: %v", url)
	}

	// Replace the host of the URL with the GitHub mirror URL.
	mirroredURL, err := gitHubMirrorURL.Parse(url.Path)
	if err != nil {
		return nil, fmt.Errorf("cannot parse GitHub mirror URL: %w", err)
	}

	return mirroredURL, nil
}

// IsGitHubDirectDownloadURL checks if a URL is a GitHub URL that can be directly downloaded.
func IsGitHubDirectDownloadURL(url *url.URL) bool {
	return url.Host == "github.com" && (url.Scheme == "http" || url.Scheme == "https")
}
