package zip

import (
	gozip "archive/zip"
	"strings"

	"github.com/lippkg/lip/internal/path"
)

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
