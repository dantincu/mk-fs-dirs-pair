param(
    [string]$arg1,
    [string]$arg2
)

function HtmlEncode {
    param ([string]$inputString)
    $output = $inputString -replace '&', '&amp;'
    $output = $output -replace '<', '&lt;'
    $output = $output -replace '>', '&gt;'
    $output = $output -replace '"', '&quot;'
    $output = $output -replace "'", '&#39;'
    return $output
}

function Encode-Xml {
    param ([string]$inputString)
    $encodedString = HtmlEncode -input $inputString
    $encodedString = $encodedString -replace '\\', '\\'
    $encodedString = $encodedString -replace '_', '\_'
    return $encodedString
}

function Sanitize-FileName {
    param ([string]$fileName)
    $safeFileName = $fileName -replace '/', '%'
    $invalidChars = [System.IO.Path]::GetInvalidFileNameChars()
    $safeFileName = $safeFileName -replace "[$([Regex]::Escape($invalidChars -join ''))]", ''
    return $safeFileName
}

$title = $arg1
$shortDirName = $arg2
$fullDirNamePart = Sanitize-FileName -fileName $title

$dir1 = $shortDirName
$dir2 = "${shortDirName}-${fullDirNamePart}"

New-Item -Path . -Name $dir1 -ItemType Directory -Force
New-Item -Path . -Name $dir2 -ItemType Directory -Force

$fileName = "0-${fullDirNamePart}.md"
$fileContent = Encode-Xml -inputString $title
$fileContent = "# " + $fileContent

Set-Content -Path ".\$dir1\$fileName" -Value $fileContent
Set-Content -Path ".\$dir2\.keep" -Value $title