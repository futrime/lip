// Package download provides a simple way to download files from the web.
package download

import (
	"errors"
	"io"
	"net/http"
	"os"
	"strconv"
	"strings"

	"github.com/liteldev/lip/context"
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

	if resp.StatusCode != http.StatusOK {
		return errors.New("cannot download file (HTTP " + resp.Status + "): " + url)
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

// DownloadGoproxyFile downloads a file from at least one goproxy url and saves it to a local path.
// It will try to download from all goproxy urls until one succeeds.
func DownloadGoproxyFile(urlPath, filePath string, progressBarStyle ProgressBarStyleType) error {
	var errList []error

	for _, goproxy := range context.GoproxyList {
		goproxy = strings.TrimSuffix(goproxy, "/")

		url := goproxy + "/" + urlPath

		err := DownloadFile(url, filePath, progressBarStyle)
		if err != nil {
			errList = append(errList, err)
			continue
		}

		return nil
	}
	if len(errList) > 0 {
		errStr := ""
		for i, err := range errList {
			errStr += context.GoproxyList[i] + ": " + err.Error() + ", "
		}
		return errors.New("failed to download " + urlPath + " from any goproxy: (" + errStr[:len(errStr)-2] + ")")
	}

	return nil
}

// GetGoproxyContent gets the content of a file from at least one goproxy url.
// It will try to download from all goproxy urls until one succeeds.
func GetGoproxyContent(urlPath string) ([]byte, error) {
	var errorList []error

TryAllGoproxy:
	for _, goproxy := range context.GoproxyList {
		goproxy = strings.TrimSuffix(goproxy, "/")

		url := goproxy + "/" + urlPath

		resp, err := http.Get(url)
		if err != nil {
			errorList = append(errorList, err)
			continue
		}

		if resp.StatusCode != http.StatusOK {
			resp.Body.Close()
			errorList = append(errorList,
				errors.New("failed to get content (HTTP CODE "+strconv.Itoa(resp.StatusCode)+")"))
			continue
		}

		content := make([]byte, 0)

		for {
			buf := make([]byte, 1024)
			n, err := resp.Body.Read(buf)
			if err != nil && err != io.EOF {
				errorList = append(errorList, errors.New("failed to read content: "+err.Error()))
				continue TryAllGoproxy
			}
			if n == 0 {
				break
			}
			content = append(content, buf[:n]...)
		}

		resp.Body.Close()
		return content, nil
	}

	if len(errorList) > 0 {
		errStr := ""
		for i, err := range errorList {
			errStr += context.GoproxyList[i] + ": " + err.Error() + ", "
		}
		return make([]byte, 0),
			errors.New("failed to get " + urlPath + " from any goproxy: (" + errStr[:len(errStr)-2] + ")")
	}

	return make([]byte, 0), errors.New("unknown error")
}
