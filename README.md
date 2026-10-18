# Imperium: Living Worlds

RimWorld 1.7 城市自治 Mod 的开发骨架。**当前不是完整玩法版本。**

本阶段：Mod 启动入口、独立 C# 建设队列原型、审批规则测试、自动构建和打包。
普通建筑允许自治；城区扩张与重大拆迁必须审批。队列尚未连接游戏地图，不会自行施工。

## Build

安装 .NET 10 SDK 和 Python 3。仓库固定使用 `Krafs.Rimworld.Ref 1.7.4979-alpha`（net10.0），不上传正版游戏 DLL，不使用 1.6 的 net472 模板。

```sh
dotnet build Source/Imperium.LivingWorlds.csproj -c Release
dotnet run --project Tests/Imperium.Core.Tests/Imperium.Core.Tests.csproj -c Release
python Scripts/package.py
```

GitHub Actions 在 main 推送、PR 或手动运行时构建、测试并生成开发版 ZIP。Actions 页面打开成功的运行，在 Artifacts 下载包，再解压内层开发包，将 `Imperium-Living-Worlds` 放入本地 Mods 目录。启用 Core 和本 Mod 后检查日志中的 `[Imperium: Living Worlds]` 启动信息。

编译通过不代表游戏内验证通过；测试时使用新存档。当前没有持久化世界数据，也没有总督 UI、城市生成、真实物流、军事、附庸城市或云端 AI。Odyssey 与 Harmony 集成将在需要时增加，不作为本骨架前置。

## Roadmap

1. 启动与纯逻辑规则测试（本骨架）。
2. 城市注册、稳定 ID、版本化存档及读写测试。
3. 总督需求评分、建设提案与玩家审批 UI。
4. 地图建设执行、资源预留、失败重试与撤销。
5. 真实物流、多城市分时模拟及性能预算。
6. 势力、军事与可选 AI 策略提供器。

## References

- https://www.nuget.org/packages/Krafs.Rimworld.Ref/1.7.4979-alpha
- https://github.com/krafs/RimRef

项目公开不自动等于允许任意再分发；正式许可证待项目所有者决定。

## Windows 纯净版测试

1. Steam 库右键 RimWorld → 管理 → 浏览本地文件，确认游戏显示 1.7；此开发包针对 1.7.4979 参考版本。
2. 将开发 ZIP 解压，把包含 About 和 Assemblies 的 Imperium-Living-Worlds 文件夹放入游戏 Mods 目录，避免双重嵌套。
3. 首次仅启用 Core 和本 Mod，关闭其他 Mod 与可选 DLC，重启游戏。
4. 在开发者模式日志中检查 `[Imperium: Living Worlds] Development scaffold loaded (0.1.0)`，确认无本 Mod 红色错误。
5. 建立新测试殖民地，保存、退出并重载。当前不会出现城市管理或施工 UI，这是预期。
6. 单独记录游戏版本、Commit、完整错误与复现步骤，再进行 Odyssey 单独启用测试。勿用正式存档试验。

项目模块职责见 docs/ARCHITECTURE.md，阶段验收见 docs/ROADMAP.md。
