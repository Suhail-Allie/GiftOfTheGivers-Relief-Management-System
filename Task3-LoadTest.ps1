param(
    [int]$VirtualUsers = 20,
    [int]$IterationsPerUser = 10,
    [string]$BaseUrl = "http://localhost:5080"
)

$ErrorActionPreference = "Stop"

# Verify that the site is available before starting.
try {
    $check = Invoke-WebRequest -Uri "$BaseUrl/" -UseBasicParsing -TimeoutSec 10
    if ([int]$check.StatusCode -ne 200) {
        throw "Home page returned HTTP $([int]$check.StatusCode)"
    }
} catch {
    Write-Host "Could not reach the local website at $BaseUrl"
    Write-Host $_.Exception.Message
    exit 1
}

$evidenceFolder = Join-Path $PSScriptRoot "Task3-Evidence"
New-Item -ItemType Directory -Path $evidenceFolder -Force | Out-Null

$runId = Get-Date -Format "yyyyMMdd_HHmmss"
$csvPath = Join-Path $evidenceFolder "LoadTest_$runId.csv"
$summaryPath = Join-Path $evidenceFolder "LoadTest_$runId.txt"

Write-Host "Starting load test: $VirtualUsers virtual users x $IterationsPerUser requests."
Write-Host "Target: $BaseUrl"

$jobs = @()

for ($userId = 1; $userId -le $VirtualUsers; $userId++) {
    $jobs += Start-Job -ArgumentList $BaseUrl, $userId, $IterationsPerUser -ScriptBlock {
        param($BaseUrl, $UserId, $Iterations)

        $routes = @(
            "/",
            "/Account/Login",
            "/Volunteer/Index",
            "/Donation/Index"
        )

        for ($i = 1; $i -le $Iterations; $i++) {
            $route = $routes[($i - 1) % $routes.Count]
            $url = "$BaseUrl$route"
            $started = [DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds()
            $timer = [Diagnostics.Stopwatch]::StartNew()
            $statusCode = 0
            $success = $false
            $errorText = ""

            try {
                $response = Invoke-WebRequest `
                    -Uri $url `
                    -UseBasicParsing `
                    -TimeoutSec 30

                $statusCode = [int]$response.StatusCode
                $success = ($statusCode -ge 200 -and $statusCode -lt 400)
            } catch {
                if ($_.Exception.Response) {
                    try {
                        $statusCode = [int]$_.Exception.Response.StatusCode
                    } catch {}
                }
                $errorText = $_.Exception.Message
            } finally {
                $timer.Stop()
            }

            [PSCustomObject]@{
                VirtualUser    = $UserId
                Iteration      = $i
                Route          = $route
                StatusCode     = $statusCode
                Success        = $success
                ResponseTimeMs = [Math]::Round($timer.Elapsed.TotalMilliseconds, 2)
                StartedEpochMs = $started
                Error          = $errorText
            }

            if ($i -lt $Iterations) {
                Start-Sleep -Milliseconds 100
            }
        }
    }
}

$results = @()

try {
    $null = Wait-Job -Job $jobs

    foreach ($job in $jobs) {
        $results += @(Receive-Job -Job $job)
        if ($job.State -ne "Completed") {
            Write-Warning "Load-test job $($job.Id) ended in state $($job.State)."
        }
    }
} finally {
    foreach ($job in $jobs) {
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
    }
}

if ($results.Count -eq 0) {
    throw "The test produced no request results. Check the job errors and rerun."
}

$results | Export-Csv -Path $csvPath -NoTypeInformation -Encoding UTF8

$times = @($results | ForEach-Object { [double]$_.ResponseTimeMs } | Sort-Object)
$average = ($times | Measure-Object -Average).Average
$p50Index = [Math]::Max(0, [Math]::Ceiling(0.50 * $times.Count) - 1)
$p95Index = [Math]::Max(0, [Math]::Ceiling(0.95 * $times.Count) - 1)
$p50 = $times[$p50Index]
$p95 = $times[$p95Index]

$successCount = @($results | Where-Object { $_.Success -eq $true }).Count
$failureCount = $results.Count - $successCount

$startMs = ($results | Measure-Object -Property StartedEpochMs -Minimum).Minimum
$endMs = ($results | ForEach-Object {
    [double]$_.StartedEpochMs + [double]$_.ResponseTimeMs
} | Measure-Object -Maximum).Maximum

$durationSeconds = [Math]::Max(0.001, ($endMs - $startMs) / 1000)
$throughput = $results.Count / $durationSeconds

$summary = @(
    "Local website load test"
    "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    "Target: $BaseUrl"
    "Virtual users: $VirtualUsers"
    "Iterations per user: $IterationsPerUser"
    "Total requests: $($results.Count)"
    "Successful requests: $successCount"
    "Failed requests: $failureCount"
    "Average response time (ms): $([Math]::Round($average, 2))"
    "Median / P50 response time (ms): $([Math]::Round($p50, 2))"
    "P95 response time (ms): $([Math]::Round($p95, 2))"
    "Observed throughput (requests/second): $([Math]::Round($throughput, 2))"
    "Routes: /, /Account/Login, /Volunteer/Index, /Donation/Index"
    "CSV results: $csvPath"
)

$summary | Set-Content -Path $summaryPath -Encoding UTF8
$summary | ForEach-Object { Write-Host $_ }

if ($failureCount -gt 0) {
    Write-Warning "One or more requests failed. Review the CSV and application logs."
}
