# AvaloniaClaudePet

一个 Avalonia UI 应用程序，通过可爱的宠物形象实时显示 Claude Code 的工作状态。

## 🎯 项目目的

AvaloniaClaudePet 是一个桌面应用程序，它以可视化的方式展示 Claude Code 的活动状态。当你使用 Claude Code 进行开发时，屏幕上会显示一个可爱的宠物角色，它会根据 Claude 的工作状态（思考、执行工具、成功、错误等）改变表情和行为，让你的编程体验更加生动有趣。并且可以在你无需紧盯屏幕的情况下放松工作，直到收到提醒。

## ✨ 功能特性

### 核心功能
- 🐱 **实时状态显示**：宠物会根据 Claude Code 的事件（如开始思考、执行工具、完成等）改变表情
- 🖼️ **多种表情**：idle（空闲）、thinking（思考）、working（工作中）、success（成功）、error（错误）、waiting（等待）
- 💬 **通知气泡**：当 Claude 等待用户输入或需要权限时显示提示
- 🔄 **系统托盘**：最小化到系统托盘，随时可访问
- 🌍 **跨平台支持**：Windows、macOS、Linux 全平台支持

### 技术特性
- 🚀 **高性能**：基于 .NET 8 和 Avalonia UI 11.x
- 📡 **嵌入式 HTTP 服务器**：接收来自 Claude Code 的 hooks 事件
- 🔗 **自动 Hook 配置**：自动在 Claude Code 中配置 hooks
- 🎨 **流畅动画**：表情切换和通知气泡都有平滑的过渡动画

## 📖 工作原理

### 事件映射机制

应用程序通过 Claude Code 的 hooks 系统接收事件，并将这些事件映射到宠物的状态：

| Claude 事件 | 宠物状态 | 描述 |
|-------------|---------|------|
| SessionStart | Idle | 宠物唤醒，欢迎用户 |
| UserPromptSubmit | Thinking | 用户提交问题，宠物开始思考 |
| PreToolUse | Working | 准备执行工具，宠物显示工作中状态 |
| PostToolUse | Working | 工具执行中 |
| PostToolUseFailure | Error | 工具执行失败，宠物显示错误表情 |
| Notification(idle) | Waiting | 等待用户返回，宠物呼唤用户 |
| Notification(perm) | Waiting | 需要用户授权，宠物提醒用户 |
| Stop | Success | 任务完成，宠物庆祝 |
| StopFailure | Error | 任务失败，宠物显示担忧 |
| SubagentStart | Thinking | 子代理启动，宠物进入深度思考 |
| SubagentStop | Working | 子代理结束，返回工作状态 |
| SessionEnd | Idle | 会话结束，宠物进入空闲状态 |

### 系统架构

```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│   Claude Code   │──HTTP→│  HTTP 服务器   │──事件→│   状态机       │
│ (发送 Webhook)  │      │ (监听 12345)    │      │ (状态转换)      │
└─────────────────┘      └─────────────────┘      └────────┬────────┘
                                                          │
                                                          ▼
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│   宠物 UI       │      │   ViewModel    │      │   服务层       │
│ (显示表情动画)   │◀─────│ (数据绑定)      │◀─────│ (业务逻辑)      │
└─────────────────┘      └─────────────────┘      └─────────────────┘
```

## 🛠️ 技术方案

### 技术栈

| 层级 | 技术 | 说明 |
|------|------|------|
| UI 框架 | Avalonia UI 11.x | 跨平台 .NET XAML 框架 |
| 运行时 | .NET 8 LTS | 性能卓越的跨平台运行时 |
| HTTP 服务器 | ASP.NET Core Minimal API | 轻量级内置 HTTP 服务器 |
| JSON 处理 | System.Text.Json | 高性能内置 JSON 库 |
| 动画系统 | Avalonia Storyboards | 原生表情过渡动画 |
| 系统托盘 | Avalonia TrayIcon | 跨平台托盘支持 |
| 构建方式 | dotnet publish | 单文件自包含发布 |

### 核心组件

#### 1. PetStateMachine (状态机)
- 管理宠物状态转换
- 处理事件触发和状态变更
- 实现临时状态的自动恢复（如错误状态 2 秒后恢复）

#### 2. HookHttpServer (HTTP 服务器)
- 监听来自 Claude Code 的 Webhook 事件
- 解析 JSON 负载数据
- 将事件转发给状态机

#### 3. PetWindow & PetControl (UI 层)
- 透明背景、无边框、置顶窗口
- 宠物形象渲染
- 表情切换动画

#### 4. NotificationService (通知服务)
- 管理通知气泡的显示和隐藏
- 处理超时自动消失
- 支持气泡替换（新通知覆盖旧通知）

#### 5. HookConfigService (配置服务)
- 读取/写入 Claude Code 配置文件
- 自动配置 hooks
- 处理端口冲突和配置合并

### 项目结构

