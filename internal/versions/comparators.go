package versions

// ---------------------------------------------------------------------
// Version comparison functions

// Equal returns true if the two versions are equal.
func Equal(v1, v2 Version) bool {
	return v1.major == v2.major &&
		v1.minor == v2.minor &&
		v1.patch == v2.patch &&
		v1.preReleaseName == v2.preReleaseName &&
		v1.preReleaseNumber == v2.preReleaseNumber
}

// GreaterThan returns true if the first version is greater than the second
// version.
func GreaterThan(v1, v2 Version) bool {
	if v1.major > v2.major {
		return true
	}
	if v1.major < v2.major {
		return false
	}
	if v1.minor > v2.minor {
		return true
	}
	if v1.minor < v2.minor {
		return false
	}
	if v1.patch > v2.patch {
		return true
	}
	if v1.patch < v2.patch {
		return false
	}
	if v1.preReleaseName != "" && v2.preReleaseName == "" {
		return false
	}
	if v1.preReleaseName == "" && v2.preReleaseName != "" {
		return true
	}
	if v1.preReleaseName > v2.preReleaseName {
		return true
	}
	if v1.preReleaseName < v2.preReleaseName {
		return false
	}
	if v1.preReleaseNumber > v2.preReleaseNumber {
		return true
	}
	return false
}

// GreaterThanOrEqual returns true if the first version is greater than or equal
// to the second version.
func GreaterThanOrEqual(v1, v2 Version) bool {
	return GreaterThan(v1, v2) || Equal(v1, v2)
}

// LessThan returns true if the first version is less than the second version.
func LessThan(v1, v2 Version) bool {
	return !GreaterThanOrEqual(v1, v2)
}

// LessThanOrEqual returns true if the first version is less than or equal to
// the second version.
func LessThanOrEqual(v1, v2 Version) bool {
	return !GreaterThan(v1, v2)
}

// Compatible returns true if the two versions are compatible.
func Compatible(v1, v2 Version) bool {
	return v1.major == v2.major &&
		v1.minor == v2.minor &&
		(v1.preReleaseName == "" || v2.preReleaseName == "")
}
