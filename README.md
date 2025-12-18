# FinalPK - Frame Sync 1v1 Shooter

![Unity Version](https://img.shields.io/badge/Unity-2022-blue.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)
![状态](https://img.shields.io/badge/Status-Prototype-orange.svg)

**FinalPK** 是一个基于 **Unity 2022** 开发的 1v1 帧同步（Lockstep）射击对战游戏客户端。
本项目演示了基于帧同步技术的确定性逻辑实现，采用 C# 控制台应用作为转发服务端（服务端代码独立，不包含在本仓库中），通过 Protobuf 进行高效的消息序列化。

---

## 🎮 游戏简介 (Introduction)

游戏设定在一个封闭的决斗场景中，两名玩家分别扮演 **Hunter (猎人)** 和 **Runner (逃亡者)** 进行 1v1 对抗。

- **Hunter**: 手持高杀伤力武器，目标是击穿掩体消灭对手。
- **Runner**: 处于被动防御状态，需要利用掩体和走位躲避致命攻击。

游戏采用**回合轮换制**，当 Hunter 耗尽弹药后，双方身份互换，直到一方生命值归零。

---

## 🕹️ 玩法说明 (Gameplay)

### 1. 场景与角色生成
进入游戏后，系统根据玩家 ID 自动分配出生点：
- **BornPos[0]**: 生成 **Hunter**。
- **BornPos[1]**: 生成 **Runner**。
- **掩体**: Runner 面前数米处有一道墙壁，用于阻挡视线和部分攻击。

### 2. 核心战斗逻辑
*   **Hunter (进攻方)**:
    *   拥有 **3 发子弹**。
    *   **瞄准**: 渲染一条红色的激光辅助线，该激光视觉上**穿透墙壁**，用于辅助预判。
    *   **射击**: 开枪后子弹实体会**穿透并破坏墙壁**（墙壁碎裂或消失），直接打击后方的 Runner。
    *   **弹药耗尽**: 打完 3 发子弹后触发**换边 (Role Swap)**。

*   **Runner (防守方)**:
    *   初始位于墙壁后方。
    *   需要通过输入（移动/冲刺）在有限空间内进行战术规避。

*   **胜负判定**:
    *   **换边机制**: 若 Hunter 三发子弹打完未分胜负，双方角色互换（Hunter ↔ Runner），位置重置，比赛继续。
    *   **游戏结束**: 任意一方生命值归零，游戏结束。

### 3. 游戏 UI
- **左侧**: 本机玩家血条 (Local HP)。
- **右侧**: 远端玩家血条 (Remote HP)。
- **中间**: Hunter 当前剩余子弹数量 (Ammo Count)。

---

## 🏗️ 技术架构 (Architecture)

本项目采用经典的 **帧同步 (Frame Synchronization)** 架构，逻辑层与表现层分离。

### 1. 客户端架构 (Client - Unity 2022)
客户端仅包含游戏逻辑与渲染，核心特点如下：
*   **确定性逻辑 (Deterministic Logic)**: 
    *   所有游戏逻辑（移动、碰撞、扣血、墙壁破坏）均在本地计算。
    *   **输入驱动**: 客户端不直接发送位置，而是发送“输入指令”（如：按下左移、按下开火）。
    *   **随机数同步**: 使用自定义的确定性随机数生成器，确保两端随机结果一致。
*   **表现与逻辑分离**:
    *   **逻辑层**: 处理子弹碰撞检测、数值计算、墙壁状态同步。
    *   **表现层**: 处理激光渲染、墙壁破碎特效、UI 更新。
*   **网络消息**: 使用 **Protobuf** 生成协议类，保证消息包体最小化且解析高效。

### 2. 服务端架构 (Server - C# Console)
*(注：服务端代码不包含在本仓库中，此处仅作架构说明)*
*   **纯转发模型 (Relay Server)**:
    *   服务端仅负责接收两个客户端的帧输入消息。
    *   服务端不运行游戏逻辑（如物理碰撞），只负责定帧（Lockstep）并将收集到的输入广播给所有客户端。
    *   逻辑简单高效，基于 C# 控制台应用程序开发。

### 3. 关键流程
1.  **连接阶段**: 客户端连接 C# 控制台服务端。
2.  **同步阶段**: 
    *   客户端采集玩家 Input。
    *   发送 Input 到服务端。
    *   接收服务端广播的 `ServerFrame`（包含所有玩家输入）。
    *   客户端执行该帧逻辑（Update Logic）。
3.  **换边逻辑**:
    *   监听 `AmmoCount`，归零时客户端逻辑层触发 `SwapRole` 事件。
    *   重置 BornPos 和角色状态，无需重新加载场景。

---

## 📂 目录结构 (Directory Structure)

```text
Assets/
├── Scripts/
│   ├── Core/           # 帧同步核心 (Lockstep, InputManager)
│   ├── Network/        # 网络层 (Socket, Protobuf Handler)
│   ├── Logic/          # 游戏逻辑 (Player, Bullet, Wall)
│   ├── View/           # 表现层 (UI, Effects)
│   └── Utils/          # 工具类
├── Scenes/             # 游戏场景
├── Prefabs/            # 角色与物体预制体
└── Plugins/            # Protobuf 库文件
