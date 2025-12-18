# FinalPK - 1v1 帧同步射击对战游戏

![Unity 2022](https://img.shields.io/badge/Unity-2022.x-blue.svg) ![Server](https://img.shields.io/badge/Server-C%23_Console-green.svg) ![NetCode](https://img.shields.io/badge/Network-Frame_Sync-orange.svg) ![Protobuf](https://img.shields.io/badge/Data-Protobuf-red.svg)

## 📖 项目简介 | Introduction

**FinalPK** 是一款基于 **Unity 2022** 和 **C# 控制台服务端** 开发的 1v1 帧同步射击游戏。

游戏核心玩法围绕“猎人(Hunter)”与“逃逸者(Runner)”的非对称对抗展开。玩家在一个包含可破坏掩体的封闭场景中进行博弈，通过精准的射击、灵活的走位以及心理战术击败对手。游戏采用**帧同步（Lockstep）**技术方案，确保了客户端之间的强一致性。

## 🎮 游戏玩法 | Gameplay

### 角色与机制
游戏分为两个回合（或多回合循环），双方玩家轮流扮演 **Hunter** 和 **Runner**。

*   **Hunter (猎人)**:
    *   **出生点**: `BornPos[0]`
    *   **目标**: 使用有限的子弹击中 Runner。
    *   **能力**:
        *   拥有 **3发子弹**。
        *   拥有一个可穿透墙壁的**激光瞄准线**，用于预判和压制。
        *   **射击机制**: 子弹具有**穿透属性**，击中墙壁会直接**破坏墙壁**（墙壁消失）并继续飞行，可直接对墙后的 Runner 造成伤害。
*   **Runner (逃逸者)**:
    *   **出生点**: `BornPos[1]`（位于掩体墙后方）
    *   **目标**: 在掩体后方通过走位躲避 Hunter 的射击。
    *   **能力**:
        *   利用场景中的墙壁作为掩体。
        *   通过输入进行战术动作转换（如移动、蹲下等）来规避伤害。

### 游戏流程 (Game Loop)
1.  **开局**: 双方分别在指定位置生成。
2.  **对抗**: Hunter 进行攻击，Runner 进行躲避。
    *   场景中的墙壁被破坏后**不会复原**，状态一直保留，意味着随着游戏进行，掩体将越来越少。
3.  **换边 (Role Swap)**:
    *   当 Hunter 的 **3发子弹耗尽** 后，触发换边逻辑。
    *   Hunter 变为 Runner，Runner 变为 Hunter，角色互换但**墙壁状态保持破坏后的样子**。
4.  **结束**: 当任意一方玩家的 **生命值 (HP) 归零** 时，游戏结束。

### 游戏 UI
*   **左侧**: 本机玩家血条。
*   **右侧**: 远程玩家（对手）血条。
*   **中间**: Hunter 当前剩余子弹数量。

---

## 🏗️ 技术架构 | Architecture

本项目采用经典的 **Client-Server** 架构，核心网络模型为 **帧同步 (Lockstep)**。

### 1. 目录结构
*   **Client/**: Unity 2022 客户端工程。包含渲染、输入处理、UI 及预测逻辑。
*   **Server/**: C# 控制台应用程序。负责帧同步的消息转发与简单的房间逻辑。
*   **Proto/**: Protobuf 消息定义文件及生成的 C# 类。

### 2. 网络通信 (Networking)
*   **通信协议**: TCP/UDP (根据具体实现，通常帧同步使用UDP或TCP均可，视即时性要求而定)。
*   **序列化**: 使用 **Google Protocol Buffers (Protobuf)** 进行高效的数据序列化与反序列化，减少网络带宽占用。

### 3. 帧同步机制 (Frame Synchronization)
由于游戏需要精确的判定（如穿墙射击、位置同步），采用了帧同步方案：
*   **逻辑分离**: 渲染层与逻辑层分离。
*   **服务端职责**: 服务端作为一个“转发器”，不运行复杂的游戏逻辑。它负责收集两名玩家在同一帧内的输入（Input），打包成“帧数据包”，然后广播给所有客户端。
*   **客户端职责**: 
    *   上传玩家操作指令（Move, Shoot 等）。
    *   接收服务端的帧数据，在本地逻辑帧中“回放”所有玩家的操作，从而驱动游戏世界更新。
    *   保证确定性（Deterministic）：相同的输入在所有客户端产生完全相同的输出（位置、血量、墙壁状态）。

### 4. 关键逻辑实现
*   **墙壁系统**: 墙壁状态由服务器帧数据驱动。当“射击”指令被执行时，所有客户端同步计算碰撞，将特定的 Wall 对象 SetActive(false) 或销毁。由于不重置场景，换边后墙壁状态天然继承。
*   **换边逻辑**: 维护一个全局 `GameState`。检测到 `BulletCount == 0` 事件帧时，执行 `SwitchRole()` 方法，交换双方的控制权映射，重置子弹计数，但不重置场景物体。

---

## 🚀 快速开始 | Getting Started

### 环境要求
*   **Unity**: 2022.x 或更高版本
*   **IDE**: Visual Studio 2022 / JetBrains Rider
*   **.NET SDK**: .NET 6.0 或更高（用于服务端）

### 部署步骤
1.  **克隆仓库**:
    ```bash
    git clone https://github.com/gopink1/FinalPK.git
    ```
2.  **启动服务端**:
    *   进入 `Server` 目录。
    *   运行 C# 控制台程序（`dotnet run` 或通过 IDE 启动）。
    *   确保服务端端口（默认例如 8888）已开放。
3.  **运行客户端**:
    *   使用 Unity Hub 打开项目文件夹。
    *   在 `Build Settings` 中将场景添加到列表中。
    *   打包出两个 Windows/Mac 可执行文件，或者在编辑器中运行一个，打包运行一个。
    *   分别点击“开始游戏”连接本地服务端。

---

## 📝 待办事项 | To-Do
*   [ ] 增加更多掩体布局。
*   [ ] 添加音效与击中反馈特效。
*   [ ] 优化断线重连机制。

---

## 🤝 贡献 | Contributing
欢迎提交 Issue 或 Pull Request 来改进代码！

## 📄 许可证 | License
MIT License
