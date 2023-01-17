// Package context includes the context of Lip.
package context

import "github.com/liteldev/lip/utils/version"

//------------------------------------------------------------------------------
// Constants

// Version is the version of Lip.
const VersionString = "0.1.0"

const DefaultGoproxy = "https://goproxy.io"

//------------------------------------------------------------------------------
// Variables

// Version is the version of Lip.
var Version version.Version

// Goproxy is the goproxy address.
var Goproxy string
