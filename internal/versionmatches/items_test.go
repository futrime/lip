package versionmatches

import (
	"testing"

	"github.com/lippkg/lip/internal/versions"
)

func TestNew(t *testing.T) {
	type Input struct {
		version   string
		matchType MatchType
	}

	testList := []struct {
		input  Input
		output string
	}{
		{Input{"1.0.0", EqualMatchType}, "1.0.0"},
		{Input{"2.0.0", GreaterThanMatchType}, ">2.0.0"},
		{Input{"1.1.0", GreaterThanOrEqualMatchType}, ">=1.1.0"},
		{Input{"0.1.0", LessThanMatchType}, "<0.1.0"},
		{Input{"0.0.1", LessThanOrEqualMatchType}, "<=0.0.1"},
		{Input{"3.2.0-beta.1", InequalMatchType}, "!3.2.0-beta.1"},
		{Input{"3.2.0", CompatibleMatchType}, "3.2.x"},
	}

	for index, test := range testList {
		version, err := versions.NewFromString(test.input.version)
		if err != nil {
			t.Fatalf("error at test %d: %v", index, err.Error())
		}

		versionMatch, err := NewItem(version, test.input.matchType)
		if err != nil {
			t.Fatalf("error at test %d: %v", index, err.Error())
		}

		if versionMatch.version != version {
			t.Errorf("wrong version at test %d", index)
		}

		if versionMatch.matchType != test.input.matchType {
			t.Errorf("wrong matchType at test %d", index)
		}

		if versionMatch.String() != testList[index].output {
			t.Errorf("wrong output at test %d: %v != %v", index, versionMatch.String(), test.output)
		}
	}
}

func TestNewFromString(t *testing.T) {
	testList := []struct {
		input  string
		output string
	}{
		{"1.0.0", "1.0.0"},
		{">2.0.0", ">2.0.0"},
		{">=1.1.0", ">=1.1.0"},
		{"<0.1.0", "<0.1.0"},
		{"<=0.0.1", "<=0.0.1"},
		{"!3.2.0", "!3.2.0"},
		{"3.2.x", "3.2.x"},
	}

	for index, test := range testList {
		versionMatch, err := NewItemFromString(test.input)
		if err != nil {
			t.Fatalf("error at test %d: %v", index, err.Error())
		}

		if versionMatch.String() != test.output {
			t.Errorf("wrong output at test %d: %v != %v", index, versionMatch.String(), test.output)
		}
	}
}
