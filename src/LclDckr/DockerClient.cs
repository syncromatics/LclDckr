using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LclDckr.Commands.Ps;
using LclDckr.Commands.Ps.Filters;
using LclDckr.Commands.Run;

namespace LclDckr
{
    public class DockerClient
    {
        private readonly string _dockerPath = "docker";

        /// <summary>
        /// "docker" must be on the path for this ctor
        /// </summary>
        public DockerClient() { }

        /// <summary>
        /// used when you need to specify an alternate docker path
        /// </summary>
        /// <param name="dockerExecutablePath">path to docker, including "docker"</param>
        public DockerClient(string dockerExecutablePath)
        {
            _dockerPath = dockerExecutablePath;
        }

        public string Build(string path = ".", string filePath =  null)
        {
            string fileArg = filePath == null ? "" : $"-f {filePath}";
            var args = $"build {fileArg} {path}";
            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            var output = process.StandardOutput
                .ReadToEnd();

            const string regex = "Successfully built (?<id>[^\\s]+)";
            var match = Regex.Match(output, regex);

            return match.Success ? match.Groups["id"].Value : null;
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
            var arguments = new RunArguments
            {
                Name = name,
                HostName = hostName,
                Interactive = interactive
            };

            return RunImage(imageName, arguments);
        }

        /// <summary>
        /// Runs the specified image in a new container
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string RunImage(string imageName, RunArguments args)
        {
            var process = GetDockerProcess($"run {args.ToArgString()} {imageName}");
            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            return process.StandardOutput.ReadToEnd();
        }

        /// <summary>
        /// Pulls and runs an image as a named container.
        /// If a container by this name already exists, it will be stopped and removed
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="containerName"></param>
        /// <param name="tag"></param>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public string RunOrReplace(string imageName, string containerName, string tag = "latest", string hostName = null)
        {
            StopAndRemoveContainer(containerName);
            PullImage(imageName, tag);
            return RunImage(imageName, containerName, hostName);
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
        /// Stops and removes a container. Does nothing if the container does not exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The name of the removed container or null if the container didn't exist</returns>
        public string StopAndRemoveContainer(string name)
        {
            if (Ps(true, new[] { new NameFilter(name) }).Any())
            {
                StopContainer(name);
                return RemoveContainer(name);
            }
            return null;
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
                    FileName = _dockerPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };
        }
    }
}
