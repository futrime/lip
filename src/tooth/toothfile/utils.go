package toothfile

import (
	"archive/zip"
	"strings"

	"github.com/lippkg/lip/tooth/toothmetadata"
)

// parseMetadataPlacement parses the wildcard placements of a tooth metadata.
func parseMetadataPlacement(metadata toothmetadata.Metadata, r *zip.ReadCloser, filePrefix string) toothmetadata.Metadata {
	placementList := make([]toothmetadata.PlacementStruct, 0)

	for _, placement := range metadata.Placement {
		// If either source or destination is not a wildcard, skip.
		if !strings.HasSuffix(placement.Source, "*") ||
			!strings.HasSuffix(placement.Destination, "*") {
			placementList = append(placementList, placement)
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
				placementList = append(placementList, toothmetadata.PlacementStruct{
					Source:      fileName,
					Destination: placement.Destination + strings.TrimPrefix(fileName, placement.Source),
					GOOS:        placement.GOOS,
					GOARCH:      placement.GOARCH,
				})
			}
		}
	}

	metadata.Placement = placementList

	return metadata
}

// GetFilePrefix returns the prefix of all files in a zip file.
func GetFilePrefix(r *zip.ReadCloser) string {
	if len(r.File) == 0 {
		return ""
	}

	prefix := ""

	// Get the longest common prefix of all files ending with a slash.
	for i, f := range r.File {
		// If this is the first file, set the prefix to the file name.
		if i == 0 {
			prefix = getLongestPrefixEndingWithSlash(f.Name)
			continue
		}

		// Get the longest common prefix of the current prefix and the file name.
		prefix = getLongestPrefixEndingWithSlash(prefix)
		filePrefix := getLongestPrefixEndingWithSlash(f.Name)

		if strings.HasPrefix(prefix, filePrefix) {
			prefix = filePrefix
			continue
		}

		for j := 0; j < len(prefix) && j < len(filePrefix); j++ {
			if prefix[j] != filePrefix[j] {
				prefix = prefix[:j]
				break
			}
		}
	}

	return prefix
}

// getLongestPrefixEndingWithSlash returns the longest prefix of a string ending with a slash.
func getLongestPrefixEndingWithSlash(s string) string {
	lastSlash := strings.LastIndex(s, "/")
	if lastSlash == -1 {
		return ""
	}

	return s[:lastSlash+1]
}
