package teeth

import (
	"encoding/json"
	"fmt"
	"strings"

	"github.com/xeipuuv/gojsonschema"
)

type RawMetadata struct {
	FormatVersion string          `json:"format_version"`
	Tooth         string          `json:"tooth"`
	Version       string          `json:"version"`
	Info          RawMetadataInfo `json:"info"`

	Commands     RawMetadataCommands `json:"commands"`
	Dependencies map[string]string   `json:"dependencies"`
	Files        RawMetadataFiles    `json:"files"`

	Platforms []RawMetadataPlatformsItem `json:"platforms"`
}

type RawMetadataInfo struct {
	Name        string `json:"name"`
	Description string `json:"description"`
	Author      string `json:"author"`
}

type RawMetadataCommands struct {
	PreInstall    []string `json:"pre_install"`
	PostInstall   []string `json:"post_install"`
	PreUninstall  []string `json:"pre_uninstall"`
	PostUninstall []string `json:"post_uninstall"`
}

type RawMetadataFiles struct {
	Place    []RawMetadataFilesPlaceItem `json:"place"`
	Preserve []string                    `json:"preserve"`
}

type RawMetadataFilesPlaceItem struct {
	Src  string `json:"src"`
	Dest string `json:"dest"`
}

type RawMetadataPlatformsItem struct {
	GOARCH string `json:"goarch"`
	GOOS   string `json:"goos"`

	Commands     RawMetadataCommands `json:"commands"`
	Dependencies map[string]string   `json:"dependencies"`
	Files        RawMetadataFiles    `json:"files"`
}

const rawMetadataJSONSchema = `
{
	"$schema": "http://json-schema.org/draft-07/schema#",
	"type": "object",
	"properties": {
		"format_version": {
			"type": "integer",
			"const": 2
		},
		"tooth": {
			"type": "string"
		},
		"version": {
			"type": "string"
		},
		"info": {
			"type": "object",
			"properties": {
				"name": {
					"type": "string"
				},
				"description": {
					"type": "string"
				},
				"author": {
					"type": "string"
				}
			},
			"required": [
				"name",
				"description",
				"author"
			]
		},
		"commands": {
			"type": "object",
			"properties": {
				"pre_install": {
					"type": "array",
					"items": {
						"type": "string"
					}
				},
				"post_install": {
					"type": "array",
					"items": {
						"type": "string"
					}
				},
				"pre_uninstall": {
					"type": "array",
					"items": {
						"type": "string"
					}
				},
				"post_uninstall": {
					"type": "array",
					"items": {
						"type": "string"
					}
				}
			}
		},
		"dependencies": {
			"type": "object",
			"patternProperties": {
				"^.*$": {
					"type": "string"
				}
			}
		},
		"files": {
			"type": "object",
			"properties": {
				"place": {
					"type": "array",
					"items": {
						"type": "object",
						"properties": {
							"src": {
								"type": "string"
							},
							"dest": {
								"type": "string"
							}
						},
						"required": [
							"src",
							"dest"
						]
					}
				},
				"preserve": {
					"type": "array",
					"items": {
						"type": "string"
					}
				}
			}
		},
		"platforms": {
			"type": "array",
			"items": {
				"type": "object",
				"properties": {
					"goarch": {
						"type": "string"
					},
					"goos": {
						"type": "string"
					},
					"commands": {
						"type": "object",
						"properties": {
							"pre_install": {
								"type": "array",
								"items": {
									"type": "string"
								}
							},
							"post_install": {
								"type": "array",
								"items": {
									"type": "string"
								}
							},
							"pre_uninstall": {
								"type": "array",
								"items": {
									"type": "string"
								}
							},
							"post_uninstall": {
								"type": "array",
								"items": {
									"type": "string"
								}
							}
						}
					},
					"dependencies": {
						"type": "object",
						"patternProperties": {
							"^.*$": {
								"type": "string"
							}
						}
					},
					"files": {
						"type": "object",
						"properties": {
							"place": {
								"type": "array",
								"items": {
									"type": "object",
									"properties": {
										"src": {
											"type": "string"
										},
										"dest": {
											"type": "string"
										}
									},
									"required": [
										"src",
										"dest"
									]
								}
							},
							"preserve": {
								"type": "array",
								"items": {
									"type": "string"
								}
							}
						}
					}
				},
				"required": [
					"goos"
				]
			}
		}
	},
	"required": [
		"format_version",
		"tooth",
		"version",
		"info"
	]
}
`

func NewRawMetadata(jsonBytes []byte) (RawMetadata, error) {
	var err error

	// Validate JSON against schema
	schemaLoader := gojsonschema.NewStringLoader(rawMetadataJSONSchema)
	documentLoader := gojsonschema.NewStringLoader(string(jsonBytes))

	result, err := gojsonschema.Validate(schemaLoader, documentLoader)
	if err != nil {
		return RawMetadata{}, fmt.Errorf("failed to validate raw metadata: %w", err)
	}

	if !result.Valid() {
		var errors []string
		for _, err := range result.Errors() {
			errors = append(errors, err.String())
		}
		return RawMetadata{}, fmt.Errorf("raw metadata is invalid: %v", strings.Join(errors, ", "))
	}

	// Unmarshal JSON
	var rawMetadata RawMetadata
	err = json.Unmarshal(jsonBytes, &rawMetadata)
	if err != nil {
		return RawMetadata{}, fmt.Errorf("failed to unmarshal raw metadata: %w", err)
	}

	return rawMetadata, nil
}

func (m RawMetadata) JSON() ([]byte, error) {
	jsonBytes, err := json.Marshal(m)
	if err != nil {
		return nil, fmt.Errorf("failed to marshal raw metadata: %w", err)
	}

	return jsonBytes, nil
}
