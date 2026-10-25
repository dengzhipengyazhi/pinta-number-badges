# PintaNumberBadges — Pinta「数字圆标」插件

[![Build](https://github.com/dengzhipengyazhi/pinta-number-badges/actions/workflows/build.yml/badge.svg)](https://github.com/dengzhipengyazhi/pinta-number-badges/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Pinta 3.1+](https://img.shields.io/badge/Pinta-3.1%2B-blue.svg)](https://www.pinta-project.com/)

给图片快速添加 **圆圈 + 数字** 标注（教程步骤编号、图片说明、要点标记等），以 Pinta 官方 **add-in 插件** 的形式实现。

支持两种风格：**实心徽章** 和 **马克笔/手绘圆圈**（圆圈内部透明、线条不规则、数字用手写字体）。

![两种风格](https://github.com/dengzhipengyazhi/pinta-number-badges/releases/download/v1.1.0/sample-badges.png)

**手绘红圈效果**：

![手绘红圈示例](https://github.com/dengzhipengyazhi/pinta-number-badges/releases/download/v1.1.0/handdrawn-example.png)

> 两张图都由插件真实的 `BadgeRenderer` 代码渲染（没有启动 Pinta 图形界面，直接用 Pinta 的 Cairo/Pango 栈生成），即最终落到图片上的实际效果。

## 下载安装

1. 从 [Releases](https://github.com/dengzhipengyazhi/pinta-number-badges/releases/latest) 下载最新的
   `PintaNumberBadges.<version>.mpack`（本仓库的 [`package/`](package/) 目录里也有一份）；
2. 打开 Pinta（3.1.x），主菜单（标题栏右上角 **☰**；或菜单栏）→
   **「插件」→「插件管理器...」**（英文界面：*Add-ins → Add-in Manager...*）；
3. 点击插件管理器左上角的 **「从文件安装...」**（Install from file...），选择刚下载的 `.mpack`；
4. 按提示安装，然后**重启 Pinta**；
5. 工具箱里出现「数字圆标」工具（图标与文字工具相同，因为插件暂未打包自定义图标）。

> 该 `.mpack` 已用 Mono.Addins 官方工具 `mautil` 打包，并做过安装校验
>（Pinta 3.1.2 根插件、依赖 `Pinta >= 3.1`、扩展节点均正确注册）。

### 开发/调试安装

按官方 `PintaDemoExtension` 的做法，也可以把编译出的 `PintaNumberBadges.dll`
复制到 Pinta 的安装目录（与 `Pinta.Core.dll` 同级）。Windows 下若 Pinta 装在
`Program Files`，需要管理员权限。

## 功能

| 功能 | 说明 |
|---|---|
| 新增工具 | 工具箱新增「数字圆标 / Numbered Badge」工具 |
| 单击放置 | 左键 = 用主色，右键 = 用副色 |
| 数字 | 0–9999；默认每放一个自动 +1（可关闭，用于重复同一编号） |
| 填充 / 不填充 | 填充 = 实心徽章（自动黑/白对比色文字）；不填充 = 马克笔圆圈，内部透明 |
| 手绘风格 | 圆圈略带抖动、不闭合；数字使用系统手写字体（Windows: Segoe Print / macOS: Bradley Hand / 其他: Comic Sans MS 或回退无衬线），并带轻微倾斜 |
| 线宽 | 圆圈/描边宽度 0–50 px；不填充且设为 0 时自动取 ≈12% 半径的线宽 |
| 半径 | 3–1000 px，可用 `[` / `]` 快捷键微调 |
| 实时预览 | 鼠标移动时在临时工具层显示预览，**不会**被保存/导出/拼合进图片 |
| 无限撤销 | 每放一个圆标生成一条历史记录，`Ctrl+Z` 可撤销 |
| 记忆设置 | 编号、自动递增、半径、线宽、填充、手绘在重启 Pinta 后保留 |
| 双语界面 | 自动根据系统语言显示中文或英文 |

## 调成「手绘红圈」效果

1. 把调色板 **主色** 设为红色（点击状态栏色块修改）；
2. 工具栏 **关闭「填充」**（`Fill` 不勾选）→ 圆内透明，只画圈；
3. 工具栏 **打开「手绘」**（`Hand` 勾选）→ 不规则圆 + 手写体数字；
4. 半径设 20–60、线宽设 3–6（或填 0 自动）；
5. 在画布上单击即可。

> 手写数字依赖系统字体：Windows 用 **Segoe Print**，macOS 用 **Bradley Hand**；
> 缺少时自动回退成无衬线字体（圆圈的手绘抖动仍然生效）。

## 使用

1. 选择「数字圆标」工具；
2. 工具栏设置：**编号 / 自动 / 半径 / 线宽 / 填充 / 手绘**；
3. 在画布上单击放置；右键用副色；
4. `[` / `]` 调整半径；
5. 建议先 **图层 → 添加新图层**（`Ctrl+Shift+N`）再标注，保持原图不被修改；
6. `Ctrl+Z` 撤销上一个圆标。

## 从源码构建

### 依赖

- **.NET 8 SDK**（Pinta 3.1.x 的目标框架是 `net8.0`）
- **Pinta 3.1.x 源码**（作为编译引用；脚本会自动下载到 `../Pinta-3.1.2-src`）

```powershell
# 下载 Pinta 3.1.2 源码（放到仓库同级目录，默认 PintaSourceDir 指向这里）
pwsh ./scripts/get-pinta-source.ps1

# 编译
dotnet build -c Release

# 或者：指定源码路径
dotnet build -c Release "-p:PintaSourceDir=D:\src\Pinta-3.1.2-src"

# 编译并打包 .mpack（需要 Mono.Addins.UtilTool）
.\build.ps1 -Pack
```

产物：

```
bin\Release\net8.0\PintaNumberBadges.dll     # 开发安装用
package\PintaNumberBadges.1.1.mpack          # 最终安装包
```

也可以手动把 Pinta 作为 git submodule 引用（官方 add-in 的推荐做法）：

```powershell
git submodule add https://github.com/PintaProject/Pinta.git Pinta
git -C Pinta checkout 3.1.2
dotnet build -c Release "-p:PintaSourceDir=$PWD/Pinta"
```

### 兼容性

- **目标**：Pinta **3.1.x**（按 3.1.2 的公开 API 编译；Pinta 3.1.0 起 API 一致）
- Pinta 3.2 开发版保留了 `AddinCompatVersion = "3.1"`，按设计仍可运行；
- 若要用 master（3.2）源码编译，需把目标框架改成 `net10.0`
  （`-p:TargetFramework=net10.0`）并把 `.addin.xml` 的依赖版本同步改为 3.2；
- Windows / Linux / macOS 通用：插件完全托管，GTK、Cairo、Pango 等原生库都由 Pinta 提供。

## 仓库结构

```
.
├─ .addin.xml                 # Mono.Addins 清单（嵌入 DLL），声明 id/版本/对 Pinta>=3.1 的依赖
├─ NumberBadgesExtension.cs   # [Mono.Addins.Extension] IExtension：Initialize 注册工具 / Uninitialize 注销
├─ NumberBadgeTool.cs         # BaseTool 子类：工具栏、鼠标/键盘事件、预览、写入图层、撤销
├─ BadgeRenderer.cs           # 纯绘制逻辑：圆/手绘圈 + 居中数字 + 对比色 + 自动缩放
├─ Strings.cs                 # 中英双语文案
├─ docs/                      # 渲染示例图
├─ package/*.mpack            # 打包好的安装包
├─ scripts/get-pinta-source.ps1  # 下载构建所需的 Pinta 源码
├─ build.ps1                  # 构建 / 打包脚本
├─ .github/workflows/build.yml   # CI：构建 + 打包 .mpack 作为 artifact
├─ CHANGELOG.md / CONTRIBUTING.md / SECURITY.md / LICENSE
└─ PintaNumberBadges.csproj
```

## 实现说明

- **注册方式**：Pinta 的 `Pinta.Core.IExtension` 是 Mono.Addins 的 *type extension
  point*（路径 `/Pinta/TypeExtensions/Pinta.Core.IExtension`）。扩展类加
  `[Mono.Addins.Extension]` 即可被发现，`Initialize()` 里
  `PintaCore.Tools.AddTool(...)`，与内置工具（`CoreToolsExtension`）走同一条路径。
- **预览**：画在 `document.Layers.ToolLayer`（临时工具层）上，鼠标移动时重绘；
  Pinta 的 `GetLayersToPaint` 会显示它，而 `GetFlattenedImage`（保存/导出/拼合）
  **排除**它，所以预览不会污染图片。
- **撤销**：放置前 `Surface.Clone()` 快照，放置后
  `PushNewItem(new SimpleHistoryItem(...))`。
- **手绘圈**：以确定性随机种子（由编号 + 半径决定，因此预览与最终落笔完全一致）
  生成 13 个抖动点，用 Catmull-Rom 转贝塞尔画出略微椭圆、有轻微旋转、首尾重叠的
  闭合路径，`LineCap.Round` 描边；填充模式下则填充该不规则轮廓。
- **文字渲染**：`PangoCairo.Functions.CreateLayout(cairoContext)` +
  `Pango.Layout.GetPixelExtents` 精确居中；数字过长/过大时自动缩小字号；
  手绘模式使用系统手写字体并旋转约 ±4°。
- **设置持久化**：只存 `int` / `bool`（Pinta 的设置文件只回读 int/bool/string）。

## 隐私与安全

- 运行时不联网、无遥测、无分析上报；不读取、不存储任何密钥或个人信息。
- 仅使用 Pinta 自身的设置文件（记忆工具选项）和文档的临时工具层。
- 仓库中不包含任何 API key、token、私钥或个人信息；`.gitignore` 排除了构建产物。
- 安全问题请通过 [SECURITY.md](SECURITY.md) 的私有渠道报告。

## 已知限制

- 圆标画在**当前图层**（Pinta 工具的常规行为），建议先建新图层；
- 编号超过 9999 后回绕到 0；
- 手写数字是「系统手写字体」而不是真正的笔迹（同编号圆标的圈形固定）；
- 预览只在当前选中图层之上显示，切换图层后移动一下鼠标即刷新；
- 工具箱图标复用了 Pinta 内置的文字工具图标；
- 中英文字符串直接写在插件里（未接入 Pinta 的 gettext 翻译流程）。

## 声明

- 本项目是**第三方插件**，与 Pinta 官方项目无隶属关系；Pinta 本身是
  MIT 许可的开源项目（<https://github.com/PintaProject/Pinta>）。
- 本插件在 **AI 辅助**下开发完成，由维护者审核并发布。
- 如果你计划将其提交到 Pinta 官方仓库或社区插件仓库，请先阅读 Pinta 的
  [contribution policy](https://github.com/PintaProject/Pinta/blob/master/patch-guidelines.md)：
  项目要求代码由人类撰写，明确禁止 AI 生成代码的贡献。

## 许可证

[MIT](LICENSE) © 2026 dengzhipengyazhi

MIT 与 Pinta 及其官方示例插件（`PintaDemoExtension`、`FarbfeldFormatAddin`）一致，
允许自由使用、修改、分发，保留版权与许可声明即可。详见 [CHANGELOG.md](CHANGELOG.md)
与 [CONTRIBUTING.md](CONTRIBUTING.md)。
