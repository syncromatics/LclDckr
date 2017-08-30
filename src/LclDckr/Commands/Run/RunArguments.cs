using System.Collections.Generic;
using System.Text;

namespace LclDckr.Commands.Run
{
    public class RunArguments
    {
        public bool Interactive { get; set; }

        public string Name { get; set; }

        public string HostName { get; set; }

        public IDictionary<string, string> EnvironmentArgs { get; set; }  = new Dictionary<string, string>();

        public IList<string> Volumes { get; set; }  = new List<string>();

        public IDictionary<string, string> PortMappings { get; set; }  = new Dictionary<string, string>();

        public string ToArgString()
        {
            var args = new StringBuilder("-d");

            if (Interactive)
            {
                args.Append("i");
            }

            if (Name != null)
            {
                args.Append($" --name {Name}");
            }

            if (HostName != null)
            {
                args.Append($" --hostname {HostName}");
            }

            foreach (var environmentArg in EnvironmentArgs)
            {
                args.Append($" -e {environmentArg.Key}={environmentArg.Value}");
            }

            foreach (var volume in Volumes)
            {
                args.Append($" -v {volume}");
            }

            foreach (var mapping in PortMappings)
            {
                args.Append($" -p {mapping.Key}:{mapping.Value}");
            }

            return args.ToString();
        }
    }
}
