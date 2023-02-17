package toothmetadata

import (
	"testing"

	"github.com/liteldev/lip/utils/versions"
	"github.com/liteldev/lip/utils/versions/versionmatch"
)

func TestNewFromJSON(t *testing.T) {
	// Read test data
	jsonData := []byte(`
{
  "format_version": 1,
  "dependencies": {
    "test.test/test/depend": [
      [
        ">=1.0.0",
        "<=1.1.0"
      ],
      [
        "2.0.x"
      ]
    ]
  },
  "information": {
    "author": "test author",
    "description": "test description",
    "homepage": "test homepage",
    "license": "test license",
    "name": "test name"
  },
  "placement": [
    {
      "destination": "test/testdirectory",
      "source": "test/test.test"
    }
  ],
  "possession": [
    "plugins/LiteLoader/"
  ],
  "tooth": "test.test/test/test",
  "version": "1.0.0"
}
	`)

	// Test
	metadata, err := NewFromJSON(jsonData)
	if err != nil {
		t.Fatalf(err.Error())
	}

	// Check
	if metadata.ToothPath != "test.test/test/test" {
		t.Errorf("metadata.ToothPath is not correct")
	}

	if metadata.Version.String() != "1.0.0" {
		t.Errorf("metadata.Version is not correct")
	}

	if len(metadata.Dependencies) != 1 {
		t.Errorf("metadata.Dependencies is not correct")
	}

	if len(metadata.Dependencies["test.test/test/depend"]) != 2 {
		t.Errorf("metadata.Dependencies is not correct")
	}

	if len(metadata.Dependencies["test.test/test/depend"][0]) != 2 {
		t.Errorf("metadata.Dependencies is not correct")
	}

	if len(metadata.Dependencies["test.test/test/depend"][1]) != 1 {
		t.Errorf("metadata.Dependencies is not correct")
	}

	if metadata.Dependencies["test.test/test/depend"][0][0].String() != ">=1.0.0" {
		t.Errorf("metadata.Dependencies is not correct")
	}

	if metadata.Dependencies["test.test/test/depend"][0][1].String() != "<=1.1.0" {
		t.Errorf("metadata.Dependencies is not correct")
	}

	if metadata.Dependencies["test.test/test/depend"][1][0].String() != "2.0.x" {
		t.Errorf("metadata.Dependencies is not correct")
	}

	if metadata.Information.Name != "test name" {
		t.Errorf("metadata.Information is not correct")
	}

	if metadata.Information.Description != "test description" {
		t.Errorf("metadata.Information is not correct")
	}

	if metadata.Information.Author != "test author" {
		t.Errorf("metadata.Information is not correct")
	}

	if metadata.Information.License != "test license" {
		t.Errorf("metadata.Information is not correct")
	}

	if metadata.Information.Homepage != "test homepage" {
		t.Errorf("metadata.Information is not correct")
	}

	if len(metadata.Placement) != 1 {
		t.Errorf("metadata.Placement is not correct")
	}

	if metadata.Placement[0].Source != "test/test.test" {
		t.Errorf("metadata.Placement is not correct")
	}

	if metadata.Placement[0].Destination != "test/testdirectory" {
		t.Errorf("metadata.Placement is not correct")
	}

	if len(metadata.Possession) != 1 {
		t.Errorf("metadata.Possession is not correct")
	}

	if metadata.Possession[0] != "plugins/LiteLoader/" {
		t.Errorf("metadata.Possession is not correct")
	}
}

func TestJSON(t *testing.T) {
	// Create test data
	version, _ := versions.NewFromString(`1.0.0`)

	versionMatch0, _ := versionmatch.NewFromString(`>=1.0.0`)
	versionMatch1, _ := versionmatch.NewFromString(`<=1.1.0`)
	versionMatch2, _ := versionmatch.NewFromString(`2.0.x`)

	metadata := Metadata{
		ToothPath: "test.test/test/test",
		Version:   version,
		Dependencies: map[string]([][]versionmatch.VersionMatch){
			"test.test/test/depend": {
				{versionMatch0, versionMatch1},
				{versionMatch2},
			},
		},
		Information: InfoStruct{
			Name:        "test name",
			Description: "test description",
			Author:      "test author",
			License:     "test license",
			Homepage:    "test homepage",
		},
		Placement: []PlacementStruct{
			{
				Source:      "test/test.test",
				Destination: "test/testdirectory",
			},
		},
		Possession: []string{
			"plugins/LiteLoader/",
		},
	}

	// Test
	json, err := metadata.JSON()
	if err != nil {
		t.Fatalf(err.Error())
	}

	// Save json
	t.Log(string(json))
}
