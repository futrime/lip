// Package logger deals with the output of Lip.
package logger

import (
	"fmt"

	color "github.com/fatih/color"
)

// Info prints a message to the console.
func Info(format string, a ...interface{}) {
	fmt.Printf(format, a...)
	// Print a new line.
	fmt.Println()
}

// Warning prints a warning message to the console.
func Warning(format string, a ...interface{}) {
	color.HiYellow("WARNING: "+format, a...)
}

// Error prints an error message to the console.
func Error(format string, a ...interface{}) {
	color.HiRed("ERROR: "+format, a...)
}

func Prompt(format string, a ...interface{}) string {
	var input string
	fmt.Printf(format, a...)
	fmt.Scanln(&input)
	return input
}

// Fatal prints an error message to the console and exits.
func SetColor(status bool) {
	color.NoColor = !status
}
