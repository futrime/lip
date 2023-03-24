// Package metadata contains the metadata of a tooth.
package toothmetadata

import (
	"bytes"
	"encoding/json"
	"errors"
	"strings"

	"github.com/liteldev/lip/tooth/toothutils"
	"github.com/liteldev/lip/utils/versions"
	"github.com/liteldev/lip/utils/versions/versionmatch"
	"github.com/xeipuuv/gojsonschema"
)

// InfoStruct is the struct that contains the information of a tooth.
type InfoStruct struct {
	Name        string
	Description string
	Author      string
	License     string
	Homepage    string
}

// PlacementStruct is the struct that contains the source and destination of a placement.
type PlacementStruct struct {
	Source      string
	Destination string
	GOOS        string
	GOARCH      string
}

// CommandStruct is the struct that contains the type, commands, GOOS and GOARCH of a command.
type CommandStruct struct {
	Type     string
	Commands []string
	GOOS     string
	GOARCH   string
}

// ConfirmationStruct is the struct that contains the type, message, GOOS and GOARCH of a confirmation.
type ConfirmationStruct struct {
	Type    string
	Message string
	GOOS    string
	GOARCH  string
}

type ToolStruct struct {
	Name        string
	Description string
	Entrypoints []ToolEntrypointStruct
}

type ToolEntrypointStruct struct {
	Path   string
	GOOS   string
	GOARCH string
}

// Metadata is the struct that contains all the metadata of a tooth.
type Metadata struct {
	ToothPath    string
	Version      versions.Version
	Dependencies map[string]([][]versionmatch.VersionMatch)
	Information  InfoStruct
	Placement    []PlacementStruct
	Possession   []string
	Commands     []CommandStruct
	Confirmation []ConfirmationStruct
	Tool         ToolStruct
}

const jsonSchema string = `
{
    "$schema": "https://json-schema.org/draft-07/schema",
    "type": "object",
    "additionalProperties": false,
    "required": [
        "format_version",
        "tooth",
        "version"
    ],
    "properties": {
        "format_version": {
            "enum": [
                1
            ]
        },
        "tooth": {
            "type": "string",
            "pattern": "^[a-zA-Z\\d-_\\.\\/]*$"
        },
        "version": {
            "type": "string",
            "pattern": "^\\d+\\.\\d+\\.(\\d+|0-[a-z]+(\\.[0-9]+)?)$"
        },
        "dependencies": {
            "type": "object",
            "additionalProperties": false,
            "patternProperties": {
                "^[a-zA-Z\\d-_\\.\\/]*$": {
                    "type": "array",
                    "uniqueItems": true,
                    "minItems": 1,
                    "additionalItems": false,
                    "items": {
                        "type": "array",
                        "uniqueItems": true,
                        "minItems": 1,
                        "additionalItems": false,
                        "items": {
                            "type": "string",
                            "pattern": "^((>|>=|<|<=|!)?\\d+\\.\\d+\\.\\d+|\\d+\\.\\d+\\.x)$"
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
            "additionalItems": false,
            "items": {
                "type": "object",
                "additionalProperties": false,
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
            "additionalItems": false,
            "items": {
                "type": "string"
            }
        },
        "commands": {
            "type": "array",
            "items": {
                "type": "object",
                "additionalProperties": false,
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
        },
        "confirmation": {
            "type": "array",
            "items": {
                "type": "object",
                "additionalProperties": false,
                "required": [
                    "type",
                    "message"
                ],
                "properties": {
                    "type": {
                        "enum": [
                            "install",
                            "uninstall"
                        ]
                    },
                    "message": {
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
        "tool": {
            "type": "object",
            "additionalProperties": false,
            "required": [
                "name",
                "description",
                "entrypoints"
            ],
            "properties": {
                "name": {
                    "type": "string",
                    "pattern": "(^[a-z\\d-]+|)$"
                },
                "description": {
                    "type": "string"
                },
                "entrypoints": {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "additionalProperties": false,
                        "required": [
                            "path",
                            "GOOS"
                        ],
                        "properties": {
                            "path": {
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
                }
            }
        }
    }
}
`

