Properties {
    $solution = "anybar-sharp.sln"
	
	### Directories
    $base_dir = resolve-path .
    $build_dir = "$base_dir\build"
    $src_dir = "$base_dir\src"
    $tests_dir = "$base_dir\tests"
    $package_dir = "$base_dir\packages"
    $nuspec_dir = "$base_dir\nuspecs"
    $temp_dir = "$build_dir\Temp"
   
    ### Tools
    $nuget = "$base_dir\.nuget\nuget.exe"
	
    ### Project information
    $solution_path = "$base_dir\$solution"
    $config = "Release"    
    $sharedAssemblyInfo = "$src_dir\SharedAssemblyInfo.cs"
}
