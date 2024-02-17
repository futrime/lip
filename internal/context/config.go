package context

type Config struct {
	GitHubMirrorURL  string `json:"github_mirror_url"`
	GoModuleProxyURL string `json:"go_module_proxy_url"`
	ProxyURL         string `json:"proxy_url"`
}
