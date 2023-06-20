package teeth

import (
	"archive/zip"
	"fmt"
	"io"
	"path"
	"strings"

	"github.com/lippkg/lip/pkg/paths"
)

// Archive is an archive containing a tooth.
type Archive struct {
	path     string
	metadata Metadata
}

// NewArchive creates a new archive.
func NewArchive(path string) (Archive, error) {
	var err error

	r, err := zip.OpenReader(path)
	if err != nil {
		return Archive{}, fmt.Errorf("failed to open archive: %w", err)
	}
	defer r.Close()

	filePathCommonPrefix := paths.ExtractCommonAncestor(extractAllFilePaths(r))

	// Find tooth.json.
	var toothJSONFile *zip.File = nil
	for _, file := range r.File {
		if file.Name == filePathCommonPrefix+"tooth.json" {
			toothJSONFile = file
			break
		}
	}

	if toothJSONFile == nil {
		return Archive{}, fmt.Errorf("archive does not contain tooth.json")
	}

	// Read tooth.json.
	toothJSONFileReader, err := toothJSONFile.Open()
	if err != nil {
		return Archive{}, fmt.Errorf("failed to open tooth.json: %w", err)
	}
	defer toothJSONFileReader.Close()

	toothJSONBytes, err := io.ReadAll(toothJSONFileReader)
	if err != nil {
		return Archive{}, fmt.Errorf("failed to read tooth.json: %w", err)
	}

	// Parse tooth.json.
	metadata, err := NewMetadata(toothJSONBytes)
	if err != nil {
		return Archive{}, fmt.Errorf("failed to parse tooth.json: %w", err)
	}

	// Extract all file paths and remove the common prefix.
	filePaths := extractAllFilePaths(r)
	for i, filePath := range filePaths {
		filePaths[i] = filePath[len(filePathCommonPrefix):]
	}

	metadata, err = resolveMetadataFilesPlaceRegex(metadata, filePaths)
	if err != nil {
		return Archive{}, fmt.Errorf(
			"failed to resolve metadata files place regular expressions: %w", err)
	}

	return Archive{
		path:     path,
		metadata: metadata,
	}, nil
}

// Path returns the path of the archive.
func (archive Archive) Path() string {
	return archive.path
}

// Metadata returns the metadata of the archive.
func (archive Archive) Metadata() Metadata {
	return archive.metadata
}

// ---------------------------------------------------------------------

// extractAllFilePaths extracts all file paths from a zip archive.
func extractAllFilePaths(r *zip.ReadCloser) []string {
	filePathList := make([]string, len(r.File))

	for i, file := range r.File {
		filePathList[i] = file.Name
	}

	return filePathList
}

// resolveMetadataFilesPlaceRegex parses the regexes of field place of field files in the metadata.
// filePaths should have the common prefix removed.
func resolveMetadataFilesPlaceRegex(metadata Metadata, filePaths []string) (Metadata, error) {
	var err error

	newPlace := make([]RawMetadataFilesPlaceItem, 0)

	rawMetadata := metadata.Raw()

	for _, place := range rawMetadata.Files.Place {
		if !strings.HasSuffix(place.Src, "*") {
			newPlace = append(newPlace, place)
			continue
		}

		sourcePrefix := strings.TrimSuffix(place.Src, "*")

		for _, filePath := range filePaths {
			// Skip directories.
			if strings.HasSuffix(filePath, "/") {
				continue
			}

			if !strings.HasPrefix(filePath, sourcePrefix) {
				continue
			}

			relFilePath := strings.TrimPrefix(filePath, sourcePrefix)

			newPlace = append(newPlace, RawMetadataFilesPlaceItem{
				Src:  filePath,
				Dest: path.Join(place.Dest, relFilePath),
			})
		}
	}

	rawMetadata.Files.Place = newPlace

	metadata, err = NewMetadataFromRawMetadata(rawMetadata)
	if err != nil {
		return Metadata{}, fmt.Errorf("failed to create new metadata: %w", err)
	}

	return metadata, nil
}
