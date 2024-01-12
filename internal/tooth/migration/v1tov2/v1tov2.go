package v1tov2

import (
	"encoding/json"
	"fmt"
	"runtime"
	"strings"

	"github.com/xeipuuv/gojsonschema"
)

const v1JSONSchema = `
{
    "$schema": "https://json-schema.org/draft-07/schema#",
    "type": "object",
    "required": [
        "format_version",
        "tooth",
        "version"
    ],
    "properties": {
        "format_version": {
            "const": 1
        },
        "tooth": {
            "type": "string"
        },
        "version": {
            "type": "string"
        },
        "dependencies": {
            "type": "object",
            "patternProperties": {
                "^.*$": {
                    "type": "array",
                    "minItems": 1,
                    "items": {
                        "type": "array",
                        "items": {
                            "type": "string"
                        }
                    }
                }
            }
        },
        "information": {
            "type": "object"
        },
        "placement": {
            "type": "array",
            "items": {
                "type": "object",
                "required": [
                    "source",
                    "destination"
                ],
                "properties": {
                    "source": {
                        "type": "string"
                    },
                    "destination": {
                        "type": "string"
                    },
                    "GOOS": {
                        "type": "string"
                    },
                    "GOARCH": {
                        "type": "string"
                    }
                }
            }
        },
        "possession": {
            "type": "array",
            "items": {
                "type": "string"
            }
        },
        "commands": {
            "type": "array",
            "items": {
                "type": "object",
                "required": [
                    "type",
                    "commands",
                    "GOOS"
                ],
                "properties": {
                    "type": {
                        "enum": [
                            "install",
                            "uninstall"
                        ]
                    },
                    "commands": {
                        "type": "array",
                        "items": {
                            "type": "string"
                        }
                    },
                    "GOOS": {
                        "type": "string"
                    },
                    "GOARCH": {
                        "type": "string"
                    }
                }
            }
        }
    }
}
`

type v1RawMetadata struct {
	FormatVersion int               `json:"format_version"`
	Tooth         string            `json:"tooth"`
	Version       string            `json:"version"`
	Dependencies  v1Dependencies    `json:"dependencies,omitempty"`
	Information   v1Information     `json:"information,omitempty"`
	Placement     []v1PlacementItem `json:"placement,omitempty"`
	Possession    []string          `json:"possession,omitempty"`
	Commands      []v1CommandsItem  `json:"commands,omitempty"`
}

type v1Dependencies map[string]v1DependenciesItem

type v1DependenciesItem [][]string

type v1Information struct {
	Name        string `json:"name,omitempty"`
	Description string `json:"description,omitempty"`
	Author      string `json:"author,omitempty"`
}

type v1PlacementItem struct {
	Source      string `json:"source"`
	Destination string `json:"destination"`
	GOOS        string `json:"GOOS,omitempty"`
	GOARCH      string `json:"GOARCH,omitempty"`
}

type v1CommandsItem struct {
	Type     string   `json:"type"`
	Commands []string `json:"commands"`
	GOOS     string   `json:"GOOS"`
	GOARCH   string   `json:"GOARCH,omitempty"`
}

type RawMetadata struct {
	FormatVersion int             `json:"format_version"`
	Tooth         string          `json:"tooth"`
	Version       string          `json:"version"`
	Info          RawMetadataInfo `json:"info"`

	AssetURL      string              `json:"asset_url,omitempty"`
	Commands      RawMetadataCommands `json:"commands,omitempty"`
	Dependencies  map[string]string   `json:"dependencies,omitempty"`
	Prerequisites map[string]string   `json:"prerequisites,omitempty"`
	Files         RawMetadataFiles    `json:"files,omitempty"`

	Platforms []RawMetadataPlatformsItem `json:"platforms,omitempty"`
}

type RawMetadataInfo struct {
	Name        string   `json:"name"`
	Description string   `json:"description"`
	Author      string   `json:"author"`
	Tags        []string `json:"tags"`
	Source      string   `json:"source,omitempty"`
}

type RawMetadataCommands struct {
	PreInstall    []string `json:"pre_install,omitempty"`
	PostInstall   []string `json:"post_install,omitempty"`
	PreUninstall  []string `json:"pre_uninstall,omitempty"`
	PostUninstall []string `json:"post_uninstall,omitempty"`
}

type RawMetadataFiles struct {
	Place    []RawMetadataFilesPlaceItem `json:"place,omitempty"`
	Preserve []string                    `json:"preserve,omitempty"`
	Remove   []string                    `json:"remove,omitempty"`
}

