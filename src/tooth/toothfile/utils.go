package toothfile

import (
	"archive/zip"
	"strings"

	"github.com/liteldev/lip/tooth/toothmetadata"
)

// parseMetadataPlacement parses the wildcard placements of a tooth metadata.
func parseMetadataPlacement(metadata toothmetadata.Metadata, r *zip.ReadCloser, filePrefix string) toothmetadata.Metadata {
	for i, placement := range metadata.Placement {
		// If either source or destination is not a wildcard, skip.
		if !strings.HasSuffix(placement.Source, "*") ||
			!strings.HasSuffix(placement.Destination, "*") {
			continue
		}

		placement.Source = strings.TrimSuffix(placement.Source, "*")
		placement.Destination = strings.TrimSuffix(placement.Destination, "*")

		// Find all files that match the source.
		for _, file := range r.File {
			fileName := strings.TrimPrefix(file.Name, filePrefix)
			if strings.HasPrefix(fileName, placement.Source) &&
				!strings.HasSuffix(fileName, "/") { // Skip directories.
				// Add the file to the metadata.
				metadata.Placement = append(metadata.Placement, toothmetadata.PlacementStruct{
					Source:      fileName,
					Destination: placement.Destination + strings.TrimPrefix(fileName, placement.Source),
				})
			}
		}

		// Remove the wildcard placement.
		metadata.Placement = append(metadata.Placement[:i], metadata.Placement[i+1:]...)
	}

	return metadata
}

// GetFilePrefix returns the prefix of all files in a zip file.
func GetFilePrefix(r *zip.ReadCloser) string {
	prefix := ""
	for i, file := range r.File {
		if strings.HasSuffix(file.Name, "/") { // Skip directories.
			continue
		}

		// If the prefix is empty, set it to the first file.
		if i == 0 {
			prefix = file.Name
			continue
		}

		// Find the common prefix between the prefix and the file.
		for i := 0; i < len(prefix) && i < len(file.Name); i++ {
			if prefix[i] != file.Name[i] {
				prefix = prefix[:i]
				break
			}
		}
	}

	// If tooth.json is the only file, set the prefix to empty.
	if prefix == "tooth.json" {
		prefix = ""
	}

	return prefix
}
