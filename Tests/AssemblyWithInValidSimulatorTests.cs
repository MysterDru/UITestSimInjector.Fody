using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

[TestFixture]
public class AssemblyWithInvalidSimulatorTests
{
    [TestCase]
    public void ValidateWeaverThrowsExecption()
    {
        Assert.Throws<ArgumentException>(() =>
            {
                var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"../../../TestAssemblies/AssemblyWithInValidSimulators/AssemblyWithInValidSimulators.csproj"));
                string beforeAssemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin/Debug/AssemblyWithInValidSimulators.dll");
                #if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
                #endif

                string afterAssemblyPath = beforeAssemblyPath.Replace(".dll", "2.dll");
                File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

                var moduleDefinition = ModuleDefinition.ReadModule(afterAssemblyPath);
                var weavingTask = new ModuleWeaver
                {
                    ModuleDefinition = moduleDefinition
                };

                weavingTask.Execute();
                moduleDefinition.Write(afterAssemblyPath);
            });
    }
}