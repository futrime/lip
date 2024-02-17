package network

import (
	"fmt"
	gourl "net/url"
)

// GenerateGitHubMirrorURL generates a GitHub mirror URL from a GitHub URL.
func GenerateGitHubMirrorURL(url *gourl.URL, gitHubMirrorURL *gourl.URL) (*gourl.URL, error) {
	if !IsGitHubDirectDownloadURL(url) {
		return nil, fmt.Errorf("not a GitHub URL: %v", url)
	}

	// Replace the host of the URL with the GitHub mirror URL.
	mirroredURL, err := url.Parse(fmt.Sprintf("%v%v", gitHubMirrorURL, url.Path))
	if err != nil {
		return nil, fmt.Errorf("cannot parse GitHub mirror URL: %w", err)
	}

	return mirroredURL, nil
}

// IsGitHubDirectDownloadURL checks if a URL is a GitHub URL that can be directly downloaded.
func IsGitHubDirectDownloadURL(url *gourl.URL) bool {
	return url.Host == "github.com" && (url.Scheme == "http" || url.Scheme == "https")
}
