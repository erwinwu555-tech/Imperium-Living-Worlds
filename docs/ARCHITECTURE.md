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
