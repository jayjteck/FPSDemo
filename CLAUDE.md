# FPS Demo — 项目上下文

> 给 Claude 的跨会话记忆。目标：Unity 客户端实习简历用的 FPS 射击 Demo。

## 关键路径
- Unity 工程：`E:\Unityworkspace\FPSDemo`
- 规划文档：`E:\MyGmaeWorkSpace\FPSDemo\架构建议\`（README / 01_类结构设计 / 02_搭建清单）

## 技术选型（已确定）
- Unity 6 / 2022 LTS，URP 渲染管线
- 新 Input System（用 `.inputactions` 资产，不用旧 Input Manager）
- Cinemachine 2.10.7（vcam 组件名是 `CinemachineVirtualCamera`）
- 敌人导航：NavMesh + NavMeshAgent
- 数据驱动：ScriptableObject

## 已完成
1. `PlayerController.cs`：移动 + 跳跃 + 身体跟随相机水平朝向（`SyncBodyWithCamera`）。视角由 Cinemachine POV 接管，脚本不再手动转相机。
2. `PlayerControls.inputactions`：Move / Look / Jump / Sprint（WASD + 鼠标 + 手柄）。
3. `CinemachineLookInput.cs`：实现 `AxisState.IInputAxisProvider`，把 Look 动作喂给 POV；`sensitivity` 字段（默认 0.05）缩放鼠标像素位移。
4. `CameraShake.cs`：用 `CinemachineImpulseSource.GenerateImpulse` 做相机震动。
5. `WeaponData.cs`（SO）：伤害/射速/弹匣/扩散/后坐力等纯数据，数据驱动（换枪 = 换资产，逻辑不动）。
6. `Weapon.cs`：单把枪逻辑 —— 开火（hitscan 射线）、换弹（计时器状态机，非协程）、扩散（`Random.insideUnitCircle` 圆内随机）、后坐力（调 `CameraShake.Shake`）。
7. `WeaponManager.cs`：多枪持有/切换 + 转发 Fire/Reload 输入；切枪后调 `RefreshAmmo` 通过事件中心广播弹药。
8. `WeaponState.cs`：Idle / Reloading / Equipping 枚举。
9. `IDamageable.cs`（Systems）：统一伤害接口，命中后统一调 `TakeDamage`，不区分玩家/敌人。
10. `SimpleObjectPool.cs` + `PooledEffect.cs`（Core）：对象池，特效 `SetActive` 复用 + 自动归还，不运行时 Instantiate/Destroy。
11. `PlayerControls.inputactions`：新增 Fire（左键/右扳机）、Reload（R/手柄 X）。
12. `PlayerHealth.cs`：玩家血量，实现 `IDamageable`；受击/死亡通过事件中心广播。
13. `EnemyData.cs`（SO）+ `EnemyState.cs`：敌人数据 + 状态枚举（Idle/Patrol/Chase/Attack/Dead）。
14. `EnemyAI.cs`：状态机 + NavMesh 导航；`CanSeePlayer` 三层判断（距离 + 视野角 + 射线遮挡）。
15. `EnemyHealth.cs`：敌人血量，实现 `IDamageable`。
16. `Core/Event/` 事件中心：`EventCenter`(单例) + `E_EventType` + `EventInfo` + `EventParams`；业务事件已从各自 `event` 迁移到事件中心（`AddEventListener` / `EventTrigger`）。
17. `HUD.cs`（UI）：订阅事件中心显示血量/弹药/击杀数/死亡提示（血条 + TMP 文本）。
18. `EnemyHealthBar.cs` + `EnemyHealthText.cs`（Enemy）：敌人头顶血条（图形条 / 3D 文字两种方案），billboard 面向相机，直接引用 `EnemyHealth`（局部一对一 UI，不走事件中心）。

## 场景结构约定
- Player（CharacterController + PlayerController + CameraShake + PlayerHealth + WeaponManager）
  - CameraRoot（空物体，位置 0, 1.6, 0）
- Main Camera（挂 CinemachineBrain，带 MainCamera 标签）
  - 枪（挂 `Weapon` 组件，作为相机子物体；挂 CameraRoot 下会因 Cinemachine 一帧延迟而抖动）
- CM vcam（CinemachineVirtualCamera：Follow=CameraRoot，**Body=Framing Transposer**，Aim=POV，**Accel/Decel Time=0**）
- 敌人（NavMeshAgent + CapsuleCollider + EnemyHealth + EnemyAI，都挂根物体）
  - 头顶血条（World Space Canvas + 纯色 Image Filled，挂 `EnemyHealthBar`；或 3D TMP 挂 `EnemyHealthText`）
- Canvas（挂 `HUD` 脚本 + 血条/弹药/击杀 TMP 文本 + 中心准星 `+`）

## 关键坑（已踩过，别重犯）
- vcam Body 不能用 `Hard Lock to Target`（会锁旋转，和身体跟随形成死循环），要用 `Framing Transposer`。
- 鼠标 `delta` 是像素值，必须乘 `sensitivity` 缩放，否则过灵敏。
- POV 的 Accel/Decel Time 设 0 才跟手、有"稳重感"。
- Cinemachine 2.x 接口是 `AxisState.IInputAxisProvider`（不是 3.x 的 `Cinemachine.IInputAxisProvider`）。
- FPS 枪要挂在 Main Camera 下，别挂 CameraRoot/Player 下（否则和 Cinemachine 差一帧会抖动）。
- 事件中心是全局单例，会持有所有订阅者引用；会销毁的对象（敌人）必须在 `OnDestroy` 里 `RemoveEventListener`，否则内存泄漏。
- NavMesh 未烘焙时，`SetDestination` / `isStopped` 都报 "can only be called on an active agent that has been placed on a NavMesh"；导航操作前要检查 `_agent.isOnNavMesh`。
- `WeaponData` 的 spread 是「偏移向量分量」（不是角度），0.5 ≈ 26° 太大打不中，合理值 0.02（≈1°）左右。
- Image 的 `Filled` 模式不支持 Sliced（九宫格），圆角 sprite 会被拉伸；血条用纯色矩形（Source Image 清空，或纯白 sprite + `Color` 调色）。

## 下一步
- NavMesh 烘焙验证：烘焙后敌人能追击移动（现在只靠 `isOnNavMesh` 防御，没烘焙不报错但敌人不会动）。
- UI 收尾：准星、hitmarker（命中变红，需武器广播 `OnHit` 事件）、血条引用验证。
- GameManager：游戏状态（菜单/游戏中/暂停/结束）、计分、重开（订阅 `OnEnemyDied` / `OnPlayerDied`）。
- 打磨：对象池预热 `Prewarm`、命中反馈、切枪动画（`WeaponState.Equipping`）。
