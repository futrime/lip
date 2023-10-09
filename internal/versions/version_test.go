package versions

import (
	"testing"
)

func TestNewFromString(t *testing.T) {
	testList := []struct {
		input  string
		output string
	}{
		{"1.0.0", "1.0.0"},
		{"1.0.0-beta.1", "1.0.0-beta.1"},
		{"3.2.1", "3.2.1"},
		{"3.2.0-beta", "3.2.0-beta"},
		{"3.2.0-beta.1", "3.2.0-beta.1"},
		{"0.0.1", "0.0.1"},
		{"0.0.0", "0.0.0"},
	}

	for index, test := range testList {
		version, err := NewFromString(test.input)
		if err != nil {
			t.Fatalf("error at test %d: %v", index, err.Error())
		}

		if version.String() != test.output {
			t.Errorf("wrong output at test %d: %v != %v", index, version.String(), test.output)
		}
	}
}
