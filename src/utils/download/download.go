// Package download provides a simple way to download files from the web.
package download

import (
	"io"
	"net/http"
	"os"

	"github.com/liteldev/lip/utils/logger"
	"github.com/schollz/progressbar/v3"
)

// DownloadFile downloads a file from a url and saves it to a local path.
func DownloadFile(url string, filePath string) error {
	req, _ := http.NewRequest("GET", url, nil)
	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	file, err := os.Create(filePath)
	if err != nil {
		return err
	}
	defer file.Close()

	bar := progressbar.NewOptions(
		int(resp.ContentLength),
		progressbar.OptionShowBytes(true),
		progressbar.OptionSetWidth(30),
		progressbar.OptionSetDescription("  "),
		progressbar.OptionClearOnFinish(),
		progressbar.OptionSetTheme(progressbar.Theme{
			Saucer:        "=",
			SaucerHead:    ">",
			SaucerPadding: " ",
			BarStart:      "[",
			BarEnd:        "]",
		}))

	io.Copy(io.MultiWriter(file, bar), resp.Body)

	logger.Info("    Finished.")

	return nil
}
