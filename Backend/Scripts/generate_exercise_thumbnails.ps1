<#
    Generates a poster JPG for every exercise video served by the API.

    Each video under PhysioLink.API/wwwroot (e.g. videos/<ExerciseId>.mp4) gets a
    sibling <name>.jpg taken from 30% into the clip — far enough in that the
    movement is under way, not the standing start. The admin panel derives the
    thumbnail URL from Exercises.VideoUrl by swapping the extension for .jpg, so
    no database change is needed; just run this after adding videos and commit
    the JPGs with them.

    Requires ffmpeg and ffprobe on PATH.

    Usage (from the repo root):
        powershell -ExecutionPolicy Bypass -File Backend/Scripts/generate_exercise_thumbnails.ps1
        ... -Force    # regenerate thumbnails that already exist
#>
param(
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$wwwroot = Join-Path $PSScriptRoot '..\PhysioLink.API\wwwroot' | Resolve-Path
$videoExtensions = @('.mp4', '.mov', '.m4v', '.webm')

foreach ($tool in @('ffmpeg', 'ffprobe')) {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) {
        throw "$tool was not found on PATH."
    }
}

$videos = Get-ChildItem -Path $wwwroot -Recurse -File |
    Where-Object { $videoExtensions -contains $_.Extension.ToLowerInvariant() }

$created = 0
$skipped = 0
$failed = 0

foreach ($video in $videos) {
    $thumbnail = [System.IO.Path]::ChangeExtension($video.FullName, '.jpg')

    if ((Test-Path $thumbnail) -and -not $Force) {
        $skipped++
        continue
    }

    $duration = & ffprobe -v error -show_entries format=duration -of csv=p=0 $video.FullName
    $seconds = 0.0
    if (-not [double]::TryParse($duration, [System.Globalization.NumberStyles]::Float,
            [System.Globalization.CultureInfo]::InvariantCulture, [ref]$seconds)) {
        Write-Warning "Could not read the duration of $($video.Name); skipped."
        $failed++
        continue
    }
    $seek = ($seconds * 0.3).ToString('0.###', [System.Globalization.CultureInfo]::InvariantCulture)

    # 640px wide keeps each file around 10-20 KB; the cards show it at ~260px.
    & ffmpeg -v error -y -ss $seek -i $video.FullName -frames:v 1 -vf 'scale=640:-2' -q:v 4 $thumbnail
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path $thumbnail)) {
        Write-Warning "ffmpeg failed for $($video.Name)."
        $failed++
        continue
    }

    $created++
}

Write-Host "Thumbnails: $created created, $skipped already present, $failed failed."
