
$apiUrl = "http://localhost:5044/api/transactions"

# Generate 100 test requests
$requests = 1..100 | ForEach-Object {
    @{
        amount = [math]::Round((Get-Random -Minimum 100 -Maximum 10000) / 100, 2)
        currency = @("USD", "EUR", "ILS")[(Get-Random -Minimum 0 -Maximum 3)]
        status = @("Pending", "Completed", "Failed")[(Get-Random -Minimum 0 -Maximum 3)]
    }
}

$results = @()
$startTime = Get-Date

# Send requests in batches of 20 concurrent jobs
for ($i = 0; $i -lt 100; $i += 20) {
    $batch = $requests | Select-Object -Skip $i -First 20
    $jobs = @()

    foreach ($request in $batch) {
        $body = ConvertTo-Json -InputObject $request -Compress

        $jobs += Start-Job -ScriptBlock {
            param($url, $jsonBody)

            try {
                $response = Invoke-RestMethod `
                    -Uri $url `
                    -Method POST `
                    -ContentType "application/json" `
                    -Body $jsonBody

                [PSCustomObject]@{
                    Success = $true
                    TransactionId = $response.transactionId
                    Error = $null
                }
            }
            catch {
                [PSCustomObject]@{
                    Success = $false
                    TransactionId = $null
                    Error = $_.Exception.Message
                }
            }
        } -ArgumentList $apiUrl, $body
    }

    $jobs | Wait-Job | Out-Null
    $results += Receive-Job -Job $jobs
    $jobs | Remove-Job
}

$duration = ((Get-Date) - $startTime).TotalSeconds

$successCount = @($results | Where-Object { $_.Success }).Count
$failedCount = @($results | Where-Object { -not $_.Success }).Count

Write-Host "`n===== PERFORMANCE TEST RESULTS =====" -ForegroundColor Cyan
Write-Host "Total requests: 100"
Write-Host "Successful: $successCount" -ForegroundColor Green
Write-Host "Failed: $failedCount" -ForegroundColor Red
Write-Host "Duration: $([math]::Round($duration, 2)) seconds"
Write-Host "Requests per second: $([math]::Round(100 / $duration, 2))"

if ($failedCount -gt 0) {
    Write-Host "`nErrors:" -ForegroundColor Yellow
    $results | Where-Object { -not $_.Success } |
        Select-Object -ExpandProperty Error -Unique
}
