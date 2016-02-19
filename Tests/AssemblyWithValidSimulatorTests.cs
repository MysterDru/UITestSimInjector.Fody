using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

[TestFixture]
public class AssemblyWithValidSimulatorTests
{
    Assembly assembly;
    List<string> warnings = new List<string>();
    string beforeAssemblyPath;
    string afterAssemblyPath;


    [TestFixtureSetUp]
    public void Startup()
    {
        try
        {
            var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"../../../TestAssemblies/AssemblyWithValidSimulators/AssemblyWithValidSimulators.csproj"));
            beforeAssemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin/Debug/AssemblyWithValidSimulators.dll");
            #if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
            #endif

            afterAssemblyPath = beforeAssemblyPath.Replace(".dll", "2.dll");
            File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

            var moduleDefinition = ModuleDefinition.ReadModule(afterAssemblyPath);
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition
            };

            weavingTask.Execute();
            moduleDefinition.Write(afterAssemblyPath);

            assembly = Assembly.LoadFile(afterAssemblyPath);
        }
        catch (Exception e)
        {
        }
    }


    [TestCase]
    public void ValidateNewAttributeIsAdded()
    {
        int expectedFixtureCount = 2;

        var type = this.assembly.GetType("AssemblyWithValidArguments.Class1");

        IEnumerable<Attribute> customAttributes = type.GetCustomAttributes();

        IEnumerable<Attribute> testFixtures = customAttributes.Where(x => x.GetType() == typeof(TestFixtureAttribute));

        Assert.AreEqual(expectedFixtureCount, testFixtures.Count());
    }

    [TestCase]
    public void ValidateNewAttributeArgumentCount()
    {
        var newType = this.assembly.GetType("AssemblyWithValidArguments.Class1");

        var testFixtureAttr = (TestFixtureAttribute)newType.GetCustomAttributes().FirstOrDefault(x => x.GetType() == typeof(TestFixtureAttribute));

        Assert.AreEqual(2, testFixtureAttr.Arguments.Length);
    }

    [TestCase]
    public void ValidateNewAttributeFirstArgumentIsPlatform()
    {
        var newType = this.assembly.GetType("AssemblyWithValidArguments.Class1");

        var testFixtureAttr = (TestFixtureAttribute)newType.GetCustomAttributes().FirstOrDefault(x => x.GetType() == typeof(TestFixtureAttribute));

        object arg = testFixtureAttr.Arguments.First();

        Assert.AreEqual(typeof(Xamarin.UITest.Platform), arg.GetType());

        Xamarin.UITest.Platform platform = (Xamarin.UITest.Platform)arg;
        Assert.AreEqual(Xamarin.UITest.Platform.iOS, platform);
    }

    [TestCase]
    public void ValidateNewAttributeExpectedSimulatorIds()
    {
        string[] simulatorNames = new string[] { "iPhone 5s (9.1)", "iPhone 6 Plus (9.1)" };
        string[] expectedIds = this.GetIdsForSimulatorNames(simulatorNames);

        var type = this.assembly.GetType("AssemblyWithValidArguments.Class1");

        IEnumerable<Attribute> testFixtureAttributes = type.GetCustomAttributes().Where(x => x.GetType() == typeof(TestFixtureAttribute));

        IEnumerable<string> actualIds = testFixtureAttributes.Select(x => ((TestFixtureAttribute)x).Arguments[1]).Cast<string>();

        CollectionAssert.AreEquivalent(expectedIds, actualIds);
    }

    [TestCase]
    public void ValidateInjectAttributeIsRemoved()
    {
        var type = this.assembly.GetType("AssemblyWithValidArguments.Class1");

        IList<Attribute> customAttributes = type.GetCustomAttributes().ToList();

        bool hasInjector = customAttributes.Any(x => x.GetType() == typeof(SimulatorInjector.InjectSimulatorIdsForIOSAttribute));

        Assert.IsFalse(hasInjector);
    }

    #if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
    }
    #endif

    private string[] GetIdsForSimulatorNames(string[] names)
    {
        Dictionary<string, string> values = Helper.GetSimulatorIds();

        List<string> ids = new List<string>();

        foreach (var name in names)
        {
            if (values.ContainsKey(name))
            {
                ids.Add(values[name]);
            }
        }

        return ids.ToArray();
    }
}