```
AvaloniaClaudePet/
├── src/
│   ├── AvaloniaClaudePet/
│   │   ├── Views/                # 视图
│   │   │   ├── PetWindow.axaml   # 宠物主窗口
│   │   │   └── BubbleWindow.axaml # 通知气泡窗口
│   │   ├── ViewModels/           # 视图模型
│   │   │   ├── PetViewModel.cs   # 宠物数据绑定
│   │   │   └── BubbleViewModel.cs # 气泡数据绑定
│   │   ├── Services/             # 服务层
│   │   │   ├── PetStateMachine.cs # 状态机
│   │   │   ├── HookHttpServer.cs  # HTTP 服务器
│   │   │   ├── NotificationService.cs # 通知服务
│   │   │   └── HookConfigService.cs # 配置服务
│   │   ├── Models/               # 数据模型
│   │   │   ├── PetState.cs       # 宠物状态枚举
│   │   │   ├── HookEvent.cs      # Hook 事件模型
│   │   │   └── NotificationType.cs # 通知类型
│   │   ├── Controls/             # 控件
│   │   │   └── PetControl.axaml  # 宠物控件
│   │   └── Assets/               # 资源文件
│   │       └── Expressions/      # 表情图片
│   └── AvaloniaClaudePet.Tests/ # 单元测试
└── openspec/                     # OpenSpec 规范
```

## 📦 编译方式

### 环境要求

- .NET 8 SDK 或更高版本
- 支持 Avalonia UI 11.x 的开发环境

### 构建步骤

1. **克隆仓库**
   ```bash
   git clone <repository-url>
   cd AvaloniaClaudePet
   ```

2. **还原依赖**
   ```bash
   dotnet restore
   ```

3. **构建项目**
   ```bash
   dotnet build
   ```

4. **运行测试**
   ```bash
   dotnet test
   ```

5. **发布应用程序**
   ```bash
   # Windows
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   
   # macOS
   dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
   
   # Linux
   dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
   ```

### 编译选项说明

- `-c Release`：发布配置（Debug 为调试配置）
- `-r <runtime>`：目标运行时（win-x64、osx-x64、linux-x64）
- `--self-contained`：生成自包含的可执行文件
- `-p:PublishSingleFile=true`：生成单个可执行文件

## 🚀 使用方法

### 首次运行

1. 运行应用程序
2. 首次启动时，应用程序会自动提示配置 Claude Code 的 hooks
3. 点击"配置 Hooks"按钮，应用程序会自动修改 `~/.claude/settings.json`
4. 重启 Claude Code 以使 hooks 生效

### 手动配置 Hooks

如果自动配置失败，可以手动编辑 `~/.claude/settings.json`：

```json
{
  "hooks": {
    "SessionStart": [{
      "hooks": [{
        "type": "command",
        "command": "curl -s -X POST http://localhost:12345/hooks/session-start -H 'Content-Type: application/json' -d '{\"session_id\":\"$CLAUDE_SESSION_ID\",\"hook_event_name\":\"SessionStart\"}'"
      }]
    }],
    "UserPromptSubmit": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/prompt-submit" }]
    }],
    "PreToolUse": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/pre-tool-use" }]
    }],
    "PostToolUse": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/post-tool-use" }]
    }],
    "PostToolUseFailure": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/tool-failure" }]
    }],
    "Notification": [
      {
        "matcher": "idle_prompt",
        "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/notification/idle" }]
      },
      {
        "matcher": "permission_prompt",
        "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/notification/permission" }]
      }
    ],
    "Stop": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/stop" }]
    }],
    "StopFailure": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/stop-failure" }]
    }],
    "SubagentStart": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/subagent-start" }]
    }],
    "SubagentStop": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/subagent-stop" }]
    }],
    "SessionEnd": [{
      "hooks": [{ "type": "http", "url": "http://localhost:12345/hooks/session-end" }]
    }]
  }
}
```

### 系统托盘功能

- **显示宠物**：显示/隐藏宠物窗口
- **配置 Hooks**：重新配置 Claude Code hooks
- **端口信息**：显示当前监听的端口
- **退出**：完全退出应用程序

## 🔧 故障排除

### 端口冲突

如果默认端口 12345 被占用，应用程序会自动尝试 12346-12350。如果仍然冲突：

1. 检查端口占用情况
2. 修改 `~/.claude/settings.json` 中的 URL
3. 重启应用程序

### Hooks 不工作

1. 确认 Claude Code 已重启
2. 检查 `~/.claude/settings.json` 配置是否正确
3. 查看 HTTP 服务器日志（将在后续版本中添加）

### UI 问题

- 如果宠物窗口不透明，检查是否支持透明窗口
- 如果动画卡顿，尝试更新显卡驱动
- 如果托盘图标不显示，尝试以管理员权限运行

## 🤝 贡献指南

欢迎提交 Issue 和 Pull Request！

### 开发流程

1. Fork 项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🎨 表情资源

表情图片存放在 `Assets/Expressions/` 目录下，支持：
- idle.png - 空闲状态
- thinking.png - 思考状态
- working.png - 工作状态
- success.png - 成功状态
- error.png - 错误状态
- waiting.png - 等待状态

## 📞 支持

如有问题或建议，请：
1. 查看文档和 FAQ
2. 搜索已有的 Issue
3. 创建新的 Issue 描述问题
4. 联系维护者

---

**享受与 Claude 一起编程的乐趣！** 🐱💻
