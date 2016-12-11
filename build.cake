#addin "Cake.Json"

// arguments 
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var outputDir = Directory("./build");
var projectFiles = "./src/**/project.json";

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
	var projects = GetFiles(projectFiles);
	foreach(var projectFile in projects)
	{
		var frameworks = ParseJsonFromFile(projectFile)
			.GetValue("frameworks")
			.Children<JProperty>()
			.Select(j => j.Name);

		foreach(var framework in frameworks)
		{
			if (!IsRunningOnWindows() && framework == "net452")
				continue;
			
			Information("Framework version {0}", framework);
			var settings = new DotNetCoreBuildSettings
			{
				Framework = framework
			};
			DotNetCoreBuild(projectFile.FullPath, settings);
		}			
	}
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
});

// run
Task("Default")
    .IsDependentOn("Pack");
RunTarget(target);