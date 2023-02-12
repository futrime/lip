// Package logger deals with the output of Lip.
package logger

import (
	"fmt"

	color "github.com/fatih/color"
)

type LoggingLevelType int

const (
	DebugLevel LoggingLevelType = iota
	InfoLevel
	WarningLevel
	ErrorLevel
	CriticalLevel
)

var loggingLevel LoggingLevelType = InfoLevel

// Debug prints a debug message to the console.
func Debug(format string, a ...interface{}) {
	if loggingLevel <= DebugLevel {
		color.HiBlack("DEBUG: "+format, a...)
	}
}

// Info prints a message to the console.
func Info(format string, a ...interface{}) {
	if loggingLevel <= InfoLevel {
		fmt.Printf(format, a...)
		// Print a new line.
		fmt.Println()
	}
}

// Warning prints a warning message to the console.
func Warning(format string, a ...interface{}) {
	if loggingLevel <= WarningLevel {
		color.HiYellow("WARNING: "+format, a...)
	}
}

// Error prints an error message to the console.
func Error(format string, a ...interface{}) {
	if loggingLevel <= ErrorLevel {
		color.HiRed("ERROR: "+format, a...)
	}
}

// Critical prints a critical message to the console.
func Critical(format string, a ...interface{}) {
	if loggingLevel <= CriticalLevel {
		color.HiRed("CRITICAL: "+format, a...)
	}
}

// SetLevel sets the logging level.
func SetLevel(level LoggingLevelType) {
	loggingLevel = level
}
