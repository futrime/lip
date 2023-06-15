package versionmatches

import (
	"fmt"
	"strings"

	"github.com/lippkg/lip/pkg/versions"
)

type Group struct {
	items [][]Item
}

// NewGroup creates a new version match group.
func NewGroup(items [][]Item) Group {
	return Group{
		items: items,
	}
}

// NewGroupFromString creates a new version match group from a string.
// ">1.0.0 <=2.0.0 || >3.0.0 <4.0.0" means "1.0.0 < version <= 2.0.0 or
// 3.0.0 < version < 4.0.0".
func NewGroupFromString(versionMatchGroupString string) (Group, error) {
	items := make([][]Item, 0)

	subGroupStringList := strings.Split(versionMatchGroupString, "||")
	for _, subGroupString := range subGroupStringList {
		subGroupString = strings.TrimSpace(subGroupString)

		// Trim all spaces more than one.
		for strings.Contains(subGroupString, "  ") {
			subGroupString = strings.ReplaceAll(subGroupString, "  ", " ")
		}

		subGroupItems := make([]Item, 0)
		itemStringList := strings.Split(subGroupString, " ")
		for _, itemString := range itemStringList {
			item, err := NewItemFromString(itemString)
			if err != nil {
				return Group{}, fmt.Errorf("cannot create item from string: %w", err)
			}

			subGroupItems = append(subGroupItems, item)
		}

		items = append(items, subGroupItems)
	}

	return NewGroup(items), nil
}

// Match checks if the version matches the group.
func (g Group) Match(version versions.Version) bool {
	for _, subGroup := range g.items {
		isMatched := true
		for _, item := range subGroup {
			if !item.Match(version) {
				isMatched = false
				break
			}
		}

		if isMatched {
			return true
		}
	}

	return false
}

// String returns the string representation of the group.
func (g Group) String() string {
	subGroupStrings := make([]string, 0)
	for _, subGroup := range g.items {
		itemStrings := make([]string, 0)
		for _, item := range subGroup {
			itemStrings = append(itemStrings, item.String())
		}

		subGroupStrings = append(subGroupStrings, strings.Join(itemStrings, " "))
	}

	return strings.Join(subGroupStrings, " || ")
}
