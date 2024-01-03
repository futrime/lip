package downloading

import (
	"fmt"
	"net/url"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/contexts"
)

// CalculateDownloadURLViaGoProxy calculates the download URL of a tooth
// version via GoProxy.
func CalculateDownloadURLViaGoProxy(goProxy string, toothRepo string, version semver.Version) string {
	var suffix string
	if version.Major == 0 || version.Major == 1 {
		suffix = ".zip"
	} else {
		suffix = "+incompatible.zip"
	}

	url := fmt.Sprintf("%v/%v/@v/v%v%v", goProxy, toothRepo, version.String(), suffix)

	return url
}

// GetContentFromAllGoproxies gets the content from all Go proxies.
func GetContentFromAllGoproxies(ctx contexts.Context, urlPath string) ([]byte, error) {
	var errList []error

	for _, goProxy := range ctx.GoProxyList() {
		var err error

		contentUrl, err := url.JoinPath(goProxy, urlPath)
		if err != nil {
			errList = append(errList, fmt.Errorf("cannot join URL path: %w", err))
			continue
		}

		content, err := GetContent(contentUrl)
		if err != nil {
			errList = append(errList, fmt.Errorf("cannot get content from %v: %w",
				contentUrl, err))
			continue
		}

		return content, nil
	}

	return nil, fmt.Errorf("cannot get content from all Go proxies: %v", errList)
}
