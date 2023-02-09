// Package download provides a simple way to download files from the web.
package download

import (
	"errors"
	"io"
	"net/http"
	"os"
	"strconv"
)

// downloadFile downloads a file from a url and saves it to a local path.
func DownloadFile(url string, filePath string) error {
	// Get the data
	resp, err := http.Get(url)
	if err != nil {
		return err
	}
	defer resp.Body.Close()

	// Check server response
	if resp.StatusCode != http.StatusOK {
		return errors.New("cannot download file (HTTP CODE " + strconv.Itoa(resp.StatusCode) + "): " + url)
	}

	// Create the file
	out, err := os.Create(filePath)
	if err != nil {
		return err
	}
	defer out.Close()

	// Write the body to file
	_, err = io.Copy(out, resp.Body)
	if err != nil {
		return err
	}

	return nil
}
