# Define the HtmlDecode function
function HtmlDecode {
    param ([string]$inputString)
    $output = $inputString -replace '&amp;', '&'
    $output = $output -replace '&lt;', '<'
    $output = $output -replace '&gt;', '>'
    $output = $output -replace '&quot;', '"'
    $output = $output -replace '&#39;', "'"
    return $output
}

function Decode-Xml {
    param ([string]$inputString)
    $encodedString = $inputString -replace '\\\\', '\'
    $encodedString = $encodedString -replace '\\_', '_'
    $encodedString = HtmlDecode -inputString $encodedString
    return $encodedString
}

# Define the function to sanitize file names
function Sanitize-FileName {
    param ([string]$fileName)
    $safeFileName = $fileName -replace '/', '%'
    $invalidChars = [System.IO.Path]::GetInvalidFileNameChars()
    $safeFileName = $safeFileName -replace "[$([Regex]::Escape($invalidChars -join ''))]", ''
    return $safeFileName
}

# Get the list of .md files starting with "0-"
$mdFiles = Get-ChildItem -Path . -Filter "0-*.md"

# Check if there is exactly one such file
if ($mdFiles.Count -ne 1) {
    throw "There must be exactly one .md file starting with '0-'. Found: $($mdFiles.Count)"
}

# Read the content of the file
$file = $mdFiles[0]
$fileContent = Get-Content -Path $file.FullName

# Find the first non-empty line
$firstNonEmptyLine = $fileContent | Where-Object { $_.Trim() -ne '' } | Select-Object -First 1

# Check if the first non-empty line starts with "# "
if ($firstNonEmptyLine -match '^# (.*)') {
    $title = $matches[1]

    # Html decode the title
    $decodedTitle = Decode-Xml $title

    # Sanitize the title to make it a valid file name
    $safeTitle = Sanitize-FileName $decodedTitle

    # Rename the .md file
    $newFileName = "0-$safeTitle.md"
    Rename-Item -Path $file.FullName -NewName $newFileName

    # Get the current directory and its parent directory
    $currentDir = Get-Location
    # Get the parent directory
    $parentDirPath = [System.IO.Path]::GetDirectoryName($currentDir.Path)
    if ($null -eq $parentDirPath -or $parentDirPath -eq '') { throw "Could not determine the parent directory." }
    $parentDir = Get-Item -Path $parentDirPath

    # if ($null -eq $parentDir) { throw "Could not determine the parent directory." }
    # Get the name of the current directory
    $currentDirName = Split-Path -Leaf $currentDir

    # Get the name of the current directory
    # $currentDirName = $currentDir.Name
    
    # Find the subfolder in the parent directory
    $matchingDirs = Get-ChildItem -Path $parentDir -Directory | Where-Object { $_.Name -like "$currentDirName-*" }

    # Check if there is exactly one matching subfolder
    if ($matchingDirs.Count -ne 1) {
        throw "There must be exactly one matching subfolder in the parent directory. Found: $($matchingDirs.Count)"
    }

    # Rename the matching subfolder
    $newDirName = "$currentDirName-$safeTitle"

    if ($newDirName -ne $matchingDirs[0].Name) {
        Rename-Item -Path $matchingDirs[0].FullName -NewName $newDirName
    } else {
        Write-Host "The directory already has the new name: $newDirName"
    }
    
    $keepFilePath = Join-Path -Path $parentDir.FullName -ChildPath "$newDirName\.keep"
    Set-Content -Path $keepFilePath -Value $title
} else {
    throw "The first non-empty line of the .md file must start with '# '."
}
