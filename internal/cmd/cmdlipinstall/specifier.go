package cmdlipinstall

import (
	"fmt"

	"github.com/blang/semver/v4"
	"github.com/lippkg/lip/internal/context"
	"github.com/lippkg/lip/internal/must"
	specifierpkg "github.com/lippkg/lip/internal/specifier"
	"github.com/lippkg/lip/internal/tooth"
)

// downloadToothRepoSpecifier downloads the tooth specified by the specifier and returns
// the path to the downloaded tooth.
func downloadToothRepoSpecifier(ctx *context.Context,
	specifier specifierpkg.Specifier) (tooth.Archive, error) {
	if specifier.Kind() != specifierpkg.ToothRepoKind {
		return tooth.Archive{}, fmt.Errorf("invalid specifier kind %v", specifier.Kind())
	}

	toothRepoPath := must.Must(specifier.ToothRepoPath())

	// Parse or get the tooth version.

	var toothVersion semver.Version
	isToothVersionSpecified, err := specifier.IsToothVersionSpecified()
	if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to get is tooth version specified: %w", err)
	}

	if isToothVersionSpecified {
		toothVersion = must.Must(specifier.ToothVersion())

	} else {
		latestVersion, err := tooth.GetLatestVersion(ctx, toothRepoPath)
		if err != nil {
			return tooth.Archive{}, fmt.Errorf("failed to look up tooth version: %w", err)
		}

		toothVersion = latestVersion
	}

	archive, err := downloadToothArchiveIfNotCached(ctx, toothRepoPath, toothVersion)
	if err != nil {
		return tooth.Archive{}, fmt.Errorf("failed to download archive of %v@%v: %w", toothRepoPath,
			toothVersion, err)
	}

	return archive, nil
}

// resolveSpecifiers parses the specifier string list and
// downloads the tooth specified by the specifier, and returns the list of
// downloaded tooth archives.
func resolveSpecifiers(ctx *context.Context,
	specifiers []specifierpkg.Specifier) ([]tooth.Archive, error) {

	archiveList := make([]tooth.Archive, 0)

	for _, specifier := range specifiers {
		var archive tooth.Archive

		switch specifier.Kind() {
		case specifierpkg.ToothArchiveKind:
			archivePath := must.Must(specifier.ToothArchivePath())
			localArchive, err := tooth.MakeArchive(archivePath)
			if err != nil {
				return nil, fmt.Errorf("failed to open archive %v: %w", archivePath.LocalString(), err)
			}

			archive = localArchive

		case specifierpkg.ToothRepoKind:
			downloadedArchive, err := downloadToothRepoSpecifier(ctx, specifier)
			if err != nil {
				return nil, fmt.Errorf("failed to download specifier %v: %w", specifier, err)
			}

			archive = downloadedArchive

		default:
			panic("unreachable")
		}

		archiveList = append(archiveList, archive)
	}

	return archiveList, nil
}
