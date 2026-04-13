param(
    [string]$BaseUrl = "http://localhost:5173",
    [string]$TenantId = "11111111-1111-1111-1111-111111111111",
    [string]$LogPath = "src/Presentation/logs/studentapi-20260409.log",
    [switch]$AutoStartApi = $true
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$apiProcess = $null
$hadRunningApi = $false

function Test-ApiReachable {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url
    )

    try
    {
        $statusCode = (Invoke-WebRequest -Uri "$Url/openapi/v1.json" -UseBasicParsing -TimeoutSec 5).StatusCode
        return $statusCode -eq 200
    }
    catch
    {
        return $false
    }
}

Push-Location $repoRoot

try
{

$runningApi = Get-Process -Name "StudentApi.Presentation" -ErrorAction SilentlyContinue
if ($null -ne $runningApi)
{
    $hadRunningApi = $true
    Write-Host "Detected running StudentApi.Presentation process(es). Stopping to avoid build file-lock errors..." -ForegroundColor Yellow
    $runningApi | Stop-Process -Force
}

Write-Host "[1/3] Running unit tests..." -ForegroundColor Cyan
dotnet test .\tests\StudentApi.UnitTests\StudentApi.UnitTests.csproj --logger "console;verbosity=minimal"
if ($LASTEXITCODE -ne 0)
{
    throw "Unit tests failed."
}

Write-Host "[2/3] Running integration tests..." -ForegroundColor Cyan
dotnet test .\tests\StudentApi.IntegrationTests\StudentApi.IntegrationTests.csproj --logger "console;verbosity=minimal"
if ($LASTEXITCODE -ne 0)
{
    throw "Integration tests failed."
}

Write-Host "[3/3] Running Redis/log smoke check..." -ForegroundColor Cyan
if (-not (Test-ApiReachable -Url $BaseUrl))
{
    if (-not $AutoStartApi -and -not $hadRunningApi)
    {
        Write-Host "API is not reachable at $BaseUrl and AutoStartApi is disabled. Skipping smoke checks." -ForegroundColor Yellow
        Write-Host "All tests passed (smoke checks skipped)." -ForegroundColor Green
        return
    }

    if (-not $AutoStartApi -and $hadRunningApi)
    {
        Write-Host "API was running before tests. Restarting it for smoke checks..." -ForegroundColor Yellow
    }
    else
    {
        Write-Host "API not reachable. Starting API automatically..." -ForegroundColor Yellow
    }

    $apiStdOut = Join-Path $env:TEMP "studentapi-autostart.stdout.log"
    $apiStdErr = Join-Path $env:TEMP "studentapi-autostart.stderr.log"
    Remove-Item $apiStdOut, $apiStdErr -ErrorAction SilentlyContinue

    $apiEnvironment = @{
        ASPNETCORE_ENVIRONMENT = "Development"
        "Jwt__Key" = "SuperLongLocalDevJwtSecretKey_ChangeMe_123456"
        ASPNETCORE_URLS = $BaseUrl
    }

    $apiProcess = Start-Process -FilePath "dotnet" -ArgumentList @("run", "--no-build", "--no-launch-profile", "--project", ".\src\Presentation\StudentApi.Presentation.csproj") -WorkingDirectory $repoRoot -PassThru -RedirectStandardOutput $apiStdOut -RedirectStandardError $apiStdErr -Environment $apiEnvironment

    $isReady = $false
    for ($i = 0; $i -lt 24; $i++)
    {
        if (Test-ApiReachable -Url $BaseUrl)
        {
            $isReady = $true
            break
        }

        if ($apiProcess.HasExited)
        {
            break
        }

        Start-Sleep -Seconds 2
    }

    if (-not $isReady)
    {
        $stdoutTail = if (Test-Path $apiStdOut) { (Get-Content $apiStdOut -Tail 20) -join [Environment]::NewLine } else { "(no stdout)" }
        $stderrTail = if (Test-Path $apiStdErr) { (Get-Content $apiStdErr -Tail 20) -join [Environment]::NewLine } else { "(no stderr)" }
        Write-Host "API startup stdout (tail):" -ForegroundColor DarkYellow
        Write-Host $stdoutTail
        Write-Host "API startup stderr (tail):" -ForegroundColor DarkYellow
        Write-Host $stderrTail
        throw "API failed to start at $BaseUrl within the expected time window."
    }
}

powershell -NoProfile -ExecutionPolicy Bypass -File .\tests\StudentApi.IntegrationTests\smoke\redis-log-smoke.ps1 -BaseUrl $BaseUrl -TenantId $TenantId -LogPath $LogPath
if ($LASTEXITCODE -ne 0)
{
    throw "Redis/log smoke check failed."
}

Write-Host "All tests passed." -ForegroundColor Green
}
finally
{
    if ($null -ne $apiProcess -and -not $apiProcess.HasExited)
    {
        Stop-Process -Id $apiProcess.Id -Force
    }

    Pop-Location
}
