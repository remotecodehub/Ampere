[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, Position=0, HelpMessage="Migration name")]
    [Alias("n")][string]$Name,
    [Parameter(Mandatory=$false, Position=1, HelpMessage="Startup Project")]
    [Alias("s")][string]$StartupProject = "src/web/Ampere",
    [Parameter(Mandatory=$false, Position=2, HelpMessage="Migration Project")]
    [Alias("p")][string]$Project = "src/web/Ampere.Infrastructure",
    [Parameter(Mandatory=$false, Position=3, HelpMessage="Migration Output Folder")]
    [Alias("d")][string]$MigrationsFolder = "src/web/Ampere.Infrastructure/Persistence/Migrations"
)
$RepoRoot = (Get-Item "$PSScriptRoot\..").FullName
Push-Location $RepoRoot
try {
    Write-Host "Adding migration '$Name'" -ForegroundColor Cyan
    dotnet ef migrations add $Name --project $Project --startup-project $StartupProject --output-dir $MigrationsFolder
}
finally {
    Pop-Location
}