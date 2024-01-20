package tooth

import (
	gozip "archive/zip"
	"fmt"
	"io"
	"runtime"

	"github.com/lippkg/lip/internal/path"
	"github.com/lippkg/lip/internal/zip"
)

// Archive is an archive containing a tooth.
type Archive struct {
	metadata      Metadata
	filePath      path.Path
	assetFilePath path.Path
}

// MakeArchive creates a new archive. It will automatically convert metadata to platform-specific.
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

	// Convert to platform-specific metadata.
	metadata, err = metadata.ToPlatformSpecific(runtime.GOOS, runtime.GOARCH)
	if err != nil {
		return Archive{}, fmt.Errorf("failed to convert to platform-specific metadata: %w", err)
	}

	return Archive{
		metadata:      metadata,
		filePath:      archiveFilePath,
		assetFilePath: path.MakeEmpty(),
	}, nil
}

func (ar Archive) AssetFilePath() (path.Path, error) {
	if ar.assetFilePath.IsEmpty() {
		return path.MakeEmpty(), fmt.Errorf("asset file path is empty")
	}

	return ar.assetFilePath, nil
}

// FilePath returns the path of the asset archive.
func (ar Archive) FilePath() path.Path {
	return ar.filePath
}

// Metadata returns the metadata of the archive.
func (ar Archive) Metadata() Metadata {
	return ar.metadata
}

// ToAssetArchiveAttached converts the archive to an archive with asset archive attached.
// If assetArchivePath is empty, the tooth archive will be used as the asset archive.
func (ar Archive) ToAssetArchiveAttached(assetArchiveFilePath path.Path) (Archive, error) {
	// Validate consistency of asset archive file path and asset URL.
	assetURL, err := ar.Metadata().AssetURL()
	if err != nil {
		return Archive{}, fmt.Errorf("failed to get asset URL: %w", err)
	}

	if (assetArchiveFilePath.IsEmpty() && (assetURL.String() != "")) ||
		(!assetArchiveFilePath.IsEmpty() && (assetURL.String() == "")) {
		return Archive{}, fmt.Errorf("asset archive file path and asset URL must be both specified or both empty")
	}

	if assetArchiveFilePath.IsEmpty() {
		// Extract common prefix and prepend it to all file paths in file.place.

		r, err := gozip.OpenReader(ar.filePath.LocalString())
		if err != nil {
			return Archive{}, fmt.Errorf("failed to open archive %v: %w", assetArchiveFilePath.LocalString(), err)
		}
		defer r.Close()

		filePaths, err := zip.GetFilePaths(r)
		if err != nil {
			return Archive{}, fmt.Errorf("failed to extract file paths from %v: %w", assetArchiveFilePath.LocalString(), err)
		}

		filePathRoot := path.ExtractLongestCommonPath(filePaths...)

		newMetadata := ar.metadata
		newMetadataPrefixPrepended := newMetadata.ToFilePathPrefixPrepended(filePathRoot)
		newMetadataWildcardPopulated, err := newMetadataPrefixPrepended.ToWildcardPopulated(filePaths)
		if err != nil {
			return Archive{}, fmt.Errorf("failed to populate wildcards: %w", err)
		}

		return Archive{
			metadata:      newMetadataWildcardPopulated,
			filePath:      ar.filePath,
			assetFilePath: ar.filePath,
		}, nil

	} else {
		r, err := gozip.OpenReader(assetArchiveFilePath.LocalString())
		if err != nil {
			return Archive{}, fmt.Errorf("failed to open archive %v: %w", assetArchiveFilePath.LocalString(), err)
		}
		defer r.Close()

		filePaths, err := zip.GetFilePaths(r)
		if err != nil {
			return Archive{}, fmt.Errorf("failed to extract file paths from %v: %w", assetArchiveFilePath.LocalString(), err)
		}

		newMetadata := ar.metadata
		newMetadataWildcardPopulated, err := newMetadata.ToWildcardPopulated(filePaths)
		if err != nil {
			return Archive{}, fmt.Errorf("failed to populate wildcards: %w", err)
		}

		return Archive{
			metadata:      newMetadataWildcardPopulated,
			filePath:      ar.filePath,
			assetFilePath: assetArchiveFilePath,
		}, nil
	}
}
