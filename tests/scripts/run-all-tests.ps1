param(
    [string]$BaseUrl = "http://localhost:5173",
    [string]$TenantId = "11111111-1111-1111-1111-111111111111",
    [string]$LogPath = "src/Presentation/logs/studentapi-20260409.log",
    [switch]$AutoStartApi = $true
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$apiProcess = $null

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
    if (-not $AutoStartApi)
    {
        throw "API is not reachable at $BaseUrl. Start the API before running smoke checks."
    }

    Write-Host "API not reachable. Starting API automatically..." -ForegroundColor Yellow
    $runCommand = "$env:ASPNETCORE_ENVIRONMENT='Development'; $env:Jwt__Key='SuperLongLocalDevJwtSecretKey_ChangeMe_123456'; $env:ASPNETCORE_URLS='$BaseUrl'; dotnet run --project .\src\Presentation\StudentApi.Presentation.csproj"
    $apiProcess = Start-Process -FilePath "pwsh" -ArgumentList "-NoProfile", "-Command", $runCommand -WorkingDirectory $repoRoot -PassThru

    $isReady = $false
    for ($i = 0; $i -lt 24; $i++)
    {
        if (Test-ApiReachable -Url $BaseUrl)
        {
            $isReady = $true
            break
        }

        Start-Sleep -Seconds 2
    }

    if (-not $isReady)
    {
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
