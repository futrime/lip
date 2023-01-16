package tooth

import "github.com/liteldev/lip/tooth/toothrecord"

// IsInstalled returns true if the tooth is installed.
func IsInstalled(toothPath string) (bool, error) {
	// Get the tooth record list.
	recordList, err := toothrecord.ListAll()
	if err != nil {
		return false, err
	}

	// Check if the tooth is installed.
	for _, record := range recordList {
		if record.ToothPath == toothPath {
			return true, nil
		}
	}

	return false, nil
}
