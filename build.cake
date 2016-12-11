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
	if (IsRunningOnWindows())
	{
		DotNetCoreBuild(projectFiles);
	}
	else
	{
		// in non-Windows we should not build "full framework"
		var projects = GetFiles(projectFiles);
		foreach(var projectFile in projects)
		{
			var framework = ParseJsonFromFile(projectFile)
				.GetValue("frameworks")
				.Children<JProperty>().Where(j => j.Name != "net452")
				.Select(j => j.Name)
				.FirstOrDefault();
			if(framework != null)
			{
				Information("Framework version {0}", framework);
				var settings = new DotNetCoreBuildSettings
				{
					Runtime = framework
				};
				DotNetCoreBuild(projectFiles, settings);
			}			
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