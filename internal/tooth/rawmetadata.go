package tooth

// Why to split Metadata and RawMetadata? Because we encounter a problem when
// we want to add a getter with the same name as a field.
type RawMetadata struct {
	FormatVersion int             `json:"format_version"`
	Tooth         string          `json:"tooth"`
	Version       string          `json:"version"`
	Info          RawMetadataInfo `json:"info"`

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

	Commands      RawMetadataCommands `json:"commands,omitempty"`
	Dependencies  map[string]string   `json:"dependencies,omitempty"`
	Prerequisites map[string]string   `json:"prerequisites,omitempty"`
	Files         RawMetadataFiles    `json:"files,omitempty"`
}
