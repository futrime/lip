// Package download provides a simple way to download files from the web.
package download

import (
	"errors"
	"io"
	"net/http"
	"os"
	"strconv"

	"github.com/schollz/progressbar/v3"
)

type ProgressBarStyleType int

const (
	StyleDefault ProgressBarStyleType = iota
	StylePercentageOnly
	StyleNone
)

// DownloadFile downloads a file from a url and saves it to a local path.
func DownloadFile(url string, filePath string, progressBarStyle ProgressBarStyleType) error {
	req, _ := http.NewRequest("GET", url, nil)
	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	// Check server response
	if resp.StatusCode != http.StatusOK {
		return errors.New("cannot download file (HTTP CODE " + strconv.Itoa(resp.StatusCode) + "): " + url)
	}

	// Create the file
	file, err := os.Create(filePath)
	if err != nil {
		return err
	}
	defer file.Close()

	switch progressBarStyle {
	case StyleNone:
		_, err = io.Copy(file, resp.Body)
		if err != nil {
			return errors.New("cannot download file from " + url + ": " + err.Error())
		}
		return nil
	case StylePercentageOnly:
		// Only show percentage
		bar := progressbar.NewOptions64(
			resp.ContentLength,
			progressbar.OptionClearOnFinish(),
			progressbar.OptionSetElapsedTime(false),
			progressbar.OptionSetPredictTime(false),
			progressbar.OptionSetWidth(0),
		)
		io.Copy(io.MultiWriter(file, bar), resp.Body)
		return nil
	default:
		bar := progressbar.NewOptions64(
			resp.ContentLength,
			progressbar.OptionClearOnFinish(),
			progressbar.OptionShowBytes(true),
			progressbar.OptionShowCount(),
		)
		io.Copy(io.MultiWriter(file, bar), resp.Body)
		return nil
	}
}
