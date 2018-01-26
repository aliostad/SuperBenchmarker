using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SuperBenchmarker
{
    public abstract class CommandOption
    {
        protected virtual string GetHelp()
        {
            var options = new StringBuilder();
            var parameters = new StringBuilder();
            options.AppendLine("Usage:");

            foreach (var property in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x=>x.GetCustomAttribute<OptionAttribute>()!=null)
                .OrderByDescending(x=>x.GetCustomAttribute<OptionAttribute>().Required))
            {
                var attribute = property.GetCustomAttribute<OptionAttribute>();
                if (attribute != null)
                {
                    options.Append(property.PropertyType == typeof (bool)
                                       ? " [-" + attribute.ShortName + "] "
                                       : attribute.Required
                                             ? " -" + attribute.ShortName + " " + attribute.LongName
                                             : " [-" + attribute.ShortName + " " + attribute.LongName + "]");
                    parameters.Append(" -");
                    parameters.Append(attribute.ShortName);
                    parameters.Append("\t");
                    parameters.Append(attribute.Required ? "Required. " : "Optional. ");
                    parameters.Append(attribute.HelpText);
                    parameters.AppendLine(property.PropertyType == typeof (bool) ? " (boolean switch)" :
                        (!attribute.Required && attribute.Default!=null) ? " (default=" + attribute.Default + ")" : "");
                }

            }

            options.AppendLine();
            options.AppendLine("Parameters: ");
            options.AppendLine(parameters.ToString());

            var method = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                  .FirstOrDefault(x => x.GetCustomAttributes<ExampleAttribute>().Any());
            if (method != null)
            {
                options.AppendLine();
                options.AppendLine("Examples:");
                options.AppendLine(method.Invoke(this, new object[0]).ToString());
            }

            return options.ToString();
        }


    }
}
