# FPS Demo — 项目上下文

> 给 Claude 的跨会话记忆。目标：Unity 客户端实习简历用的 FPS 射击 Demo。

## 关键路径
- Unity 工程：`E:\Unityworkspace\FPSDemo`
- 规划文档：`E:\MyGmaeWorkSpace\FPSDemo\架构建议\`（README / 01_类结构设计 / 02_搭建清单）

## 技术选型（已确定）
- Unity 6 / 2022 LTS，URP 渲染管线
- 新 Input System（用 `.inputactions` 资产，不用旧 Input Manager）
- Cinemachine 2.10.7（vcam 组件名是 `CinemachineVirtualCamera`）
- 敌人导航（后续）：NavMesh + NavMeshAgent
- 数据驱动（后续）：ScriptableObject

## 已完成
1. `PlayerController.cs`：移动 + 跳跃 + 身体跟随相机水平朝向（`SyncBodyWithCamera`）。视角由 Cinemachine POV 接管，脚本不再手动转相机。
2. `PlayerControls.inputactions`：Move / Look / Jump / Sprint（WASD + 鼠标 + 手柄）。
3. `CinemachineLookInput.cs`：实现 `AxisState.IInputAxisProvider`，把 Look 动作喂给 POV；`sensitivity` 字段（默认 0.05）缩放鼠标像素位移。
4. `CameraShake.cs`：用 `CinemachineImpulseSource.GenerateImpulse` 做相机震动。

## 场景结构约定
- Player（CharacterController + PlayerController + CameraShake）
  - CameraRoot（空物体，位置 0, 1.6, 0）
- Main Camera（挂 CinemachineBrain，带 MainCamera 标签）
- CM vcam（CinemachineVirtualCamera：Follow=CameraRoot，**Body=Framing Transposer**，Aim=POV，**Accel/Decel Time=0**）

## 关键坑（已踩过，别重犯）
- vcam Body 不能用 `Hard Lock to Target`（会锁旋转，和身体跟随形成死循环），要用 `Framing Transposer`。
- 鼠标 `delta` 是像素值，必须乘 `sensitivity` 缩放，否则过灵敏。
- POV 的 Accel/Decel Time 设 0 才跟手、有"稳重感"。
- Cinemachine 2.x 接口是 `AxisState.IInputAxisProvider`（不是 3.x 的 `Cinemachine.IInputAxisProvider`）。

## 下一步
- 做武器系统：`WeaponData`(SO) + `Weapon`(开火/换弹) + `WeaponManager`。
- Fire / Reload 动作要加进 `PlayerControls.inputactions`。
- 开火时调 `CameraShake.Shake()` 做后坐力。
