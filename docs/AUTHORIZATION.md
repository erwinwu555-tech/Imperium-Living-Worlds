# GitHub 授权

2026-10-18：ChatGPT Codex Connector 已安装到 erwinwu555-tech。安装可见，代码写入实测成功。之前的 403 发生在只有账号授权、没有可用仓库 App 安装的状态。

GitHub App 配置页面显示代码、Actions、issues、PR 和 workflows 的读写权限。仓库保持 public；公开/私有与集成写入权限不是同一设置。

当前安装选择 All repositories；可按需要在 GitHub Settings → Applications → Installed GitHub Apps → Configure 中缩小到本仓库。

CI 的 GITHUB_TOKEN 只需要 contents:read，不需要扩大到 read/write。不使用仓库 Secrets，不上传游戏 DLL 或密钥。
