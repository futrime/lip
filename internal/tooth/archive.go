package tooth

import (
	"archive/tar"
	gozip "archive/zip"
	"compress/gzip"
	"fmt"
	"io"
	"os"
	"runtime"
	"strings"

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
		return Archive{}, fmt.Errorf("failed to open zip reader %v\n\t%w", archiveFilePath.LocalString(), err)
	}
	defer r.Close()

	filePaths, err := zip.GetFilePaths(r)
	if err != nil {
		return Archive{}, fmt.Errorf("failed to extract file paths from %v\n\t%w", archiveFilePath.LocalString(), err)
	}

	filePathRoot := path.ExtractLongestCommonPath(filePaths...)

	// If only one file, it must be tooth.json. Then we should use the directory of the file as the root.
	if len(filePaths) == 1 {
		filePathRootDir, err := filePathRoot.Dir()
		if err != nil {
			return Archive{}, fmt.Errorf("failed to get directory of tooth.json\n\t%w", err)
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
		return Archive{}, fmt.Errorf("failed to open tooth.json\n\t%w", err)
	}
	defer toothJSONFileReader.Close()

	toothJSONBytes, err := io.ReadAll(toothJSONFileReader)
	if err != nil {
		return Archive{}, fmt.Errorf("failed to read tooth.json\n\t%w", err)
	}

	// Parse tooth.json.
	metadata, err := MakeMetadata(toothJSONBytes)
	if err != nil {
		return Archive{}, fmt.Errorf("failed to parse tooth.json\n\t%w", err)
	}

	// Convert to platform-specific metadata.
	metadata, err = metadata.ToPlatformSpecific(runtime.GOOS, runtime.GOARCH)
	if err != nil {
		return Archive{}, fmt.Errorf("failed to convert to platform-specific metadata\n\t%w", err)
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
		return Archive{}, fmt.Errorf("failed to get asset URL\n\t%w", err)
	}

	if (assetArchiveFilePath.IsEmpty() && (assetURL.String() != "")) ||
		(!assetArchiveFilePath.IsEmpty() && (assetURL.String() == "")) {
		return Archive{}, fmt.Errorf("asset archive file path and asset URL must be both specified or both empty")
	}

	var filePaths []path.Path
	if assetArchiveFilePath.IsEmpty() {
		// Extract common prefix and prepend it to all file paths in file.place.
		if strings.HasSuffix(ar.filePath.LocalString(), ".zip") {
			r, err := gozip.OpenReader(ar.filePath.LocalString())
			if err != nil {
				return Archive{}, fmt.Errorf("failed to open zip reader %v\n\t%w", ar.filePath.LocalString(), err)
			}
			defer r.Close()

			filePaths, err = zip.GetFilePaths(r)
			if err != nil {
				return Archive{}, fmt.Errorf("failed to extract file paths from %v\n\t%w", ar.filePath.LocalString(), err)
			}

		} else if strings.HasSuffix(ar.filePath.LocalString(), ".tar.gz") {
			file, err := os.Open(ar.filePath.LocalString())
			if err != nil {
				return Archive{}, fmt.Errorf("failed to open file %v\n\t%w", ar.filePath.LocalString(), err)
			}
			gzr, err := gzip.NewReader(file)
			if err != nil {
				return Archive{}, fmt.Errorf("failed to open gzip reader %v\n\t%w", ar.filePath.LocalString(), err)
			}
			defer gzr.Close()
			r := tar.NewReader(gzr)

			for f, err := r.Next(); err != io.EOF; f, err = r.Next() {
				if err != nil {
					return Archive{}, fmt.Errorf("failed to read file paths from %v\n\t%w", ar.filePath.LocalString(), err)
				}
				// Skip directories.
				if f.Typeflag == tar.TypeDir {
					continue
				}
				filePath, err := path.Parse(f.Name)
				if err != nil {
					return Archive{}, fmt.Errorf("failed to parse file paths from %v\n\t%w", ar.filePath.LocalString(), err)
				}
				filePaths = append(filePaths, filePath)
			}

		}
		fmt.Println(filePaths)
		filePathRoot := path.ExtractLongestCommonPath(filePaths...)

		newMetadata := ar.metadata
		newMetadataPrefixPrepended := newMetadata.ToFilePathPrefixPrepended(filePathRoot)
		newMetadataWildcardPopulated, err := newMetadataPrefixPrepended.ToWildcardPopulated(filePaths)
		if err != nil {
			return Archive{}, fmt.Errorf("failed to populate wildcards\n\t%w", err)
		}

		return Archive{
			metadata:      newMetadataWildcardPopulated,
			filePath:      ar.filePath,
			assetFilePath: ar.filePath,
		}, nil
	} else {
		var filePaths []path.Path
		if strings.HasSuffix(assetArchiveFilePath.LocalString(), ".zip") {
			r, err := gozip.OpenReader(assetArchiveFilePath.LocalString())
			if err != nil {
				return Archive{}, fmt.Errorf("failed to open zip reader %v\n\t%w", assetArchiveFilePath.LocalString(), err)
			}
			defer r.Close()

			filePaths, err = zip.GetFilePaths(r)
			if err != nil {
				return Archive{}, fmt.Errorf("failed to extract file paths from %v\n\t%w", assetArchiveFilePath.LocalString(), err)
			}

		} else if strings.HasSuffix(assetArchiveFilePath.LocalString(), ".tar.gz") {
			file, err := os.Open(assetArchiveFilePath.LocalString())
			if err != nil {
				return Archive{}, fmt.Errorf("failed to open file %v\n\t%w", assetArchiveFilePath.LocalString(), err)
			}
			gzr, err := gzip.NewReader(file)
			if err != nil {
				return Archive{}, fmt.Errorf("failed to open gzip reader %v\n\t%w", assetArchiveFilePath.LocalString(), err)
			}
			defer gzr.Close()
			r := tar.NewReader(gzr)

			for f, err := r.Next(); err != io.EOF; f, err = r.Next() {
				if err != nil {
					return Archive{}, fmt.Errorf("failed to read file paths from %v\n\t%w", assetArchiveFilePath.LocalString(), err)
				}
				// Skip directories.
				if f.Typeflag == tar.TypeDir {
					continue
				}
				filePath, err := path.Parse(f.Name)
				if err != nil {
					return Archive{}, fmt.Errorf("failed to parse file paths from %v\n\t%w", assetArchiveFilePath.LocalString(), err)
				}
				filePaths = append(filePaths, filePath)
			}

		}
		fmt.Println(filePaths)

		newMetadata := ar.metadata
		newMetadataWildcardPopulated, err := newMetadata.ToWildcardPopulated(filePaths)
		if err != nil {
			return Archive{}, fmt.Errorf("failed to populate wildcards\n\t%w", err)
		}

		return Archive{
			metadata:      newMetadataWildcardPopulated,
			filePath:      ar.filePath,
			assetFilePath: assetArchiveFilePath,
		}, nil
	}
}
