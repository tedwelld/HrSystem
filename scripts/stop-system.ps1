$ErrorActionPreference = "SilentlyContinue"
$ports = @(55329, 55330, 4200)
$pids = @()

foreach ($port in $ports) {
    $lines = netstat -ano | findstr ":$port" | findstr LISTENING
    foreach ($line in $lines) {
        $parts = ($line -split "\s+") | Where-Object { $_ -ne "" }
        $pid = $parts[-1]
        if ($pid -match "^[0-9]+$" -and $pid -ne "0") {
            $pids += [int]$pid
        }
    }
}

$pids += (Get-Process HrSystem.Api -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
$pids += (Get-Process node -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
$pids += (Get-Process npm -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
$pids = $pids | Sort-Object -Unique

foreach ($pid in $pids) {
    Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
}

Start-Sleep -Seconds 1
Write-Host "Stopped PIDs: $($pids -join ', ')"
