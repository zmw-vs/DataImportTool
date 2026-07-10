<#
.SYNOPSIS
    发布 DataImportTool 新版本到 Gitee 公开发布仓库（自动更新用）。
.DESCRIPTION
    1) 读取项目 AssemblyVersion，校验与 -Version 一致（防更新死循环）
    2) 复制主 exe -> DataImportTool_v<版本>.exe
    3) 计算 MD5，生成无 BOM 的 version.json
    4) 提交并推送到 dataimporttool-release 公开仓库
.PARAMETER Version
    新版本号，如 1.1.0.0（必须与 Properties\AssemblyInfo.cs 的 AssemblyVersion 一致）
.PARAMETER Notes
    更新说明
.PARAMETER BuildConfig
    构建配置：Release（默认）或 Debug
.EXAMPLE
    $env:GITEE_PAT = "你的令牌"
    .\tools\publish-update.ps1 -Version 1.1.0.0 -Notes "新增自动更新功能"
#>
param(
    [Parameter(Mandatory = $true)][string]$Version,
    [string]$Notes = "常规更新",
    [string]$BuildConfig = "Release"
)

$ErrorActionPreference = "Continue"   # git 的换行符警告不应终止脚本
$env:GIT_REDIRECT_STDERR = '2>&1'    # git 自行合并 stderr，避免被包装成错误
$owner = "zmwhelloword"
$repo  = "dataimporttool-release"

# 项目目录：本脚本位于 <项目>\tools\，上一级即项目根
$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Definition
$projectDir = Split-Path -Parent $scriptDir
$buildExe   = Join-Path $projectDir ("bin\{0}\DataImportTool.exe" -f $BuildConfig)

if (-not (Test-Path $buildExe)) {
    Write-Error "找不到构建产物：$buildExe 。请先在 VS 中以 $BuildConfig 编译。"
    exit 1
}

# 版本一致性校验（防止客户端装完仍是旧号 -> 反复提示更新）
$asmPath = Join-Path $projectDir "Properties\AssemblyInfo.cs"
$asmVer  = ""
if (Test-Path $asmPath) {
    $m = Select-String -Path $asmPath -Pattern 'AssemblyVersion\("([0-9.]+)"\)' | Select-Object -First 1
    if ($m) { $asmVer = $m.Matches.Groups[1].Value }
}
if ($asmVer -and $asmVer -ne $Version) {
    Write-Error "版本不一致：AssemblyInfo.cs=$asmVer，参数 Version=$Version。请先修改 AssemblyInfo.cs 的 AssemblyVersion。"
    exit 1
}

# Gitee PAT（绝不硬编码，从环境变量读取）
$giteePat = $env:GITEE_PAT
if (-not $giteePat) { Write-Error "请先设置环境变量 GITEE_PAT（如 `$env:GITEE_PAT='xxxx'）"; exit 1 }

# git 路径探测（优先 cmd\git.exe，避免 mingw64 版在纯净环境制造垃圾目录）
function Find-Git {
    $cmd = Get-Command git -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $fixed = @(
        "C:\Program Files\Git\cmd\git.exe",
        "C:\Program Files (x86)\Git\cmd\git.exe"
    )
    if ($env:LOCALAPPDATA) { $fixed += (Join-Path $env:LOCALAPPDATA "Programs\Git\cmd\git.exe") }
    if ($env:USERPROFILE)  { $fixed += (Join-Path $env:USERPROFILE ".workbuddy\binaries\PortableGit\versions\1.2.0\cmd\git.exe") }
    foreach ($c in $fixed) { if ($c -and (Test-Path $c)) { return $c } }
    return $null
}
$gitExe = Find-Git
if (-not $gitExe) { Write-Error "找不到 Git，请安装或将其加入 PATH。"; exit 1 }
Write-Host "使用 git: $gitExe"

# 发布仓库本地缓存（匿名克隆，PAT 仅用于推送）
$cacheDir = Join-Path $env:TEMP "dataimporttool-release-pub"
$plainUrl = "https://gitee.com/$owner/$repo.git"
$authUrl  = "https://{0}:{1}@gitee.com/{2}/{3}.git" -f $owner, $giteePat, $owner, $repo

if (Test-Path $cacheDir) {
    & $gitExe -C $cacheDir fetch origin 2>&1 | Out-Null
    & $gitExe -C $cacheDir checkout master 2>&1 | Out-Null
    & $gitExe -C $cacheDir reset --hard origin/master 2>&1 | Out-Null
}
else {
    & $gitExe clone $plainUrl $cacheDir 2>&1
    if ($LASTEXITCODE -ne 0) { Write-Error "克隆发布仓库失败"; exit 1 }
}

# 复制主 exe -> 版本化文件名（务必从主 exe 复制，避免滞后副本）
$exeName = "DataImportTool_v$Version.exe"
$destExe = Join-Path $cacheDir $exeName
Copy-Item -Force $buildExe $destExe
$fileSize = (Get-Item $destExe).Length

# 计算 MD5（供客户端校验完整性）
$md5 = (Get-FileHash -Algorithm MD5 $destExe).Hash

# 生成 version.json（无 BOM！用 UTF8Encoding(false)）
$base = "https://gitee.com/$owner/$repo/raw/master"
$json = @"
{
  "version": "$Version",
  "releaseDate": "$(Get-Date -Format 'yyyy-MM-dd')",
  "releaseNotes": "$Notes",
  "downloadUrl": "$base/$exeName",
  "fileSize": $fileSize,
  "fileHash": "$md5",
  "isMandatory": false
}
"@
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Join-Path $cacheDir "version.json"), $json, $utf8NoBom)
Write-Host "已生成 version.json (v$Version, size=$fileSize, md5=$md5)"

# 提交并推送
& $gitExe -C $cacheDir config core.autocrlf false
& $gitExe -C $cacheDir remote set-url origin $authUrl
& $gitExe -C $cacheDir add -A
& $gitExe -C $cacheDir commit -m "发布 v$Version : $Notes" 2>&1
if ($LASTEXITCODE -ne 0) { Write-Host "无变更或提交跳过" }
& $gitExe -C $cacheDir push origin master 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "推送失败"
    & $gitExe -C $cacheDir remote set-url origin $plainUrl
    exit 1
}

# 推送完还原纯文本 remote，避免 PAT 残留于本地配置
& $gitExe -C $cacheDir remote set-url origin $plainUrl
Write-Host "发布完成：v$Version" -ForegroundColor Green
