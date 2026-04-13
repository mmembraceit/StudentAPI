param(
    [string]$BaseUrl = "http://localhost:5173",
    [string]$TenantId = "11111111-1111-1111-1111-111111111111",
    [string]$LogPath = "src/Presentation/logs/studentapi-20260409.log"
)

$ErrorActionPreference = "Stop"

Write-Host "[1/5] Login"
$loginBody = @{ username = "admin"; password = "admin123" } | ConvertTo-Json
$login = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/auth/login" -ContentType "application/json" -Body $loginBody
$token = $login.data.accessToken
$headers = @{ Authorization = "Bearer $token" }

Write-Host "[2/5] Force Redis MISS then SET on tenant list"
Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/students" -Headers $headers -ContentType "application/json" -Body (@{
    tenantId = $TenantId
    name = "redis-smoke-$(Get-Date -Format HHmmss)"
    dateOfBirth = "2001-01-01"
} | ConvertTo-Json) | Out-Null

Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/students?tenantId=$TenantId" -Headers $headers | Out-Null

Write-Host "[3/5] Force Redis HIT on tenant list"
Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/students?tenantId=$TenantId" -Headers $headers | Out-Null

Write-Host "[4/5] Check log contains key events"
$log = Get-Content $LogPath -Raw
$requiredPatterns = @(
    "REDIS MISS students:tenant:$TenantId:all",
    "REDIS HIT students:tenant:$TenantId:all",
    "HTTP GET /api/students responded 200"
)

$missing = @()
foreach ($pattern in $requiredPatterns)
{
    if ($log -notmatch [regex]::Escape($pattern))
    {
        $missing += $pattern
    }
}

if ($missing.Count -gt 0)
{
    Write-Host "Missing patterns:" -ForegroundColor Red
    $missing | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    throw "Redis/log smoke check failed."
}

Write-Host "[5/5] Smoke check passed" -ForegroundColor Green
