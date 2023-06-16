package installing

import (
	"github.com/lippkg/lip/pkg/contexts"
	"github.com/lippkg/lip/pkg/versions"
)

// CheckIsToothManuallyInstalled checks if a tooth is manually installed.
func CheckIsToothManuallyInstalled(ctx contexts.Context,
	toothRepo string) (bool, error) {
	var err error

	// TODO: Check if the tooth is manually installed.

	return false, err
}

// LookUpVersion returns the correct version of the tooth specified by the
// specifier.
func LookUpVersion(ctx contexts.Context,
	toothRepo string) (versions.Version, error) {
	// TODO
	return versions.Version{}, nil
}
