package paths

import "testing"

func TestIsAncesterOf(t *testing.T) {
	type InputType struct {
		ancestor string
		path     string
	}

	testList := []struct {
		input  InputType
		output bool
	}{
		{InputType{"", ""}, false},
		{InputType{"", "a"}, true},
		{InputType{"a", ""}, false},
		{InputType{"a", "a"}, false},
		{InputType{"a", "a/b"}, true},
		{InputType{"a", "a/b/c"}, true},
		{InputType{"a/b", "a/b/c"}, true},
		{InputType{"a/b/c", "a/b/c"}, false},
		{InputType{"C:\\a", "C:\\a\\b"}, true},
		{InputType{"C:\\a", "C:\\a\\b\\c"}, true},
		{InputType{"C:\\a\\b", "C:\\a\\b\\c"}, true},
		{InputType{"C:\\a\\b\\c", "C:\\a\\b\\c"}, false},
		{InputType{"C:\\a", "C:\\a"}, false},
		{InputType{"C:\\a", "C:\\a\\"}, false},
		{InputType{"C:\\a\\", "C:\\a"}, false},
		{InputType{"C:\\a\\", "C:\\a\\"}, false},
	}

	for index, test := range testList {
		var err error

		result, err := IsAncesterOf(test.input.ancestor, test.input.path)
		if err != nil {
			t.Fatalf("error at test %d: %s", index, err.Error())
		}

		if result != test.output {
			t.Errorf("wrong output at test %d: %t != %t", index, result, test.output)
		}
	}
}

func TestEqual(t *testing.T) {
	type InputType struct {
		path1 string
		path2 string
	}

	testList := []struct {
		input  InputType
		output bool
	}{
		{InputType{"", ""}, true},
		{InputType{"", "a"}, false},
		{InputType{"a", ""}, false},
		{InputType{"a", "a"}, true},
		{InputType{"a", "a/b"}, false},
		{InputType{"a", "a/b/c"}, false},
		{InputType{"a/b", "a/b/c"}, false},
		{InputType{"a/b/c", "a/b/c"}, true},
		{InputType{"C:\\a", "C:\\a\\b"}, false},
		{InputType{"C:\\a", "C:\\a\\b\\c"}, false},
		{InputType{"C:\\a\\b", "C:\\a\\b\\c"}, false},
		{InputType{"C:\\a\\b\\c", "C:\\a\\b\\c"}, true},
		{InputType{"C:\\a", "C:\\a"}, true},
		{InputType{"C:\\a", "C:\\a\\"}, true},
		{InputType{"C:\\a\\", "C:\\a"}, true},
		{InputType{"C:\\a\\", "C:\\a\\"}, true},
	}

	for index, test := range testList {
		var err error

		result, err := Equal(test.input.path1, test.input.path2)
		if err != nil {
			t.Fatalf("error at test %d: %s", index, err.Error())
		}

		if result != test.output {
			t.Errorf("wrong output at test %d: %t != %t", index, result, test.output)
		}
	}
}