// NewFromJSON decodes a JSON byte array into a Metadata struct.
func NewFromJSON(jsonData []byte) (Metadata, error) {
	// Validate JSON schema.
	schemaLoader := gojsonschema.NewStringLoader(jsonSchema)
	documentLoader := gojsonschema.NewBytesLoader(jsonData)

	result, err := gojsonschema.Validate(schemaLoader, documentLoader)
	if err != nil {
		return Metadata{}, errors.New("JSON schema validation failed: " + err.Error())
	}

	if !result.Valid() {
		var errorString string
		for _, desc := range result.Errors() {
			errorString += desc.String() + " "
		}
		return Metadata{}, errors.New("JSON schema validation failed: " + errorString)
	}

	// Read to a map.
	var metadataMap map[string]interface{}
	err = json.Unmarshal(jsonData, &metadataMap)
	if err != nil {
		return Metadata{}, errors.New("Failed to decode JSON into metadata: " + err.Error())
	}

	// Parse to metadata.
	var metadata Metadata

	// Tooth path should be lower case.
	metadata.ToothPath = strings.ToLower(metadataMap["tooth"].(string))
	if !toothutils.IsValidToothPath(metadata.ToothPath) {
		return Metadata{}, errors.New("failed to decode JSON into metadata: invalid tooth path: " + metadata.ToothPath)
	}

	version, err := versions.NewFromString(metadataMap["version"].(string))

	if err != nil {
		return Metadata{}, errors.New("failed to decode JSON into metadata: " + err.Error())
	}
	metadata.Version = version

	metadata.Dependencies = make(map[string]([][]versionmatch.VersionMatch))
	if _, ok := metadataMap["dependencies"]; ok {
		for toothPath, versionMatchOuterList := range metadataMap["dependencies"].(map[string]interface{}) {
			// Tooth path should be lower case.
			toothPath = strings.ToLower(toothPath)

			metadata.Dependencies[toothPath] = make([][]versionmatch.VersionMatch, len(versionMatchOuterList.([]interface{})))
			for i, versionMatchInnerList := range versionMatchOuterList.([]interface{}) {
				metadata.Dependencies[toothPath][i] = make([]versionmatch.VersionMatch, len(versionMatchInnerList.([]interface{})))
				for j, versionMatch := range versionMatchInnerList.([]interface{}) {
					versionMatch, err := versionmatch.NewFromString(versionMatch.(string))
					if err != nil {
						return Metadata{}, errors.New("failed to decode JSON into metadata: " + err.Error())
					}

					metadata.Dependencies[toothPath][i][j] = versionMatch
				}
			}
		}
	}

	if _, ok := metadataMap["information"]; ok {
		if _, ok := metadataMap["information"].(map[string]interface{})["name"]; ok {
			metadata.Information.Name = metadataMap["information"].(map[string]interface{})["name"].(string)
		}
		if _, ok := metadataMap["information"].(map[string]interface{})["description"]; ok {
			metadata.Information.Description = metadataMap["information"].(map[string]interface{})["description"].(string)
		}
		if _, ok := metadataMap["information"].(map[string]interface{})["author"]; ok {
			metadata.Information.Author = metadataMap["information"].(map[string]interface{})["author"].(string)
		}
		if _, ok := metadataMap["information"].(map[string]interface{})["license"]; ok {
			metadata.Information.License = metadataMap["information"].(map[string]interface{})["license"].(string)
		}
		if _, ok := metadataMap["information"].(map[string]interface{})["homepage"]; ok {
			metadata.Information.Homepage = metadataMap["information"].(map[string]interface{})["homepage"].(string)
		}
	}

	if _, ok := metadataMap["placement"]; ok {
		metadata.Placement = make([]PlacementStruct, len(metadataMap["placement"].([]interface{})))
		for i, placement := range metadataMap["placement"].([]interface{}) {
			source := placement.(map[string]interface{})["source"].(string)
			destination := placement.(map[string]interface{})["destination"].(string)

			metadata.Placement[i].Source = source
			metadata.Placement[i].Destination = destination

			if _, ok := placement.(map[string]interface{})["GOOS"]; ok {
				metadata.Placement[i].GOOS = placement.(map[string]interface{})["GOOS"].(string)
			}

			if _, ok := placement.(map[string]interface{})["GOARCH"]; ok {
				metadata.Placement[i].GOARCH = placement.(map[string]interface{})["GOARCH"].(string)
			}
		}
	} else {
		metadata.Placement = make([]PlacementStruct, 0)
	}

	if _, ok := metadataMap["possession"]; ok {
		metadata.Possession = make([]string, len(metadataMap["possession"].([]interface{})))
		for i, possession := range metadataMap["possession"].([]interface{}) {
			metadata.Possession[i] = possession.(string)
		}
	} else {
		metadata.Possession = make([]string, 0)
	}

	if _, ok := metadataMap["commands"]; ok {
		metadata.Commands = make([]CommandStruct, len(metadataMap["commands"].([]interface{})))
		for i, command := range metadataMap["commands"].([]interface{}) {
			metadata.Commands[i].Type = command.(map[string]interface{})["type"].(string)

			commandContent := make([]string, len(command.(map[string]interface{})["commands"].([]interface{})))
			for j, command := range command.(map[string]interface{})["commands"].([]interface{}) {
				commandContent[j] = command.(string)
			}
			metadata.Commands[i].Commands = commandContent

			metadata.Commands[i].GOOS = command.(map[string]interface{})["GOOS"].(string)

			if _, ok := command.(map[string]interface{})["GOARCH"]; ok {
				metadata.Commands[i].GOARCH = command.(map[string]interface{})["GOARCH"].(string)
			}
		}
	} else {
		metadata.Commands = make([]CommandStruct, 0)
	}

	if _, ok := metadataMap["confirmation"]; ok {
		metadata.Confirmation = make([]ConfirmationStruct, len(metadataMap["confirmation"].([]interface{})))
		for i, confirmation := range metadataMap["confirmation"].([]interface{}) {
			metadata.Confirmation[i].Type = confirmation.(map[string]interface{})["type"].(string)

			metadata.Confirmation[i].Message = confirmation.(map[string]interface{})["message"].(string)

			if _, ok := confirmation.(map[string]interface{})["GOOS"]; ok {
				metadata.Confirmation[i].GOOS = confirmation.(map[string]interface{})["GOOS"].(string)
			}

			if _, ok := confirmation.(map[string]interface{})["GOARCH"]; ok {
				metadata.Confirmation[i].GOARCH = confirmation.(map[string]interface{})["GOARCH"].(string)
			}
		}
	} else {
		metadata.Confirmation = make([]ConfirmationStruct, 0)
	}

	if _, ok := metadataMap["tool"]; ok {
		metadata.Tool.Name = metadataMap["tool"].(map[string]interface{})["name"].(string)
		metadata.Tool.Description = metadataMap["tool"].(map[string]interface{})["description"].(string)

		metadata.Tool.Entrypoints = make([]ToolEntrypointStruct, len(metadataMap["tool"].(map[string]interface{})["entrypoints"].([]interface{})))
		for i, entrypoint := range metadataMap["tool"].(map[string]interface{})["entrypoints"].([]interface{}) {
			metadata.Tool.Entrypoints[i].Path = entrypoint.(map[string]interface{})["path"].(string)
			metadata.Tool.Entrypoints[i].GOOS = entrypoint.(map[string]interface{})["GOOS"].(string)

			if _, ok := entrypoint.(map[string]interface{})["GOARCH"]; ok {
				metadata.Tool.Entrypoints[i].GOARCH = entrypoint.(map[string]interface{})["GOARCH"].(string)
			}
		}
	}

	return metadata, nil
}

