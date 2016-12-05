Properties {
	### Directories
	$base_dir = resolve-path .
    $build_dir = "$base_dir\build"
    $src_dir = "$base_dir\src"
    $tests_dir = "$base_dir\tests"
    $package_dir = "$base_dir\packages"
    $nuspec_dir = "$base_dir\nuspecs"
    $temp_dir = "$build_dir\temp"
    $framework_dir =  $env:windir + "\Microsoft.Net\Framework\v4.0.30319"

	### Tools
    $nuget = "$base_dir\.nuget\nuget.exe"
    $7zip = "$package_dir\7-Zip.CommandLine.*\tools\7za.exe"

	### Project information
    $solution = "anybar-sharp.sln"
	$config = "Release"    
	$solution_path = "$base_dir\$solution"	
}

Task Default -Depends Pack

### Tasks
Task Collect -Depends Clean -Description "Copy all artifacts to the build folder." {
	Collect-Assembly "AnyBar" "net451"
	Collect-Assembly "AnyBar" "netstandard1.3"
}

Task Pack -Depends Collect -Description "Create NuGet packages and archive files." {
    $version = Get-PackageVersion
	Create-Package "AnyBar" $version
}


Task Restore -Description "Restore NuGet packages for solution." {
    Write-Host "Restoring NuGet packages for '$solution'..." -ForegroundColor "Green"
    Exec { .$nuget restore $solution }
}

Task Clean -Description "Clean up build and project folders." {
    Clean-Directory $build_dir

    if ($solution) {
        Write-Host "Cleaning up '$solution'..." -ForegroundColor "Green"
        Exec { msbuild $solution_path /target:Clean /nologo /verbosity:minimal }
    }
}

Task Compile -Depends Clean, Restore -Description "Compile all the projects in a solution." {
    Write-Host "Compiling '$solution'..." -ForegroundColor "Green"
	
    Exec { msbuild $solution_path /p:Configuration=$config /nologo /verbosity:minimal }
}

Task Version -Description "Patch AssemblyInfo and AppVeyor version files." {
    $newVersion = Read-Host "Please enter a new version number (major.minor.patch)"
    Update-SharedVersion $newVersion
}

### Functions
function Collect-Assembly($project, $target) {
    Write-Host "Collecting assembly '$target/$project'..." -ForegroundColor "Green"
    
    $source = (Get-SrcOutputDir $project $target) + "\$project.*"
    $destination = "$build_dir\$target"

    Create-Directory $destination
    Copy-Files $source $destination
}

function Create-Package($project, $version) {
    Write-Host "Creating NuGet package for '$project'..." -ForegroundColor "Green"

    Create-Directory $temp_dir
    Copy-Files "$nuspec_dir\$project.nuspec" $temp_dir

    Try {
        Replace-Content "$nuspec_dir\$project.nuspec" '0.0.0' $version
        Exec { .$nuget pack "$nuspec_dir\$project.nuspec" -OutputDirectory "$build_dir" -BasePath "$build_dir" -Version "$version" -Symbols }
    }
    Finally {
        Move-Files "$temp_dir\$project.nuspec" $nuspec_dir
    }
}

function Get-PackageVersion {
    $version = Get-BuildVersion

	$tag = $env:APPVEYOR_REPO_TAG_NAME
    if ($tag -And $tag.StartsWith("v$version-")) {
        $version = $tag.Substring(1)
        Write-Host "Using tag-based version '$version' for packages..." -ForegroundColor "Green"
    }

    return $version
}

function Get-BuildVersion {
    $version = Get-SharedVersion
    $buildNumber = $env:APPVEYOR_BUILD_NUMBER

    if ($env:APPVEYOR_REPO_TAG -ne "True" -And $buildNumber -ne $null) {
        $version += "-build-" + $buildNumber.ToString().PadLeft(5, '0')
        Write-Host "Using CI build version '$version'..." -ForegroundColor "Green"
    }

    return $version
}

function Get-SharedVersion {
    $line = Get-Content "$sharedAssemblyInfo" | where {$_.Contains("AssemblyVersion")}
    $line.Split('"')[1]
}

function Update-SharedVersion($version) {
    Check-Version($version)
        
    $versionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $versionAssembly = 'AssemblyVersion("' + $version + '")';

    if (Test-Path $sharedAssemblyInfo) {
        Write-Host "Patching '$sharedAssemblyInfo'..." -ForegroundColor "Green"
        Replace-Content "$sharedAssemblyInfo" $versionPattern $versionAssembly
    }
}

function Check-Version($version) {
    if ($version -notmatch "[0-9]+(\.([0-9]+|\*)){1,3}") {
        Write-Error "Version number incorrect format: $version"
    }
}

function Create-Archive($name) {
    Write-Host "Creating archive '$name.zip'..." -ForegroundColor "Green"
    Remove-Directory $temp_dir
    Create-Zip "$build_dir\$name.zip" "$build_dir"
}

function Create-Zip($file, $dir){
    if (Test-Path -path $file) { Remove-Item $file }
    Create-Directory $dir
    Exec { & $7zip a -mx -tzip $file $dir\* } 
}

function Create-Directory($dir) {
    New-Item -Path $dir -Type Directory -Force > $null
}

function Clean-Directory($dir) {
    If (Test-Path $dir) {
        Write-Host "Cleaning up '$dir'..." -ForegroundColor "DarkGray"
        Remove-Item "$dir\*" -Recurse -Force
    }
}

function Remove-File($file) {
    if (Test-Path $file) {
        Write-Host "Removing '$file'..." -ForegroundColor "DarkGray"
        Remove-Item $file -Force
    }
}

function Remove-Directory($dir) {
    if (Test-Path $dir) {
        Write-Host "Removing '$dir'..." -ForegroundColor "DarkGray"
        Remove-Item $dir -Recurse -Force
    }
}

function Copy-Files($source, $destination) {
    Copy-Item "$source" $destination -Force > $null
}

function Move-Files($source, $destination) {
    Move-Item "$source" $destination -Force > $null
}

function Replace-Content($file, $pattern, $substring) {
    (gc $file) -Replace $pattern, $substring | sc $file
}

function Get-SrcOutputDir($project, $target) {
    $result = _Get-OutputDir $src_dir $project $target

    Write-Host "  Using directory $result" -ForegroundColor "DarkGray"
    return $result
}

function Get-TestsOutputDir($project, $target) {
    $result = _Get-OutputDir $tests_dir $project $target

    Write-Host "  Using directory $result" -ForegroundColor "DarkGray"
    return $result
}

function _Get-OutputDir($dir, $project, $target) {
    $baseDir = "$dir\$project\bin"
    
    if ($target -And (Test-Path "$baseDir\$target\$config")) {
        return "$baseDir\$target\$config"
    }

    if ($target -And (Test-Path "$baseDir\$config\$target")) {
        return "$baseDir\$config\$target"
    }

    return "$baseDir\$config"
}