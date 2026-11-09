# เช็กคอมไพล์ในเครื่อง แล้วคัดลอกมอดไปโฟลเดอร์ Mods ของ WorldBox
# ใช้: powershell -File deploy.ps1
$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$game = "C:\Program Files (x86)\Steam\steamapps\common\worldbox\Mods"

dotnet build "$here\build\check.csproj" -nologo -v q -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { throw "compile check failed" }

New-Item -ItemType Directory -Force $game | Out-Null
Remove-Item -Recurse -Force "$game\NgaodaraPack" -ErrorAction SilentlyContinue
Copy-Item -Recurse "$here\NgaodaraPack" "$game\NgaodaraPack"
Write-Host "deployed to $game\NgaodaraPack"