type RawMetadataFilesPlaceItem struct {
	Src  string `json:"src"`
	Dest string `json:"dest"`
}

type RawMetadataPlatformsItem struct {
	GOARCH string `json:"goarch,omitempty"`
	GOOS   string `json:"goos"`

	AssetURL      string              `json:"asset_url,omitempty"`
	Commands      RawMetadataCommands `json:"commands,omitempty"`
	Dependencies  map[string]string   `json:"dependencies,omitempty"`
	Prerequisites map[string]string   `json:"prerequisites,omitempty"`
	Files         RawMetadataFiles    `json:"files,omitempty"`
}

// Migrate migrates the metadata from v1 to v2.
func Migrate(jsonBytes []byte) ([]byte, error) {
	// Validate JSON against schema.
	v1SchemaLoader := gojsonschema.NewStringLoader(v1JSONSchema)
	v1DocumentLoader := gojsonschema.NewBytesLoader(jsonBytes)

	result, err := gojsonschema.Validate(v1SchemaLoader, v1DocumentLoader)
	if err != nil {
		return nil, fmt.Errorf("error validating JSON against schema: %w", err)
	}

	if !result.Valid() {
		return nil, fmt.Errorf("JSON is not valid against schema: %v", result.Errors())
	}

	// Unmarshal JSON into struct.
	var v1RawMetadata v1RawMetadata
	if err := json.Unmarshal(jsonBytes, &v1RawMetadata); err != nil {
		return nil, fmt.Errorf("error unmarshaling JSON into struct: %w", err)
	}

	// Migrate struct.
	v2RawMetadata := RawMetadata{
		FormatVersion: 2,
		Tooth:         v1RawMetadata.Tooth,
		Version:       v1RawMetadata.Version,
		Info: RawMetadataInfo{
			Name:        v1RawMetadata.Information.Name,
			Description: v1RawMetadata.Information.Description,
			Author:      v1RawMetadata.Information.Author,
			Source:      "",
			Tags:        make([]string, 0),
		},
		Commands: RawMetadataCommands{
			PreInstall:    make([]string, 0),
			PostInstall:   make([]string, 0),
			PreUninstall:  make([]string, 0),
			PostUninstall: make([]string, 0),
		},
		Dependencies: make(map[string]string),
		Files: RawMetadataFiles{
			Place:  make([]RawMetadataFilesPlaceItem, 0),
			Remove: v1RawMetadata.Possession,
		},
		Platforms: make([]RawMetadataPlatformsItem, 0),
	}

	// Solve dependencies.
	for toothRepoPath, depMatrix := range v1RawMetadata.Dependencies {
		depInnerStringList := make([]string, 0)
		for _, andDepList := range depMatrix {
			depInnerStringList = append(depInnerStringList, strings.Join(andDepList, " "))
		}

		v2RawMetadata.Dependencies[toothRepoPath] = strings.Join(depInnerStringList, " || ")
	}

	// Solve commands
	for _, v1Command := range v1RawMetadata.Commands {
		if v1Command.GOOS != "" && runtime.GOOS != v1Command.GOOS {
			continue
		}

		if v1Command.GOARCH != "" && runtime.GOARCH != v1Command.GOARCH {
			continue
		}

		switch v1Command.Type {
		case "install":
			v2RawMetadata.Commands.PostInstall = append(
				v2RawMetadata.Commands.PostInstall, v1Command.Commands...)
		case "uninstall":
			v2RawMetadata.Commands.PreUninstall = append(
				v2RawMetadata.Commands.PreUninstall, v1Command.Commands...)
		}
	}

	// Solve files
	for _, v1Placement := range v1RawMetadata.Placement {
		if v1Placement.GOOS != "" && runtime.GOOS != v1Placement.GOOS {
			continue
		}

		if v1Placement.GOARCH != "" && runtime.GOARCH != v1Placement.GOARCH {
			continue
		}

		v2RawMetadata.Files.Place = append(
			v2RawMetadata.Files.Place, RawMetadataFilesPlaceItem{
				Src:  v1Placement.Source,
				Dest: strings.TrimSuffix(v1Placement.Destination, "*"),
			})
	}

	resultJSONBytes, err := json.Marshal(v2RawMetadata)
	if err != nil {
		return nil, fmt.Errorf("error marshaling struct into JSON: %w", err)
	}

	return resultJSONBytes, nil
}
