param(
    [Parameter(Mandatory = $true)]
    [string]$PlansDirectory
)

# Doctor Plans - Validate all plans and report issues
# Output format: Id | Name | Status | YamlOk | Worktrees | Errors

$ErrorActionPreference = 'SilentlyContinue'

$results = @()

# Get all plan folders
$planFolders = Get-ChildItem -Path $PlansDirectory -Directory | Where-Object { $_.Name -match '^\d{5}-' }

foreach ($folder in $planFolders) {
    $planId = $folder.Name.Substring(0, 5)
    $yamlPath = Join-Path $folder.FullName "plan.yaml"
    $worktreesPath = Join-Path $folder.FullName "worktrees"

    $errors = @()
    $yamlOk = "✓"
    $title = ""
    $state = ""
    $worktreeCount = 0

    # Check if plan.yaml exists
    if (-not (Test-Path $yamlPath)) {
        $yamlOk = "✗"
        $errors += "Missing plan.yaml"
    } else {
        # Try to parse yaml
        try {
            $yamlContent = Get-Content $yamlPath -Raw -ErrorAction Stop

            # Extract title
            if ($yamlContent -match '(?m)^title:\s*"?([^"\n]+)"?') {
                $title = $Matches[1].Trim()
            } else {
                $title = "<no title>"
                $errors += "Missing title"
            }

            # Extract state
            if ($yamlContent -match '(?m)^state:\s*(.+)') {
                $state = $Matches[1].Trim()
            } else {
                $state = "<no state>"
                $errors += "Missing state"
            }

            # Validate yaml structure
            if ($yamlContent -notmatch '(?m)^project:') { $errors += "Missing project" }
            if ($yamlContent -notmatch '(?m)^repos:') { $errors += "Missing repos" }
            if ($yamlContent -notmatch '(?m)^created:') { $errors += "Missing created" }

        } catch {
            $yamlOk = "✗"
            $errors += "Invalid YAML: $($_.Exception.Message)"
            $title = "<parse error>"
            $state = "<parse error>"
        }
    }

    # Check worktrees
    if (Test-Path $worktreesPath) {
        $worktreeDirs = Get-ChildItem -Path $worktreesPath -Directory -ErrorAction SilentlyContinue
        $worktreeCount = $worktreeDirs.Count

        # Check for nested worktrees (worktrees inside worktrees) - [FORCE] ERROR
        foreach ($wtDir in $worktreeDirs) {
            $nestedWorktrees = Join-Path $wtDir.FullName "worktrees"
            if (Test-Path $nestedWorktrees) {
                $nestedCount = (Get-ChildItem -Path $nestedWorktrees -Directory -ErrorAction SilentlyContinue).Count
                if ($nestedCount -gt 0) {
                    $errors += "[FORCE] Nested worktrees in $($wtDir.Name)"
                }
            }

            # Check for .git worktree files inside the worktree (indicates git operations happened inside worktree)
            $gitWorktreeFile = Join-Path $wtDir.FullName ".git"
            if ((Test-Path $gitWorktreeFile) -and (Get-Content $gitWorktreeFile -Raw -ErrorAction SilentlyContinue) -match 'gitdir:.*worktrees') {
                $errors += "[FORCE] Git worktree created inside $($wtDir.Name)"
            }
        }
    }

    # Check for orphaned worktrees (worktrees folder exists but is empty)
    if ((Test-Path $worktreesPath) -and $worktreeCount -eq 0) {
        $errors += "Empty worktrees directory"
    }

    # Check for revisions
    $revisionsPath = Join-Path $folder.FullName "revisions"
    if (-not (Test-Path $revisionsPath)) {
        $errors += "Missing revisions folder"
    } else {
        $revisionFiles = Get-ChildItem -Path $revisionsPath -File -Filter "*.md" -ErrorAction SilentlyContinue
        if ($revisionFiles.Count -eq 0) {
            $errors += "No revision files"
        }
    }

    # Truncate long titles
    if ($title.Length -gt 50) {
        $title = $title.Substring(0, 47) + "..."
    }

    $errorStr = if ($errors.Count -gt 0) { $errors -join "; " } else { "-" }

    $results += [PSCustomObject]@{
        Id = $planId
        Title = $title
        State = $state
        YamlOk = $yamlOk
        Worktrees = $worktreeCount
        Errors = $errorStr
    }
}

# Output as formatted table
$results | Format-Table -AutoSize -Property Id, Title, State, YamlOk, Worktrees, Errors

# Summary
Write-Host "`nSummary:" -ForegroundColor Cyan
Write-Host "Total plans: $($results.Count)"
$withErrors = ($results | Where-Object { $_.Errors -ne "-" }).Count
Write-Host "Plans with errors: $withErrors"
$withForceErrors = ($results | Where-Object { $_.Errors -match '\[FORCE\]' }).Count
if ($withForceErrors -gt 0) {
    Write-Host "Plans with [FORCE] errors (nested worktrees): $withForceErrors" -ForegroundColor Red
}
$invalidYaml = ($results | Where-Object { $_.YamlOk -eq "✗" }).Count
if ($invalidYaml -gt 0) {
    Write-Host "Plans with invalid YAML: $invalidYaml" -ForegroundColor Yellow
}
