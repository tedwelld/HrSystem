param(
    [string]$ApiBaseUrl = "http://localhost:55330/api"
)

$ErrorActionPreference = "Stop"

function Invoke-JsonPost {
    param(
        [string]$Url,
        [hashtable]$Body,
        [hashtable]$Headers
    )

    $json = $Body | ConvertTo-Json -Depth 8
    if ($null -ne $Headers) {
        return Invoke-RestMethod -Uri $Url -Method Post -Headers $Headers -ContentType "application/json" -Body $json
    }

    return Invoke-RestMethod -Uri $Url -Method Post -ContentType "application/json" -Body $json
}

Write-Host "Validating HrSystem API at $ApiBaseUrl" -ForegroundColor Cyan

$adminLogin = Invoke-JsonPost -Url "$ApiBaseUrl/auth/login" -Body @{
    email = "admin@hrsystem.com"
    password = "Admin@HrSystem2026!"
}

$candidateLogin = Invoke-JsonPost -Url "$ApiBaseUrl/auth/login" -Body @{
    email = "john.candidate@hrsytem.com"
    password = "User@12345"
}

$adminHeaders = @{ Authorization = "Bearer $($adminLogin.token)" }
$candidateHeaders = @{ Authorization = "Bearer $($candidateLogin.token)" }

$cv = Invoke-JsonPost -Url "$ApiBaseUrl/cv/structured" -Headers $candidateHeaders -Body @{
    fileName = "john-validation.json"
    fullText = "C# .NET SQL REST API Angular 4 years"
    skills = @("c#", "dotnet", "sql", "angular", "rest api")
    educationSummary = "BSc Computer Science"
    yearsOfExperience = 4
    certificationsSummary = "Azure Certified"
}

$now = Get-Date -Format "yyyyMMddHHmmss"
$job = Invoke-JsonPost -Url "$ApiBaseUrl/jobs" -Headers $adminHeaders -Body @{
    title = "Validation Backend Engineer $now"
    description = "Validation job posting for automated system check"
    location = "Remote"
    employmentType = "Full-time"
    experienceLevel = "Mid"
    requiredSkills = @("c#", "dotnet", "sql", "rest api")
    salaryMin = 90000
    salaryMax = 120000
    companyId = 1
}

$app = Invoke-JsonPost -Url "$ApiBaseUrl/applications" -Headers $candidateHeaders -Body @{
    jobPostingId = $job.id
    cvProfileId = $cv.id
    coverLetter = "Automated validation application"
}

Invoke-JsonPost -Url "$ApiBaseUrl/applications/admin/update-stage" -Headers $adminHeaders -Body @{
    applicationId = $app.id
    stage = "UnderReview"
} | Out-Null

$followUp = Invoke-JsonPost -Url "$ApiBaseUrl/applications/admin/follow-up" -Headers $adminHeaders -Body @{
    applicationId = $app.id
    note = "Automated validation follow-up"
}

$adminDash = Invoke-RestMethod -Uri "$ApiBaseUrl/dashboard/admin" -Headers $adminHeaders
$candidateDash = Invoke-RestMethod -Uri "$ApiBaseUrl/dashboard/candidate" -Headers $candidateHeaders
$notifications = Invoke-RestMethod -Uri "$ApiBaseUrl/notifications/mine" -Headers $candidateHeaders
$openJobs = Invoke-RestMethod -Uri "$ApiBaseUrl/jobs/open" -Headers $candidateHeaders

Write-Host "Validation passed." -ForegroundColor Green
Write-Host "Admin: $($adminLogin.user.email)"
Write-Host "Candidate: $($candidateLogin.user.email)"
Write-Host "CV ID: $($cv.id)"
Write-Host "Job ID: $($job.id)"
Write-Host "Application ID: $($app.id)"
Write-Host "Follow-up ID: $($followUp.id)"
Write-Host "Admin open jobs: $($adminDash.openJobPostings)"
Write-Host "Candidate apps: $($candidateDash.myApplications)"
Write-Host "Candidate notifications: $($notifications.Count)"
Write-Host "Open jobs endpoint count: $($openJobs.Count)"
