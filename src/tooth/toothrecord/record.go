// Package record provides functions to manage the record files.
package toothrecord

import (
	"bytes"
	"encoding/json"
	"errors"
	"os"
	"path/filepath"
	"sort"
	"strings"

	localfile "github.com/liteldev/lip/localfile"
	metadatautils "github.com/liteldev/lip/tooth/toothmetadata"
	versionutils "github.com/liteldev/lip/utils/version"
	versionmatchutils "github.com/liteldev/lip/utils/version/versionmatch"
)

// infoStruct is the struct that contains the information of a tooth.
type InfoStruct struct {
	Name        string
	Description string
	Author      string
	License     string
	Homepage    string
}

// placementStruct is the struct that contains the source and destination of a placement.
type PlacementStruct struct {
	Source      string
	Destination string
}

// Record is the struct that contains the record of a tooth installation.
type Record struct {
	ToothPath           string
	Version             versionutils.Version
	Dependencies        map[string]([][]versionmatchutils.VersionMatch)
	Information         InfoStruct
	Placement           []PlacementStruct
	IsManuallyInstalled bool
}

// New creates a new Record struct from a record file.
func New(recordFilePath string) (Record, error) {
	content, err := os.ReadFile(recordFilePath)
	if err != nil {
		return Record{}, errors.New("cannot read the record file " + recordFilePath + ": " + err.Error())
	}

	// Parse the record file.
	currentRecord, err := NewFromJSON(content)
	if err != nil {
		return Record{}, errors.New(err.Error())
	}

	return currentRecord, nil
}

// NewFromJSON decodes a JSON byte array into a Record struct.
func NewFromJSON(jsonData []byte) (Record, error) {
	// Read to a map.
	var recordMap map[string]interface{}
	err := json.Unmarshal(jsonData, &recordMap)
	if err != nil {
		return Record{}, errors.New("failed to decode JSON into record: " + err.Error())
	}

	// Parse to record.
	var record Record

	record.ToothPath = recordMap["tooth"].(string)

	version, err := versionutils.NewFromString(recordMap["version"].(string))

	if err != nil {
		return Record{}, errors.New("failed to decode JSON into record: " + err.Error())
	}
	record.Version = version

	record.Dependencies = make(map[string]([][]versionmatchutils.VersionMatch))
	for toothPath, versionMatchOuterList := range recordMap["dependencies"].(map[string]interface{}) {
		record.Dependencies[toothPath] = make([][]versionmatchutils.VersionMatch, len(versionMatchOuterList.([]interface{})))
		for i, versionMatchInnerList := range versionMatchOuterList.([]interface{}) {
			record.Dependencies[toothPath][i] = make([]versionmatchutils.VersionMatch, len(versionMatchInnerList.([]interface{})))
			for j, versionMatch := range versionMatchInnerList.([]interface{}) {
				versionMatch, err := versionmatchutils.NewFromString(versionMatch.(string))
				if err != nil {
					return Record{}, errors.New("failed to decode JSON into record: " + err.Error())
				}

				record.Dependencies[toothPath][i][j] = versionMatch
			}
		}
	}

	record.Information.Name = recordMap["information"].(map[string]interface{})["name"].(string)
	record.Information.Description = recordMap["information"].(map[string]interface{})["description"].(string)
	record.Information.Author = recordMap["information"].(map[string]interface{})["author"].(string)
	record.Information.License = recordMap["information"].(map[string]interface{})["license"].(string)
	record.Information.Homepage = recordMap["information"].(map[string]interface{})["homepage"].(string)

	record.Placement = make([]PlacementStruct, len(recordMap["placement"].([]interface{})))
	for i, placement := range recordMap["placement"].([]interface{}) {
		record.Placement[i].Source = placement.(map[string]interface{})["source"].(string)
		record.Placement[i].Destination = placement.(map[string]interface{})["destination"].(string)
	}

	record.IsManuallyInstalled = recordMap["is_manually_installed"].(bool)

	return record, nil
}

