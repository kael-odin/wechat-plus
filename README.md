
# WeChat Plus 商用工作台

本仓库当前在上游 `huiyadanli/RevokeMsgPatcher` v2.1 基础上新增了 WeChat Plus MVP 开发骨架，用于实现“闭源商用主程序 + 独立开源助手组件”的微信多开工作台。原上游 GPLv3 项目内容保留在下方，新的商用工作台代码放在独立的 `WeChatPlus.sln` 中，避免和原补丁工具混在一个可执行程序里。

## 当前新增内容

- `docs/product/commercial-shell-prd.md`：商用工作台 PRD。
- `docs/product/closed-source-commercial-plan.md`：闭源商用计划书。
- `docs/product/release-package-manifest.md`：MVP 运行包清单和 GPL 边界说明。
- `docs/superpowers/plans/2026-06-26-wechat-plus-mvp.md`：MVP 实施计划。
- `WeChatPlus.sln`：新的 MVP 解决方案。
- `WeChatPlus.Core`：中立核心模型、助手命令契约、本地话术库、账号持久化、试用授权状态、授权 API 请求/响应解析和云端激活编排、云端更新清单获取和本地回退、开源组件声明数据、设置摘要服务、诊断包服务、运行包校验服务、运行环境检查服务、助手完整性校验服务和安装清单元数据。
- `WeChatPlus.OpenHelper`：独立开源助手组件命令行原型，输出 JSON。
- `WeChatPlus.Shell`：闭源商用壳原型，三栏 WinForms 工作台 UI。
- `WeChatPlus.Tests`：无第三方依赖的控制台测试。
- `WeChatPlus.Shell/update-manifest.json`：本地更新清单占位，用于主程序和助手组件版本检查，商业发布前可替换为云端清单。

## 架构边界

WeChat Plus 的商业化边界按以下方式设计：

- 闭源商用壳只引用 `WeChatPlus.Core` 中新写的中立模型和服务。
- 多开、补丁等 GPL 敏感能力放在 `WeChatPlus.OpenHelper` 独立可执行文件中。
- 商用壳通过进程边界调用助手组件，不复制、不链接 GPLv3 源码。
- 如果后续把 RevokeMsgPatcher 的 GPLv3 多开/补丁实现迁入助手组件，助手组件对应修改源码必须公开，并在产品内展示许可证和源码地址。

当前 `WeChatPlus.OpenHelper` 已提供 JSON CLI 边界和多开助手命令；`multi-instance start` 会先尝试关闭已有微信实例互斥句柄再启动本机微信，`windows` 可枚举微信进程主窗口，`embed`/`detach` 可尝试把微信窗口嵌入或拆回独立窗口，`focus`、`close`、`close-all`、`close-mutex` 和 `close-all-mutex` 都在独立助手组件内执行。

## 构建与验证

