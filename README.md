这是一个为你生成的 `README.md` 文件。我已经为你组织好了内容，包括游戏介绍、玩法说明、以及基于你描述的“帧同步”和“无服务端逻辑”特点进行的架构分析。

你可以直接复制下面的内容到你项目的根目录下。

***

# FinalPK - Frame Sync 1v1 Shooter

![Unity Version](https://img.shields.io/badge/Unity-2022-blue.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)
![状态](https://img.shields.io/badge/Status-Prototype-orange.svg)

**FinalPK** 是一个基于 **Unity 2022** 开发的 1v1 帧同步（Lockstep）射击对战游戏。
本项目演示了如何在没有权威服务器（Server-Authoritative）逻辑的情况下，仅通过客户端之间的输入同步（Input Synchronization）来实现确定性的多人对战体验。

---

## 🎮 游戏简介 (Introduction)

在 **FinalPK** 中，两名玩家被分配到通过帧同步连接的虚拟战场中。游戏采用非对称竞技机制，一局游戏中包含攻守转换。

- **Hunter (猎人)**: 手持武器，拥有有限的弹药，目标是击杀 Runner。
- **Runner (逃亡者)**: 需要利用掩体（墙壁）进行躲避，并在枪林弹雨中生存。

### 核心机制
- **双人联机**：基于帧同步技术，保证左右两端客户端逻辑完全一致。
- **攻守互换**：Hunter 打完 3 发子弹后，双方角色立即互换，体验不同的对抗乐趣。
- **环境破坏**：Hunter 的攻击可以破坏掩体墙壁，子弹具有穿透效果。

---

## 🕹️ 玩法说明 (Gameplay)

### 角色与生成
- **Hunter**: 生成于 `BornPos[0]`。
- **Runner**: 生成于 `BornPos[1]`，面前有一堵墙壁作为掩体。

### 操作与规则
1.  **瞄准与射击 (Hunter)**:
    -   Hunter 拥有 **3发子弹**。
    -   **激光瞄准**：Hunter 瞄准时会渲染一条可穿透墙壁的激光线，辅助判断弹道。
    -   **开火**：射击会直接破坏墙壁（子弹穿透）并打击 Runner。
2.  **躲避 (Runner)**:
    -   Runner 位于墙后，需通过输入指令进行左右移动或动作转换，预判 Hunter 的射击并躲避。
3.  **UI 界面**:
    -   **左侧血条**: 本机玩家生命值。
    -   **右侧血条**: 远端（Remote）玩家生命值。
    -   **中间数字**: Hunter 当前剩余子弹数。
4.  **胜负判定**:
    -   任意一方生命值归零，游戏结束。
    -   若 Hunter 耗尽 3 发子弹仍未结束游戏，双方角色互换（Hunter 变 Runner，Runner 变 Hunter），战斗继续。

---

## 🏗️ 架构分析 (Architecture Analysis)

本项目采用标准的**客户端预测/帧同步 (Deterministic Lockstep)** 架构。由于项目中不包含服务端逻辑代码（Serverless / Dummy Server），所有的游戏逻辑计算完全在客户端进行。

### 1. 帧同步核心 (Frame Synchronization)
- **输入驱动 (Input Driven)**: 游戏不再同步“位置”或“血量”，而是同步“玩家输入（Input）”。
- **确定性模拟 (Deterministic Simulation)**:
    -   两个客户端（本机与远端）运行完全相同的逻辑代码。
    -   在相同的初始状态下（`BornPos`），输入相同的指令序列（移动、开火），必然得到相同的游戏结果（位置更新、墙壁破坏、扣血）。
- **逻辑帧与渲染帧分离**:
    -   游戏逻辑以固定的频率（如 15Hz 或 30Hz）更新，确保物理计算的一致性。
    -   Unity 的 `Update` (渲染) 通过插值（Interpolation）来平滑表现逻辑帧之间的物体运动，避免画面卡顿。

### 2. 网络通信 (Networking)
- **P2P / 转发模式**: 由于没有权威服务端，客户端之间直接交换输入数据（或通过一个简单的 Relay Server 转发）。
- **锁帧机制**: 只有当客户端收到了所有玩家在当前帧的输入后，才会推进下一帧的逻辑运算。

### 3. 游戏流程 (Game Flow)
- **初始化**: 加载场景 -> 根据 PlayerID 确定 BornPos -> 实例化角色。
- **回合制逻辑**: 
    -   监听 `BulletCount`。
    -   当 `BulletCount == 0` 时，触发 `SwitchRole()` 方法，重置位置并交换控制权。

### 4. 渲染与逻辑 (Rendering vs Logic)
- **激光渲染**: 这是一个纯视觉表现（View Layer）。它根据逻辑层计算的射线检测结果进行绘制，但不影响实际的碰撞判定结果。
- **墙壁破坏**: 这是一个状态变更。当逻辑帧判定子弹命中墙壁，修改墙壁的 `Active` 状态或 `Mesh`，所有客户端同步执行此变更。

---

## 📂 项目结构 (Project Structure)

```text
Assets/
├── Scripts/
│   ├── Core/           # 帧同步核心逻辑 (Lockstep Manager)
│   ├── Logic/          # 游戏业务逻辑 (PlayerController, WeaponSystem)
│   ├── View/           # 表现层 (UI, LaserRenderer, Effects)
│   └── Network/        # 网络传输层 (Input Sender/Receiver)
├── Scenes/
│   └── MainScene       # 游戏主场景 (包含 BornPos 和 Wall)
└── Prefabs/
    ├── Hunter.prefab
    └── Runner.prefab
```

*(注：以上目录结构为基于描述的推断，请根据实际项目情况调整)*

---

## 🚀 快速开始 (Getting Started)

1.  **环境要求**: 
    -   Unity 2022.x 或更高版本。
2.  **运行方式**:
    -   Clone 本仓库到本地。
    -   使用 Unity 打开项目。
    -   为了测试联机，你通常需要构建（Build）出一个 Windows/Mac 可执行文件。
    -   运行一个客户端作为 **Host** (或 Player 1)。
    -   运行编辑器或第二个客户端作为 **Client** (或 Player 2)。
    -   连接成功后，游戏自动开始。

---

## 🤝 贡献 (Contributing)

欢迎提交 Issue 或 Pull Request 来改进同步算法或增加新的游戏机制！

---

**Author**: gopink1  
**GitHub**: [https://github.com/gopink1/FinalPK](https://github.com/gopink1/FinalPK)
