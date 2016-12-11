// arguments 
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var outputDir = Directory("./build");

// tasks definitions
Task("Clean")
    .Does(() =>
{
    CleanDirectory(outputDir);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
	DotNetCoreRestore();
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetCoreBuild("./src/**/project.json");
});

Task("Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
	var settings = new DotNetCoreTestSettings
	{
		Configuration = configuration
	};
	DotNetCoreTest("./tests/AnyBar.Tests", settings);
});

Task("Pack")
    .IsDependentOn("Tests")
    .Does(() =>
{           
	var settings = new DotNetCorePackSettings
	{
		Configuration = configuration,
		OutputDirectory = outputDir
	};
	DotNetCorePack("./src/AnyBar.Client", settings);
	DotNetCorePack("./src/AnyBar.CLI", settings);
});

// run
Task("Default")
    .IsDependentOn("Pack");
RunTarget(target);