当前环境没有 `dotnet` SDK 或 Visual Studio MSBuild，已验证可使用系统 .NET Framework MSBuild 构建新的 MVP 解决方案：

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' WeChatPlus.sln /p:Configuration=Debug /p:Platform='Any CPU' /v:minimal
.\WeChatPlus.Tests\bin\Debug\WeChatPlus.Tests.exe
.\WeChatPlus.OpenHelper\bin\Debug\WeChatPlus.OpenHelper.exe version --json
.\WeChatPlus.OpenHelper\bin\Debug\WeChatPlus.OpenHelper.exe multi-instance status
.\WeChatPlus.OpenHelper\bin\Debug\WeChatPlus.OpenHelper.exe multi-instance windows
.\WeChatPlus.OpenHelper\bin\Debug\WeChatPlus.OpenHelper.exe patch status --app wechat
.\WeChatPlus.Install\bin\Debug\WeChatPlus.Install.exe --plan
.\WeChatPlus.Uninstall\bin\Debug\WeChatPlus.Uninstall.exe --plan
```

运行商用壳：

```powershell
.\WeChatPlus.Shell\bin\Debug\WeChatPlus.Shell.exe
```

安装入口：
```powershell
.\WeChatPlus.Install\bin\Debug\WeChatPlus.Install.exe --plan
.\WeChatPlus.Install\bin\Debug\WeChatPlus.Install.exe --package-root .\WeChatPlus.Shell\bin\Debug --install-root "$env:TEMP\WeChatPlusInstall" --start-menu-root "$env:TEMP\WeChatPlusStart"
.\WeChatPlus.Install\bin\Debug\WeChatPlus.Install.exe --package-root .\WeChatPlus.Shell\bin\Debug --install-root "$env:TEMP\WeChatPlusInstall" --start-menu-root "$env:TEMP\WeChatPlusStart" --write-registry --rollback-on-failure
```

`WeChatPlus.Install.exe` 使用 `InstallerManifest`/`InstallPlanner` 生成安装计划，复制清单列出的运行文件，通过 `WScript.Shell` late binding 创建开始菜单 `.lnk`，并在 JSON 输出中返回 `shortcutMode`（`windows-shell-link` 或 `fallback-target-file`）；同时写入 `install-registration.json` 作为卸载注册表登记预演。默认不写注册表，只有显式传入 `--write-registry` 时才写入 HKCU 卸载登记，并返回 `registryMode`。传入 `--rollback-on-failure` 后，安装失败会尝试删除已复制文件、快捷方式、登记 JSON 和本次写入的 HKCU 卸载登记。

卸载入口：

```powershell
.\WeChatPlus.Uninstall\bin\Debug\WeChatPlus.Uninstall.exe --plan
.\WeChatPlus.Uninstall\bin\Debug\WeChatPlus.Uninstall.exe
.\WeChatPlus.Uninstall\bin\Debug\WeChatPlus.Uninstall.exe --remove-registry
.\WeChatPlus.Uninstall\bin\Debug\WeChatPlus.Uninstall.exe --remove-user-data
```

`WeChatPlus.Uninstall.exe` 使用 `InstallerManifest`/`UninstallPlanner` 生成清理计划，只删除清单列出的运行文件和快捷方式；默认保留用户数据目录且不删除注册表，只有显式传入 `--remove-registry` 时才删除 HKCU 卸载登记，显式传入 `--remove-user-data` 时才删除本地数据目录。后续完整安装器仍需补齐提权、更完整的数据清理 UI 和正式 MSI/EXE 打包流程。

注意：直接构建原始 `RevokeMsgPatcher.sln` 在当前机器上会因为缺少对应 Targeting Pack、NuGet 包和旧 MSBuild 编译能力出现错误；新的 MVP 工作集中在 `WeChatPlus.sln`。

## MVP 状态

已完成：

- 三栏工作台 UI：账号栏、微信工作区嵌入优先模式（失败时自动降级为聚焦模式）、右侧快捷话术栏、顶部会员/设置/检查更新/开源声明入口、左侧隐私锁、底部截图入口。
- 本地话术库：默认分类、默认话术、搜索、新增/编辑/删除、点击复制、底部常用短语按钮、JSON 导入导出、CSV 导入。
- 本地账号管理：账号记录保存到 `accounts.json`，支持备注编辑、删除本地记录、账号排序，启动/刷新微信窗口后更新账号状态，检测不到的旧进程会自动标记离线，选中已检测窗口时通过助手组件请求聚焦并在中间区展示账号、PID、窗口句柄和聚焦结果。
- 隐私锁：锁定状态和 PIN 哈希保存到 `privacy_lock.json`，首次启用前要求设置自定义 PIN，启用后隐藏会话承载区和进程状态，输入 PIN 后恢复显示；后续可继续接入系统认证。
- 试用/会员授权状态：设备哈希、试用期、离线宽限期、本地激活码状态、云端激活请求构造、可替换传输层、响应落地和网络失败保留本地状态；试用版限制最多管理 2 个微信账号、最多保存 50 条话术并禁用话术导入导出；不硬编码真实密钥。
- 开源组件声明：默认记录 `WeChatPlus.OpenHelper`、GPLv3 许可证和上游源码地址，并在商用壳内展示。
- 助手组件：`version --json`、`multi-instance status`、`multi-instance windows`、`multi-instance embed --handle <hWnd> --parent <hWnd>`、`multi-instance detach --handle <hWnd>`、`multi-instance focus --handle <hWnd>`、`multi-instance close --pid <pid>`、`multi-instance close-all`、`multi-instance close-all-mutex`、`multi-instance close-mutex --pid <pid>`、`patch status --app wechat`。
- 运行环境检查：商用壳启动和设置页可展示管理员权限、助手组件可用性、微信安装路径和当前微信进程数；微信安装路径由独立助手组件通过 `multi-instance status` 返回。
- 更新检查：商用壳会先尝试拉取预留云端 `update-manifest.json`，失败时回退运行目录本地清单，比较主程序和助手组件版本并展示更新状态；如果清单提供 `helperSha256`，会对当前 `WeChatPlus.OpenHelper.exe` 做 SHA-256 完整性校验；当前使用占位云端地址，不包含真实更新密钥。
- 设置页：商用壳可展示运行环境检查、运行包校验结果、缺失必需文件、数据目录、运行目录、助手组件、更新清单、账号、话术、授权、隐私锁、隐私说明、开源组件声明和诊断日志路径及存在状态，并提供隐私说明查看和本地数据清理入口；清理动作只删除账号备注、话术、授权缓存、隐私锁状态、组件声明和诊断日志，不删除运行程序。
- 工作台工具：结构化解析助手组件窗口 JSON，批量刷新微信进程/窗口状态、关闭选中微信进程、关闭全部微信、截图到剪贴板、截图时隐藏当前窗口、诊断入口可生成脱敏支持包。
- 构建输出：商用壳会把独立助手组件、中立 Core、GPLv3 `LICENSE`、`README.md`、`components.json` 和 `update-manifest.json` 复制到自身输出目录，形成最小 MVP 运行包。
- 安装清单：Core 提供默认安装目录、开始菜单快捷方式、卸载入口、卸载注册表键、运行包文件移除列表、默认保留用户数据策略和 GPL 助手源码地址等结构化元数据，供安装器/打包器消费；当前安装入口已支持真实 `.lnk` 创建、显式 HKCU 卸载登记写入/删除和失败回滚，但还不是完整 MSI/EXE 安装器。
- 测试：命令解析、JSON 输出、助手版本/运行环境/窗口 JSON 解析、工作区聚焦状态文案、诊断日志写入/导出、脱敏诊断包生成和导出目录命名、话术种子/搜索/常用短语、话术更新/删除、JSON/CSV 导入、账号持久化/备注/删除/排序/离线同步、隐私锁状态持久化和自定义 PIN、本地数据清理、开源组件声明、运行包清单、安装清单、运行包文件校验、助手 SHA-256 校验、设置摘要、运行环境检查、试用/本地激活授权状态、云端激活响应解析/持久化、云端激活传输编排、云端更新清单加载/本地回退、授权功能限制、试用版话术数量限制和更新清单状态。

下一步：

- 继续打磨微信窗口嵌入体验，包括窗口样式恢复、最小化/最大化同步和更多微信版本兼容性验证。
- 接入真实授权服务端、云端更新清单和安装器。

---

<p align="center">
	<a><img width="100px" src="https://raw.githubusercontent.com/huiyadanli/RevokeMsgPatcher/master/Images/logo.png"/></a>
</p>
<p align="center">
	<a href="https://www.microsoft.com/download/details.aspx?id=30653">
		<img src="https://img.shields.io/badge/platform-windows-lightgrey.svg?style=flat-square"/>
	</a>
	<a href="https://github.com/huiyadanli/RevokeMsgPatcher/releases">
		<img src="https://img.shields.io/github/downloads/huiyadanli/RevokeMsgPatcher/total.svg?style=flat-square"/>
	</a>
	<a href="https://ci.appveyor.com/project/huiyadanli/RevokeMsgPatcher">
		<img src="https://img.shields.io/appveyor/ci/huiyadanli/RevokeMsgPatcher.svg?style=flat-square"/>
	</a>
</p>

# 👀微信/QQ/TIM防撤回补丁
适用于 Windows 下 PC 版微信/QQ/TIM的防撤回补丁。**支持最新版微信/QQ/TIM**，其中微信能够选择安装多开功能。

<img width="180px" src="https://raw.githubusercontent.com/huiyadanli/RevokeMsgPatcher/master/Images/revoke.jpg"/>

下载地址：
**[⚡️点我下载最新版本](https://github.com/huiyadanli/RevokeMsgPatcher/releases/download/2.0/RevokeMsgPatcher.v2.0.zip)** |
[☁备用下载-蓝奏云](https://wwmy.lanzouq.com/b0fot7dpe)  密码:coco| 
[☁备用下载-百度云](https://pan.baidu.com/s/15ilr78t8F1-VW8eUZSkr_Q?pwd=3rrj) 

相关文档：
**[✔支持哪些版本](https://github.com/huiyadanli/RevokeMsgPatcher/wiki/%E7%89%88%E6%9C%AC%E6%94%AF%E6%8C%81)** | 
[❓常见问题](https://github.com/huiyadanli/RevokeMsgPatcher/wiki#%E5%B8%B8%E8%A7%81%E9%97%AE%E9%A2%98) | 
[📖查看完整文档](https://github.com/huiyadanli/RevokeMsgPatcher/wiki)

原理与方法：
[📗微信](https://github.com/huiyadanli/RevokeMsgPatcher/wiki/%E5%BE%AE%E4%BF%A1%E9%98%B2%E6%92%A4%E5%9B%9E%E4%B8%8E%E5%A4%9A%E5%BC%80%E6%95%99%E7%A8%8B) |
[📕QQ](https://github.com/huiyadanli/RevokeMsgPatcher/wiki/QQ%E6%88%96TIM%E9%98%B2%E6%92%A4%E5%9B%9E%E6%95%99%E7%A8%8B) |
[📘TIM](https://github.com/huiyadanli/RevokeMsgPatcher/wiki/QQ%E6%88%96TIM%E9%98%B2%E6%92%A4%E5%9B%9E%E6%95%99%E7%A8%8B)
**（本人不参与方法寻找，仅做特征搬运）**

附带产物：[一个通用的微信多开工具](https://github.com/huiyadanli/RevokeMsgPatcher/tree/master/RevokeMsgPatcher.MultiInstance)

## 📷截图
![Screenshot](https://raw.githubusercontent.com/huiyadanli/RevokeMsgPatcher/master/Images/screenshot.png)

## 🔨使用方法

1. 首先，你的系统需要满足以下条件：

    * Windows 7 或更高版本，**不支持XP**。
    * [.NET Framework 4.5.2](https://www.microsoft.com/en-us/download/details.aspx?id=42642) 或更高版本。**低于此版本在打开程序时可能无反应，或者直接报错**。

2. 使用本程序前，先关闭微信/QQ/TIM。

3. **以管理员身份运行本程序**，等待右下角获取最新的补丁信息。

4. 选择微信/QQ/TIM的安装路径。如果你用的安装版的微信/QQ/TIM，正常情况下本程序会自动从注册表中获取安装路径，绿色版需要手动选择路径。

5. 点击防撤回。界面可能会出现一段时间的无响应，请耐心等待。**由于修改了微信的 WeChatWin.dll 文件、QQ/TIM的 IM.dll 文件，杀毒软件可能会弹出警告，放行即可。**

注意：微信/QQ/TIM更新之后要重新安装补丁！

## 💡致谢

本项目早期内容源自 [wechat_anti_revoke](https://github.com/36huo/wechat_anti_revoke) 项目。

QQNT 防撤回依赖于 [LiteLoaderQQNT](https://github.com/LiteLoaderQQNT/LiteLoaderQQNT)，修补依赖于 [DLLHijackMethod](https://github.com/LiteLoaderQQNT/QQNTFileVerifyPatch/tree/DLLHijackMethod) 并集成了以下插件：

* [插件列表查看 LL-plugin-list-viewer](https://github.com/ltxhhz/LL-plugin-list-viewer)
* [防撤回 LiteLoaderQQNT-Anti-Recall](https://github.com/xh321/LiteLoaderQQNT-Anti-Recall)

微信4.0版本后的防撤回特征来自于 [BetterWX](https://github.com/zetaloop/BetterWX)

## ❤️投喂

觉的好用的话，可以支持作者哟ヾ(･ω･`｡) 
* [⚡爱发电](https://afdian.com/@huiyadanli)
* [🍚微信赞赏](https://github.com/huiyadanli/huiyadanli/blob/master/DONATE.md)

## 📄License
[GPLv3](https://github.com/huiyadanli/RevokeMsgPatcher/blob/master/LICENSE)

![](https://raw.githubusercontent.com/huiyadanli/RevokeMsgPatcher/master/Images/give_a_star.png)
