# WeChat Plus MVP Runtime Package Manifest

This manifest describes the minimum runtime package for the WeChat Plus MVP. It is not an installer yet; it is the verified folder layout produced from `WeChatPlus.Shell\bin\<Configuration>`.

## Required Files

| File | Role | Notes |
| --- | --- | --- |
| `WeChatPlus.Shell.exe` | ClosedSourceShell | Commercial workbench UI and user workflow orchestration. |
| `WeChatPlus.Core.dll` | NeutralCore | Fresh neutral DTOs, repositories, contracts, and local services. |
| `WeChatPlus.OpenHelper.exe` | OpenHelper | Independent helper executable for WeChat process/window/multi-instance operations. |
| `LICENSE` | OpenSourceLicense | GPLv3 license from the upstream project; shipped for open-source component compliance. |
| `README.md` | RuntimeGuide | Project structure, build commands, GPL boundary, and run instructions. |
| `components.json` | OpenSourceNotice | Shipped with the runtime package and copied to the app data folder on first run by `ComponentRepository`; exposed in the open-source notice UI. |
| `update-manifest.json` | UpdateManifest | Local placeholder manifest for product/helper version checks; replace with a downloaded cloud manifest before commercial release. |

## Boundary Rules

- `WeChatPlus.Shell.exe` must not copy, link, or compile GPLv3 implementation code.
- GPL-sensitive operations remain in `WeChatPlus.OpenHelper.exe` and are invoked through CLI/JSON process boundaries.
- Any helper implementation that directly derives from GPLv3 upstream code must remain source-available with its license and source URL shown in the product.

## Current Build Behavior

`WeChatPlus.Shell.csproj` copies the helper executable, neutral core DLL, root `LICENSE`, root `README.md`, packaged `components.json`, and `update-manifest.json` into the Shell output directory after build. On first run, `ComponentRepository` copies the packaged open-source component declaration into the local data directory so users can view it from the app. The Shell can read `update-manifest.json` from the runtime folder to show product/helper update status without embedding any real update secret.

`ReleasePackageValidator` checks the runtime folder against this manifest and reports missing required files. The Settings page displays the validation summary and missing file list so a user can diagnose an incomplete runtime folder or a deleted helper component without crashing the closed-source shell.
