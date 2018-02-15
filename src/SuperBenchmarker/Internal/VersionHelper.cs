using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SuperBenchmarker.Internal
{
    public static class VersionHelper
    {
        public static string GetVersion()
        {
            var att = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return att.InformationalVersion;
        }
    }
}
