[CmdletBinding()]
param(
    [Parameter(Mandatory=$true, Position=0, HelpMessage="Number of migrations to revert")]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$Count,
    [Parameter(Mandatory=$false, Position=1, HelpMessage="Startup project")]
    [string]$StartupProject = "src/web/Ampere",
    [Parameter(Mandatory=$false, Position=2, HelpMessage="Migrations project")]
    [string]$Project = "src/web/Ampere.Infrastructure"
)

$RepoRoot = (Get-Item "$PSScriptRoot\..").FullName

Push-Location $RepoRoot

try {
    $MigrationsFolder = Join-Path $RepoRoot (Join-Path $Project "Persistence/Migrations")

    if (-not (Test-Path $MigrationsFolder)) {
        Write-Error "Migrations folder was not located at: $MigrationsFolder"
        return
    }

    $MigrationFiles = Get-ChildItem -Path $MigrationsFolder -Filter "*.cs" | 
                      Where-Object { $_.Name -notlike "*.Designer.cs" -and $_.Name -notlike "*Snapshot.cs" } | 
                      Sort-Object Name

    $TotalMigrations = $MigrationFiles.Count

    # Validação da quantidade solicitada
    if ($Count -gt $TotalMigrations) {
        Write-Host "ERROR: It was requested the rollback of $Count migration(s), but there are only $TotalMigrations migration(s) present in project." -ForegroundColor Red
        Write-Host "Revert cancelled." -ForegroundColor Yellow
        return
    }

    Write-Host "Number of migrations found: $TotalMigrations" -ForegroundColor Cyan
    Write-Host "Starting the rollback of $Count migration(s)..." -ForegroundColor Yellow

    # Target Migration: a migration para a qual o banco deve ser revertido antes de remover
    $TargetIndex = $TotalMigrations - $Count - 1

    if ($TargetIndex -ge 0) {
        # Extrai o nome da migration de destino para atualizar a base de dados
        # Exemplo de arquivo: 20260819120000_MinhaMigration.cs -> extrai '20260819120000_MinhaMigration'
        $TargetMigrationName = $MigrationFiles[$TargetIndex].BaseName
        Write-Host "Updating database for the migration state: $TargetMigrationName" -ForegroundColor Cyan
        dotnet ef database update $TargetMigrationName --project $Project --startup-project $StartupProject
    }
    else {
        # Se for remover TODAS as migrations existentes, atualiza a base para o estado inicial (0)
        Write-Host "Reverting all migrations from database (0 state)..." -ForegroundColor Cyan
        dotnet ef database update 0 --project $Project --startup-project $StartupProject
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Error "An error happened when revering database. The process was terminated."
        return
    }

    # Desempilha e remove os arquivos de migration um por um
    for ($i = 0; $i -lt $Count; $i++) {
        $CurrentRollbackNumber = $i + 1
        Write-Host "Reverting migration [$CurrentRollbackNumber/$Count]..." -ForegroundColor LightMagenta
        dotnet ef migrations remove --project $Project --startup-project $StartupProject --force

        if ($LASTEXITCODE -ne 0) {
            Write-Error "Error reverting migration at step $CurrentRollbackNumber."
            break
        }
    }

    Write-Host "Rollback successfully finished!" -ForegroundColor Green
}
finally {
    Pop-Location
}