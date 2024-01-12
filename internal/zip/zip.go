package zip

import (
	gozip "archive/zip"
	"strings"

	"github.com/lippkg/lip/internal/path"
)

// GetFilePaths returns a list of file paths in a zip archive. Directories are skipped.
func GetFilePaths(r *gozip.ReadCloser) ([]path.Path, error) {
	filePaths := make([]path.Path, 0)

	for _, file := range r.File {
		// Skip directories.
		if strings.HasSuffix(file.Name, "/") {
			continue
		}

		filePath, err := path.Parse(file.Name)
		if err != nil {
			return nil, err
		}

		filePaths = append(filePaths, filePath)
	}

	return filePaths, nil
}
