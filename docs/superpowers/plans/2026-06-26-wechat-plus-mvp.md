# WeChat Plus MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a verified MVP foundation for the Windows WeChat multi-account commercial workbench while preserving the closed-shell and open-helper boundary.

**Architecture:** Add a separate `WeChatPlus.sln` so the new workbench can build in this environment without depending on the original `RevokeMsgPatcher.sln` build state. The closed-source shell references only freshly-authored neutral core contracts and services; the GPL-sensitive helper is a separate executable invoked by process boundary.

**Tech Stack:** .NET Framework 4.0-compatible C#, WinForms, `System.Web.Script.Serialization` JSON, file-based JSON local storage, console test harness.

---

## File Structure

- Create `WeChatPlus.sln`: solution containing the new MVP projects.
- Create `WeChatPlus.Core/WeChatPlus.Core.csproj`: neutral contracts, models, local data services, helper process client, trial license logic.
- Create `WeChatPlus.Tests/WeChatPlus.Tests.csproj`: console test harness with no external dependencies.
- Create `WeChatPlus.OpenHelper/WeChatPlus.OpenHelper.csproj`: independent helper executable with JSON CLI commands.
- Create `WeChatPlus.Shell/WeChatPlus.Shell.csproj`: commercial shell prototype with three-column WinForms UI.
- Modify `README.md`: document project structure, build commands, GPL boundary, and next steps.

## Task 1: Contract and Local Data Tests

**Files:**
- Create: `WeChatPlus.Tests/Program.cs`
- Create: `WeChatPlus.Tests/WeChatPlus.Tests.csproj`
- Create: `WeChatPlus.sln`

- [ ] **Step 1: Write tests first**

Add tests that require:

- `HelperCommandParser` parses `version --json`, `multi-instance start`, and `multi-instance close-mutex --pid 1234`.
- `JsonResultWriter` serializes a success result containing `ok` and `command`.
- `QuickReplyRepository` creates default categories and can search seeded replies.
- `TrialLicenseService` creates a trial with plan `trial` and a non-empty device hash.

- [ ] **Step 2: Run test build to verify RED**

Run:

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' WeChatPlus.sln /p:Configuration=Debug /p:Platform='Any CPU' /v:minimal
```

Expected: FAIL because `WeChatPlus.Core` and the tested classes do not exist yet.

- [ ] **Step 3: Implement minimal core**

Create:

- `WeChatPlus.Core/Contracts/HelperCommand.cs`
- `WeChatPlus.Core/Contracts/HelperCommandResult.cs`
- `WeChatPlus.Core/Contracts/HelperCommandParser.cs`
- `WeChatPlus.Core/Contracts/JsonResultWriter.cs`
- `WeChatPlus.Core/Models/AccountRecord.cs`
- `WeChatPlus.Core/Models/QuickReply.cs`
- `WeChatPlus.Core/Models/ReplyCategory.cs`
- `WeChatPlus.Core/Models/LicenseState.cs`
- `WeChatPlus.Core/Models/OpenSourceComponent.cs`
- `WeChatPlus.Core/Services/AppPaths.cs`
- `WeChatPlus.Core/Services/QuickReplyRepository.cs`
- `WeChatPlus.Core/Services/TrialLicenseService.cs`
- `WeChatPlus.Core/WeChatPlus.Core.csproj`

- [ ] **Step 4: Run tests to verify GREEN**

Run the same MSBuild command, then run:

```powershell
.\WeChatPlus.Tests\bin\Debug\WeChatPlus.Tests.exe
```

Expected: build exit code 0 and test harness prints passed tests.

## Task 2: Open Helper CLI

**Files:**
- Create: `WeChatPlus.OpenHelper/Program.cs`
- Create: `WeChatPlus.OpenHelper/MultiInstance/WeChatProcessService.cs`
- Create: `WeChatPlus.OpenHelper/WeChatPlus.OpenHelper.csproj`
- Modify: `WeChatPlus.sln`

- [ ] **Step 1: Implement helper as separate executable**

Support commands:

- `version --json`
- `multi-instance status`
- `multi-instance start`
- `multi-instance close-mutex --pid <pid>`
- `patch status --app wechat`

All commands must output JSON through `JsonResultWriter`.

- [ ] **Step 2: Verify helper**

Run:

```powershell
.\WeChatPlus.OpenHelper\bin\Debug\WeChatPlus.OpenHelper.exe version --json
.\WeChatPlus.OpenHelper\bin\Debug\WeChatPlus.OpenHelper.exe multi-instance status
.\WeChatPlus.OpenHelper\bin\Debug\WeChatPlus.OpenHelper.exe patch status --app wechat
```

Expected: valid JSON for each command.

## Task 3: Commercial Shell Prototype

**Files:**
- Create: `WeChatPlus.Shell/Program.cs`
- Create: `WeChatPlus.Shell/MainForm.cs`
- Create: `WeChatPlus.Shell/Properties/AssemblyInfo.cs`
- Create: `WeChatPlus.Shell/WeChatPlus.Shell.csproj`
- Modify: `WeChatPlus.sln`

- [ ] **Step 1: Build three-column UI**

The form must include:

- top bar with product name, helper status, member entry, open-source notice entry.
- left account rail with add account, privacy lock, split window buttons.
- center WeChat workspace placeholder.
- right quick reply panel with search, category list, reply list, import/export/edit buttons.
- bottom screenshot controls.

- [ ] **Step 2: Wire MVP behavior**

Behavior:

- Add account invokes helper `multi-instance start`.
- Quick reply search reads from `QuickReplyRepository`.
- Double-click reply copies reply content to clipboard.
- Member entry shows trial state.
- Open-source notice shows GPLv3 helper boundary and source URL.

- [ ] **Step 3: Verify shell build**

Run:

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' WeChatPlus.sln /p:Configuration=Debug /p:Platform='Any CPU' /v:minimal
```

Expected: all new projects build.

## Task 4: README and Completion Evidence

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Document**

Add sections:

- `WeChat Plus 商用工作台`
- project structure
- build and run commands
- closed-source shell versus GPL helper boundary
- current MVP status
- next development tasks

- [ ] **Step 2: Verify**

Run:

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe' WeChatPlus.sln /p:Configuration=Debug /p:Platform='Any CPU' /v:minimal
.\WeChatPlus.Tests\bin\Debug\WeChatPlus.Tests.exe
.\WeChatPlus.OpenHelper\bin\Debug\WeChatPlus.OpenHelper.exe version --json
```

Expected: build and tests pass; helper outputs JSON.
