package tooth

import (
	gozip "archive/zip"
	"fmt"
	"io"
	"strings"

	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/zip"
)

// Archive is an archive containing a tooth.
type Archive struct {
	metadata        Metadata
	filePath        path.Path
	contentPathRoot path.Path
}

// MakeArchive creates a new archive.
func MakeArchive(archiveFilePath path.Path) (Archive, error) {
	r, err := gozip.OpenReader(archiveFilePath.LocalString())
	if err != nil {
		return Archive{}, fmt.Errorf("failed to open archive: %w", err)
	}
	defer r.Close()

	filePaths, err := zip.GetFilePaths(r)
	if err != nil {
		return Archive{}, fmt.Errorf("failed to extract file paths: %w", err)
	}

	filePathRoot := path.ExtractLongestCommonPath(filePaths...)

	// If only one file, it must be tooth.json. Then we should use the directory of the file as the root.
	if len(filePaths) == 1 {
		filePathRootDir, err := filePathRoot.Dir()
		if err != nil {
			return Archive{}, fmt.Errorf("failed to get directory of tooth.json: %w", err)
		}

		filePathRoot = filePathRootDir
	}

	// Find tooth.json.
	toothJSONFilePath := filePathRoot.Join(path.MustParse("tooth.json"))
	var toothJSONFile *gozip.File = nil
	for _, file := range r.File {
		if file.Name == toothJSONFilePath.String() {
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
	metadata, err := MakeMetadata(toothJSONBytes)
	if err != nil {
		return Archive{}, fmt.Errorf("failed to parse tooth.json: %w", err)
	}

	// Extract all file paths and remove the common prefix.
	filePathsTrimmed := make([]path.Path, 0)
	for _, filePath := range filePaths {
		filePathsTrimmed = append(filePathsTrimmed, filePath.TrimPrefix(filePathRoot))
	}

	metadataWithoutWildcards, err := populateMetadataFilePlaceWildcards(metadata, filePathsTrimmed)
	if err != nil {
		return Archive{}, fmt.Errorf(
			"failed to resolve metadata files place regular expressions: %w", err)
	}

	return Archive{
		metadata:        metadataWithoutWildcards,
		filePath:        archiveFilePath,
		contentPathRoot: filePathRoot,
	}, nil
}

// FilePath returns the path of the asset archive.
func (ar Archive) FilePath() path.Path {
	return ar.filePath
}

// ContentFilePathRoot returns the directory of tooth.json in the archive.
func (ar Archive) ContentFilePathRoot() path.Path {
	return ar.contentPathRoot
}

// Metadata returns the metadata of the archive.
func (ar Archive) Metadata() Metadata {
	return ar.metadata
}

// populateMetadataFilePlaceWildcards populates wildcards in files.place field of metadata.
// filePaths should be relative to the directory of tooth.json.
func populateMetadataFilePlaceWildcards(metadata Metadata, filePaths []path.Path) (Metadata, error) {
	newPlace := make([]RawMetadataFilesPlaceItem, 0)

	rawMetadata := metadata.Raw()

	for _, placeItem := range rawMetadata.Files.Place {
		// If not wildcard, just append.
		if !strings.HasSuffix(placeItem.Src, "*") {
			newPlace = append(newPlace, placeItem)
			continue
		}

		sourcePathPrefix, err := path.Parse(strings.TrimSuffix(placeItem.Src, "*"))
		if err != nil {
			return Metadata{}, fmt.Errorf("failed to parse source path prefix: %w", err)
		}

		destPathPrefix, err := path.Parse(placeItem.Dest)
		if err != nil {
			return Metadata{}, fmt.Errorf("failed to parse destination path prefix: %w", err)
		}

		for _, filePath := range filePaths {
			if !filePath.HasPrefix(sourcePathPrefix) {
				continue
			}

			relFilePath := filePath.TrimPrefix(sourcePathPrefix)

			newPlace = append(newPlace, RawMetadataFilesPlaceItem{
				Src:  filePath.String(),
				Dest: destPathPrefix.Join(relFilePath).String(),
			})
		}
	}

	rawMetadata.Files.Place = newPlace

	metadata = Metadata{rawMetadata}

	return metadata, nil
}
