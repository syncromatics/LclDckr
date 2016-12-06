using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LclDckr.Commands.Ps;
using LclDckr.Commands.Ps.Filters;

namespace LclDckr
{
    public class DockerClient
    {
        public string Build(string path = ".")
        {
            var args = $"build {path}";
            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            return process.StandardOutput
                .ReadToEnd()
                .Split('\n')
                .Last()
                .Split(' ')
                .Last();
        }

        /// <summary>
        /// Runs the specified image in a new container
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="name"></param>
        /// <param name="hostName"></param>
        /// <param name="interactive"></param>
        /// <returns>The long uuid of the created container</returns>
        public string RunImage(string imageName, string name, string hostName = null, bool interactive = false)
        {
            var hostArg = hostName != null ? $"--hostname {hostName}" : "";
            var interactiveArg = interactive ? "i" : "";

            var args = $"run -d{interactiveArg} --name {name} {hostArg} {imageName}";

            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            return process.StandardOutput.ReadToEnd();
        }

        /// <summary>
        /// Pulls the specified image.
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="tag"></param>
        public void PullImage(string imageName, string tag = "latest")
        {
            var args = $"pull {imageName}:{tag}";

            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();
        }

        /// <summary>
        /// starts an existing container by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the name of the started container</returns>
        public string StartContainer(string name)
        {
            var args = $"start {name}";

            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            return process.StandardOutput.ReadToEnd();
        }

        /// <summary>
        /// stops an existing container by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the name of the stopped container</returns>
        public string StopContainer(string name)
        {
            var args = $"stop {name}";
            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            return process.StandardOutput.ReadToEnd();
        }

        /// <summary>
        /// removes an existing container by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string RemoveContainer(string name)
        {
            var args = $"rm {name}";
            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            return process.StandardOutput.ReadToEnd();
        }

        /// <summary>
        /// Returns info on this systems containers
        /// </summary>
        /// <param name="all">true for all containers, false for running containers only</param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public ICollection<ContainerInfo> Ps(bool all = false, IEnumerable<IFilter> filters = null)
        {
            var args = new StringBuilder("ps");

            if (all)
            {
                args.Append(" -a");
            }

            if (filters != null)
            {
                foreach (var filter in filters)
                {
                    args.Append($" --filter \"{filter.Value}\"");
                }
            }

            var process = GetDockerProcess(args.ToString());

            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            var headers = process.StandardOutput.ReadLine();

            var parser = new ContainerInfoParser(headers);

            var containers = new List<ContainerInfo>();

            while (!process.StandardOutput.EndOfStream)
            {
                var fields = process.StandardOutput.ReadLine();
                var container = parser.Parse(fields);
                containers.Add(container);
            }

            return containers;
        }

        private Process GetDockerProcess(string arguments)
        {
            return new Process
            {
                StartInfo =
                {
                    FileName = "docker",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };
        }
    }
}
