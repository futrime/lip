package toothfile

import (
	"archive/zip"
	"strings"

	"github.com/liteldev/lip/tooth/toothmetadata"
)

// parseMetadataPlacement parses the wildcard placements of a tooth metadata.
func parseMetadataPlacement(metadata toothmetadata.Metadata, r *zip.ReadCloser) toothmetadata.Metadata {
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
			if strings.HasPrefix(file.Name, placement.Source) &&
				!strings.HasSuffix(file.Name, "/") { // Skip directories.
				// Add the file to the metadata.
				metadata.Placement = append(metadata.Placement, toothmetadata.PlacementStruct{
					Source:      file.Name,
					Destination: placement.Destination + strings.TrimPrefix(file.Name, placement.Source),
				})
			}
		}

		// Remove the wildcard placement.
		metadata.Placement = append(metadata.Placement[:i], metadata.Placement[i+1:]...)
	}

	return metadata
}
