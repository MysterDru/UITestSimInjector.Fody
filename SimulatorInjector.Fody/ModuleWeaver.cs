using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using Xamarin.UITest;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

public class ModuleWeaver
{
    private const string AttributeName = "SimulatorInjector.InjectSimulatorIdsForIOSAttribute";

    // Will log an informational message to MSBuild
    public Action<string> LogInfo { get; set; }

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }

    Dictionary<string, string> SimulatorValues { get; set; }

    TypeSystem typeSystem;

    // Init logging delegates to make testing easier
    public ModuleWeaver()
    {
        LogInfo = m =>
        {
        };
    }

    public void Execute()
    {
        this.SimulatorValues = Helper.GetSimulatorIds();

        IEnumerable<TypeDefinition> types = this.GetTypes();

        foreach (var type in types)
        {
            Inject(type);
        }
    }

    private IEnumerable<TypeDefinition> GetTypes()
    {
        return this.ModuleDefinition.Types.Where(ContainsAttribute);
    }

    private bool ContainsAttribute(TypeDefinition type)
    {
        return type.CustomAttributes.Any(x => x.AttributeType.FullName == ModuleWeaver.AttributeName);
    }

    private void Inject(TypeDefinition targetType)
    {
        CustomAttribute existing = targetType.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == ModuleWeaver.AttributeName);

        if (existing != null)
        {
            CustomAttributeArgument[] args = (CustomAttributeArgument[])existing.ConstructorArguments.First().Value;

            List<string> simulatorNames = new List<string>();
            foreach (var arg in args)
            {
                simulatorNames.Add((string)arg.Value);
            }

            TypeReference platformType = ModuleDefinition.ImportReference(typeof(Xamarin.UITest.Platform));
            TypeReference objectType = ModuleDefinition.ImportReference(typeof(System.Object));
            TypeReference objectParamsType = ModuleDefinition.ImportReference(typeof(System.Object[]));

            ModuleDefinition module = targetType.Module;

            ConstructorInfo constructorInfo = typeof(NUnit.Framework.TestFixtureAttribute).GetConstructors().Where(x => x.GetParameters().Length > 0).FirstOrDefault();

            MethodReference attributeConstructor = targetType.Module.ImportReference(constructorInfo);

            foreach (string simulatorName in simulatorNames)
            {                
                if (this.SimulatorValues.ContainsKey(simulatorName))
                {
                    string simulatorId = this.SimulatorValues[simulatorName];

                    var attribute = new CustomAttribute(attributeConstructor);

                    CustomAttributeArgument platformArg = new CustomAttributeArgument(platformType, Platform.iOS);
                    CustomAttributeArgument nameArg = new CustomAttributeArgument(module.TypeSystem.String, simulatorId);

                    CustomAttributeArgument platformWrapper = new CustomAttributeArgument(objectType, platformArg);
                    CustomAttributeArgument nameWrapper = new CustomAttributeArgument(objectType, nameArg);

                    attribute.ConstructorArguments.Add(new CustomAttributeArgument(objectParamsType, new CustomAttributeArgument[2] { platformWrapper, nameWrapper }));

                    targetType.CustomAttributes.Add(attribute);
                }
                else
                {
                    throw new ArgumentException(string.Format("Simulator with name of \"{0}\" does not exist on this machine.", simulatorName));
                }
            }
        }

        targetType.CustomAttributes.Remove(existing);
    }
}