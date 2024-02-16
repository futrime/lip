package context

type Config struct {
	GitHubMirrorURL  string `json:"github_mirror_url"`
	GoModuleProxyURL string `json:"go_module_proxy_url"`
	Socks5ProxyURL   string `json:""`
	HttpProxyURL     string `json:""`
	HttpsProxyURL    string `json:""`
}
