using FlubuCore.Context;
using FlubuCore.Context.Attributes.BuildProperties;
using FlubuCore.IO;
using FlubuCore.Scripting;
using FlubuCore.Scripting.Attributes;
using FlubuCore.Tasks;
using FlubuCore.Tasks.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks; // 添加这个命名空间引用
using System.Linq; // 添加此命名空间引用

namespace build
{
    public class Build : DefaultBuildScript
    {


        [FromArg("c|configuration")]
        [BuildConfiguration]
        public string Configuration { get; set; } = "Release";
        protected List<FileFullPath> ProjectFiles { get; set; }
        protected string ArtifactsDir => RootDirectory.CombineWith("artifacts");
        protected override void BeforeBuildExecution(ITaskContext context)
        {
            ProjectFiles = context.GetFiles(RootDirectory.CombineWith("."), "*/*.csproj");
            if (!Directory.Exists(ArtifactsDir))
            {
                Directory.CreateDirectory(ArtifactsDir);
            }
        }
        protected override void ConfigureTargets(ITaskContext context)
        {
            // 2. 设置控制台输出为UTF-8（避免二次乱码）
            Console.OutputEncoding = Encoding.Unicode;
            var clean = context.CreateTarget("Clean")
                .SetDescription("Cleans the output of all projects in the solution.")
                .AddCoreTask(x => x.Clean().AddDirectoryToClean(ArtifactsDir, true));

            var restore = context.CreateTarget("Restore")
                .SetDescription("Restores the dependencies and tools of all projects in the solution.")
                .DependsOn(clean)
                .AddCoreTask(x => x.Restore());

            var build = context.CreateTarget("Build")
                .SetDescription("Builds all projects in the solution.")
                .DependsOn(restore)
                .AddCoreTask(x => x.Build());


            var pack = context.CreateTarget("Pack")
                .SetDescription("Creates nuget packages for Cap.")
                .ForEach(ProjectFiles, (projectFile, target) =>
                {
                    if (projectFile.FileName != "build.csproj")
                    {
                        target.AddCoreTask(x =>
                        {
                            var task = x.Pack()
                                .NoBuild()
                                .Project(projectFile)
                                .IncludeSymbols()
                                .OutputDirectory(ArtifactsDir);
                            return task;
                        });
                    }
                });

            var upload = context.CreateTarget("Upload")
                .SetDescription("Upload to nuget")
                .DoAsync(async (context) =>
                {
                    var nupkgs = Directory.GetFiles(ArtifactsDir).Where(f => f.EndsWith(".nupkg"));
                    await Parallel.ForEachAsync(nupkgs, async (projectFile, cancellationToken) =>
                    {
                        var task = context.CoreTasks().NugetPush(projectFile)
                            .ApiKey(Environment.GetEnvironmentVariable("NUGET_API_KEY"))
                            .WithOutputLogLevel(LogLevel.None)
                            .WithLogLevel(LogLevel.None)
                            .CaptureErrorOutput()
                            .DoNotFailOnError()
                            .DoNotLogTaskExecutionInfo()
                            .SkipDuplicate()
                            .ServerUrl("https://api.nuget.org/v3/index.json");
                        task.Retry(int.MaxValue, 1000, (c, err) =>
                        {
                            Console.WriteLine(task.GetOutput());
                            Console.WriteLine(err.Message);
                            if (task.GetOutput().Contains("indicate success: 403"))
                            {
                                throw new Exception("NUGET_API_KEY 授权错误");
                            }

                            return true;
                        });
                        task.Finally((c) =>
                        {
                            var output = task.GetOutput();

                            if (output.Contains("Conflict https://www.nuget.org/api/v2/package/"))
                            {
                                Console.WriteLine("包已存在,跳过");
                            }
                            else if (output.Contains("Created https://www.nuget.org/api/v2/symbolpackage/"))
                            {
                                Console.WriteLine("创建成功");
                            }
                            else if (output.Contains("Forbidden https://www.nuget.org/api/v2/package/"))
                            {
                            }
                            else
                            {
                                Console.WriteLine(task.GetOutput());
                            }
                        });
                        await task.ExecuteAsync(context);
                    });
                });


            context.CreateTarget("Default")
                .SetDescription("Runs all targets.")
                .SetAsDefault()
                .DependsOn(clean, build, pack, upload);
        }
    }
}