// Package context includes the context of Lip.
package context

import "errors"

//------------------------------------------------------------------------------
// Constants

// Version is the version of Lip.
const Version = "0.1.0"

const DefaultGoproxy = "https://goproxy.io"

//------------------------------------------------------------------------------
// Variables

// Goproxy is the goproxy address.
var Goproxy string

//------------------------------------------------------------------------------
// Functions

// Validate validates the context.
func Validate() error {
	if Goproxy == "" {
		return errors.New("context.Goproxy is empty")
	}

	return nil
}
