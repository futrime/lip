package toothrecord

// IsToothInstalled returns true if the tooth is installed.
func IsToothInstalled(toothPath string) (bool, error) {
	// Get the tooth record list.
	recordList, err := ListAll()
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
