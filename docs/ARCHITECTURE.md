# Architecture

## 当前最小实现

`Source/Imperium.LivingWorlds.csproj` 是游戏适配层，只编译 `Compatibility/ImperiumMod.cs`，引用纯逻辑 Core。启动时输出版本日志，不执行地图修改。
`Source/Core/Imperium.Core.csproj` 不引用游戏，编译建设审批队列；测试通过可执行的行为测试运行，断言失败返回非零码。

## 边界与数据流

总督需求 → 计划提案 → 玩家审批/政策 → 资源预留 → 建设执行 → 结果回写。普通建筑可进入 Ready；扩张和重大拆迁停留在 AwaitingApproval。云端模型只提出候选计划，不能绕过审批或直接调用游戏 API。当前只有审批队列原型。

加载地图使用游戏实体为真源；未加载城市使用独立摘要账本。地图切换通过稳定 ID、单一所有权和结算时间戳防止重复生产/库存复制。资源在物流任务中预留，到货才入账。地图实体操作仅在游戏线程；云端请求异步且有超时、本地回退。

存档后续增加 schemaVersion、迁移器和缺失 Def 防护。每阶段设置城市数、居民数、模拟间隔与耗时预算，先测量再优化。不能以合并程序集或减少 Mod 数量代替性能验证。

## 编译依据

固定 `Krafs.Rimworld.Ref 1.7.4979-alpha`；2026-10-18 核实该具体包目标为 net10.0，页面注明获 Ludeon 授权发布参考程序集。不同 1.7 实验版本可能需要重新验证。
https://www.nuget.org/packages/Krafs.Rimworld.Ref/1.7.4979-alpha

只把我们生成的两份 DLL 打入 ZIP，不分发游戏/参考 DLL。当前不依赖 Harmony、Odyssey 或云端服务。

## M1 城市记录与存档适配（待游戏验证）

`Source/Core/CityRegistry.cs` 保存稳定城市 GUID、世界格编号、名称和建城 tick，拒绝重复编号与重复地块。`Source/Compatibility/ImperiumWorldComponent.cs` 负责世界存档的 `ExposeData`，将游戏 Scribe 对象和纯 C# 模型分离。新存档 schema 为 1；此前无城市数据的空存档可迁移。**未知版本不可保证原样保存：遇到加载错误务必退出且不要覆盖旧存档。** 详见 `docs/CITY_PERSISTENCE.md`。

注册表按地块建立索引，`FindByTile` 为 O(1)，不逐 tick 扫描城市。`Source/Compatibility/ImperiumDebugActions.cs` 提供仅开发者模式可见的验证命令：在当前地图所在地块登记城市、把注册表写入日志；读档时也逐城输出同格式日志，供保存前后逐字比对。

上述接口尚未由实际游戏测试验证。此阶段没有玩家可用的城市创建 UI、世界生成或建设执行。世界格目前以整数保存，尚未区分 1.6+ 的星球图层（Odyssey 轨道层），后续需改为图层感知 ID。
