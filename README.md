
# WeChat Plus 商用工作台

本仓库当前在上游 `huiyadanli/RevokeMsgPatcher` v2.1 基础上新增了 WeChat Plus MVP 开发骨架，用于实现“闭源商用主程序 + 独立开源助手组件”的微信多开工作台。原上游 GPLv3 项目内容保留在下方，新的商用工作台代码放在独立的 `WeChatPlus.sln` 中，避免和原补丁工具混在一个可执行程序里。

## 当前新增内容

- `docs/product/commercial-shell-prd.md`：商用工作台 PRD。
- `docs/product/closed-source-commercial-plan.md`：闭源商用计划书。
- `docs/superpowers/plans/2026-06-26-wechat-plus-mvp.md`：MVP 实施计划。
- `WeChatPlus.sln`：新的 MVP 解决方案。
- `WeChatPlus.Core`：中立核心模型、助手命令契约、本地话术库、试用授权状态。
- `WeChatPlus.OpenHelper`：独立开源助手组件命令行原型，输出 JSON。
- `WeChatPlus.Shell`：闭源商用壳原型，三栏 WinForms 工作台 UI。
- `WeChatPlus.Tests`：无第三方依赖的控制台测试。

## 架构边界

WeChat Plus 的商业化边界按以下方式设计：

- 闭源商用壳只引用 `WeChatPlus.Core` 中新写的中立模型和服务。
- 多开、补丁等 GPL 敏感能力放在 `WeChatPlus.OpenHelper` 独立可执行文件中。
- 商用壳通过进程边界调用助手组件，不复制、不链接 GPLv3 源码。
- 如果后续把 RevokeMsgPatcher 的 GPLv3 多开/补丁实现迁入助手组件，助手组件对应修改源码必须公开，并在产品内展示许可证和源码地址。

当前 `WeChatPlus.OpenHelper` 已提供 JSON CLI 边界和安全的状态/版本命令；`multi-instance start` 会尝试启动本机微信，`close-mutex` 已保留独立助手命令入口，等待后续在开源助手仓库中接入 GPL 合规实现。

## 构建与验证

当前环境没有 `dotnet` SDK 或 Visual Studio MSBuild，已验证可使用系统 .NET Framework MSBuild 构建新的 MVP 解决方案：

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' WeChatPlus.sln /p:Configuration=Debug /p:Platform='Any CPU' /v:minimal
.\WeChatPlus.Tests\bin\Debug\WeChatPlus.Tests.exe
.\WeChatPlus.OpenHelper\bin\Debug\WeChatPlus.OpenHelper.exe version --json
.\WeChatPlus.OpenHelper\bin\Debug\WeChatPlus.OpenHelper.exe multi-instance status
```

运行商用壳：

```powershell
.\WeChatPlus.Shell\bin\Debug\WeChatPlus.Shell.exe
```

注意：直接构建原始 `RevokeMsgPatcher.sln` 在当前机器上会因为缺少对应 Targeting Pack、NuGet 包和旧 MSBuild 编译能力出现错误；新的 MVP 工作集中在 `WeChatPlus.sln`。

## MVP 状态

已完成：

- 三栏工作台 UI：账号栏、微信工作区占位、右侧快捷话术栏、顶部会员/开源声明入口、底部截图入口。
- 本地话术库：默认分类、默认话术、搜索、新增、复制、JSON 导入导出、CSV 导入。
- 试用授权状态：设备哈希、试用期、离线宽限期。
- 助手组件：`version --json`、`multi-instance status`、`patch status --app wechat`。
- 构建输出：商用壳会把独立助手组件复制到自身输出目录，便于进程边界调用。
- 测试：命令解析、JSON 输出、话术种子/搜索、JSON/CSV 导入、试用授权状态。

下一步：

- 在独立开源助手组件中接入真实微信互斥句柄关闭逻辑，并公开对应源码。
- 为商用壳增加微信窗口枚举、聚焦和嵌入/降级聚焦模式。
- 接入真实截图流程。
- 增加授权 API 客户端和安装器。

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