// NewFromMetadata creates a new record from a tooth metadata.
func NewFromMetadata(metadata metadatautils.Metadata) Record {
	record := Record{}

	record.ToothPath = metadata.ToothPath

	record.Version = metadata.Version

	record.Dependencies = metadata.Dependencies

	record.Information.Name = metadata.Information.Name
	record.Information.Description = metadata.Information.Description
	record.Information.Author = metadata.Information.Author
	record.Information.License = metadata.Information.License
	record.Information.Homepage = metadata.Information.Homepage

	record.Placement = make([]PlacementStruct, len(metadata.Placement))
	for i, placement := range metadata.Placement {
		record.Placement[i].Source = placement.Source
		record.Placement[i].Destination = placement.Destination
	}

	record.IsManuallyInstalled = false

	return record
}

// JSON encodes a Record struct into a JSON byte array.
func (record Record) JSON() ([]byte, error) {
	recordMap := make(map[string]interface{})

	recordMap["tooth"] = record.ToothPath

	recordMap["version"] = record.Version.String()

	recordMap["dependencies"] = make(map[string]interface{})
	for toothPath, versionMatchOuterList := range record.Dependencies {
		recordMap["dependencies"].(map[string]interface{})[toothPath] =
			make([]interface{}, len(versionMatchOuterList))
		for i, versionMatchInnerList := range versionMatchOuterList {
			recordMap["dependencies"].(map[string]interface{})[toothPath].([]interface{})[i] =
				make([]interface{}, len(versionMatchInnerList))
			for j, versionMatch := range versionMatchInnerList {
				recordMap["dependencies"].(map[string]interface{})[toothPath].([]interface{})[i].([]interface{})[j] = versionMatch.String()
			}
		}
	}

	recordMap["information"] = make(map[string]interface{})
	recordMap["information"].(map[string]interface{})["name"] = record.Information.Name
	recordMap["information"].(map[string]interface{})["description"] = record.Information.Description
	recordMap["information"].(map[string]interface{})["author"] = record.Information.Author
	recordMap["information"].(map[string]interface{})["license"] = record.Information.License
	recordMap["information"].(map[string]interface{})["homepage"] = record.Information.Homepage

	recordMap["placement"] = make([]interface{}, len(record.Placement))
	for i, placement := range record.Placement {
		recordMap["placement"].([]interface{})[i] = make(map[string]interface{})
		recordMap["placement"].([]interface{})[i].(map[string]interface{})["source"] = placement.Source
		recordMap["placement"].([]interface{})[i].(map[string]interface{})["destination"] = placement.Destination
	}

	recordMap["is_manually_installed"] = record.IsManuallyInstalled

	// Encode recordMap into JSON
	buf := bytes.NewBuffer([]byte{})
	encoder := json.NewEncoder(buf)

	encoder.SetIndent("", "  ")

	// Prevent HTML escaping. Otherwise, "<", ">", "&", U+2028, and U+2029
	// characters are escaped to "\u003c", "\u003e", "\u0026", "\u2028", and "\u2029".
	encoder.SetEscapeHTML(false)

	err := encoder.Encode(recordMap)
	if err != nil {
		return nil, errors.New("failed to encode record into JSON: " + err.Error())
	}

	return buf.Bytes(), nil
}

// ListAll lists all installed tooth records.
func ListAll() ([]Record, error) {
	recordList := make([]Record, 0)

	// Get all record paths
	recordDir, err := localfile.RecordDir()
	if err != nil {
		return nil, errors.New("failed to get record directory: " + err.Error())
	}

	files, err := os.ReadDir(recordDir)
	if err != nil {
		return nil, errors.New("failed to read record directory: " + err.Error())
	}

	for _, file := range files {
		recordFilePath := filepath.Join(recordDir, file.Name())

		// Read record
		record, err := New(recordFilePath)
		if err != nil {
			return nil, errors.New("failed to read record file" + file.Name() + ": " + err.Error())
		}

		recordList = append(recordList, record)
	}

	// Sort record list by tooth path in a case-insensitive order.
	sort.Slice(recordList, func(i, j int) bool {
		return strings.ToLower(recordList[i].ToothPath) < strings.ToLower(recordList[j].ToothPath)
	})

	return recordList, nil
}