// JSON encodes a Metadata struct into a JSON byte array.
func (metadata Metadata) JSON() ([]byte, error) {
	metadataMap := make(map[string]interface{})

	metadataMap["format_version"] = 1

	metadataMap["tooth"] = metadata.ToothPath

	metadataMap["version"] = metadata.Version.String()

	metadataMap["dependencies"] = make(map[string]interface{})
	for toothPath, versionMatchOuterList := range metadata.Dependencies {
		metadataMap["dependencies"].(map[string]interface{})[toothPath] =
			make([]interface{}, len(versionMatchOuterList))
		for i, versionMatchInnerList := range versionMatchOuterList {
			metadataMap["dependencies"].(map[string]interface{})[toothPath].([]interface{})[i] =
				make([]interface{}, len(versionMatchInnerList))
			for j, versionMatch := range versionMatchInnerList {
				metadataMap["dependencies"].(map[string]interface{})[toothPath].([]interface{})[i].([]interface{})[j] = versionMatch.String()
			}
		}
	}

	metadataMap["information"] = make(map[string]interface{})
	metadataMap["information"].(map[string]interface{})["name"] = metadata.Information.Name
	metadataMap["information"].(map[string]interface{})["description"] = metadata.Information.Description
	metadataMap["information"].(map[string]interface{})["author"] = metadata.Information.Author
	metadataMap["information"].(map[string]interface{})["license"] = metadata.Information.License
	metadataMap["information"].(map[string]interface{})["homepage"] = metadata.Information.Homepage

	metadataMap["placement"] = make([]interface{}, len(metadata.Placement))
	for i, placement := range metadata.Placement {
		metadataMap["placement"].([]interface{})[i] = make(map[string]interface{})
		metadataMap["placement"].([]interface{})[i].(map[string]interface{})["source"] = placement.Source
		metadataMap["placement"].([]interface{})[i].(map[string]interface{})["destination"] = placement.Destination
	}

	metadataMap["possession"] = make([]interface{}, len(metadata.Possession))
	for i, possession := range metadata.Possession {
		metadataMap["possession"].([]interface{})[i] = possession
	}

	metadataMap["commands"] = make([]interface{}, len(metadata.Commands))
	for i, command := range metadata.Commands {
		metadataMap["commands"].([]interface{})[i] = make(map[string]interface{})
		metadataMap["commands"].([]interface{})[i].(map[string]interface{})["type"] = command.Type
		metadataMap["commands"].([]interface{})[i].(map[string]interface{})["commands"] = command.Commands
		metadataMap["commands"].([]interface{})[i].(map[string]interface{})["GOOS"] = command.GOOS
		if command.GOARCH != "" {
			metadataMap["commands"].([]interface{})[i].(map[string]interface{})["GOARCH"] = command.GOARCH
		}
	}

	metadataMap["confirmation"] = make([]interface{}, len(metadata.Confirmation))
	for i, confirmation := range metadata.Confirmation {
		metadataMap["confirmation"].([]interface{})[i] = make(map[string]interface{})
		metadataMap["confirmation"].([]interface{})[i].(map[string]interface{})["type"] = confirmation.Type
		metadataMap["confirmation"].([]interface{})[i].(map[string]interface{})["message"] = confirmation.Message
		if confirmation.GOOS != "" {
			metadataMap["confirmation"].([]interface{})[i].(map[string]interface{})["GOOS"] = confirmation.GOOS
		}
		if confirmation.GOARCH != "" {
			metadataMap["confirmation"].([]interface{})[i].(map[string]interface{})["GOARCH"] = confirmation.GOARCH
		}
	}

	metadataMap["tool"] = make(map[string]interface{})
	metadataMap["tool"].(map[string]interface{})["name"] = metadata.Tool.Name
	metadataMap["tool"].(map[string]interface{})["description"] = metadata.Tool.Description
	metadataMap["tool"].(map[string]interface{})["entrypoints"] = make([]interface{}, len(metadata.Tool.Entrypoints))
	for i, entrypoint := range metadata.Tool.Entrypoints {
		metadataMap["tool"].(map[string]interface{})["entrypoints"].([]interface{})[i] = make(map[string]interface{})
		metadataMap["tool"].(map[string]interface{})["entrypoints"].([]interface{})[i].(map[string]interface{})["path"] = entrypoint.Path
		metadataMap["tool"].(map[string]interface{})["entrypoints"].([]interface{})[i].(map[string]interface{})["GOOS"] = entrypoint.GOOS
		if entrypoint.GOARCH != "" {
			metadataMap["tool"].(map[string]interface{})["entrypoints"].([]interface{})[i].(map[string]interface{})["GOARCH"] = entrypoint.GOARCH
		}
	}

	// Encode metadataMap into JSON
	buf := bytes.NewBuffer([]byte{})
	encoder := json.NewEncoder(buf)

	encoder.SetIndent("", "  ")

	// Prevent HTML escaping. Otherwise, "<", ">", "&", U+2028, and U+2029
	// characters are escaped to "\u003c", "\u003e", "\u0026", "\u2028", and "\u2029".
	encoder.SetEscapeHTML(false)

	err := encoder.Encode(metadataMap)
	if err != nil {
		return nil, errors.New("failed to encode metadata into JSON: " + err.Error())
	}

	return buf.Bytes(), nil
}

// IsTool returns true if the metadata is for a tool.
func (m Metadata) IsTool() bool {
	return m.Tool.Name != ""
}
