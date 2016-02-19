using System;

namespace SimulatorInjector
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class InjectSimulatorIdsForIOSAttribute : System.Attribute
    {
        public string[] SimulatorNames { get; private set; }

        public InjectSimulatorIdsForIOSAttribute(params string[] simulatorNames)
        {
            this.SimulatorNames = simulatorNames;
        }
    }
}