// Package context includes the context of Lip.
package context

import (
	"os"
	"strings"

	"github.com/liteldev/lip/utils/versions"
)

//------------------------------------------------------------------------------
// Constants

// Version is the version of Lip.
var VersionString = "v0.0.0"

const DefaultGoproxyURL = "https://goproxy.io"

const DefaultRegistryURL = "https://registry.litebds.com"

//------------------------------------------------------------------------------
// Variables

// Version is the version of Lip.
var Version versions.Version

// GoproxyList is the goproxy address.
var GoproxyList []string

// RegistryURL is the registry address.
var RegistryURL string

//------------------------------------------------------------------------------
// Functions

// Init initializes the
func Init() {
	var err error

	// Set Version.
	Version, err = versions.NewFromString(strings.TrimPrefix(VersionString, "v"))
	if err != nil {
		Version, _ = versions.NewFromString("0.0.0")
	}

	// Set Goproxy.
	if goproxy := os.Getenv("LIP_GOPROXY"); goproxy != "" {
		GoproxyList = strings.Split(goproxy, ",")
	} else {
		GoproxyList = []string{DefaultGoproxyURL}
	}

	// Set RegistryURL.
	if registryURL := os.Getenv("LIP_REGISTRY"); registryURL != "" {
		RegistryURL = registryURL
	} else {
		RegistryURL = DefaultRegistryURL
	}
}
