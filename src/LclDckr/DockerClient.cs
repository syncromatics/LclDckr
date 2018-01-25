using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LclDckr.Commands.Ps;
using LclDckr.Commands.Ps.Filters;
using LclDckr.Commands.Run;

namespace LclDckr
{
    /// <summary>
    /// provides wrapper over docker cli operations on containers and images
    /// </summary>
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

        /// <summary>
        /// Builds a docker image
        /// </summary>
        /// <param name="path">Path to the context directory</param>
        /// <param name="filePath">Path to docker file, must include the file i.e. PATH/Dockerfile</param>
        /// <param name="tag">the name and optionally a tag in name:tag format</param>
        /// <returns>The identifier for the created iamge. Will be tag if supplied</returns>
        public string Build(string path = ".", string filePath =  null, string tag = null)
        {
            string fileArg = filePath == null ? "" : $"-f {filePath}";
            string tagArg = tag == null ? "" : $"-t {tag}";
            var args = $"build {fileArg} {tagArg} {path}";
            string output;
            using (var process = GetDockerProcess(args))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                // remove trailing \n
                output = process.StandardOutput.ReadToEnd().TrimEnd();
            }

            const string regex = "Successfully built (?<id>[^\\s]+)";
            var match = Regex.Match(output, regex);

            return match.Success ? match.Groups["id"].Value : null;
        }

        /// <summary>
        /// Removes a specified Docker image.
        /// </summary>
        /// <param name="imageName">The name of the image to remove.</param>
        /// <param name="force">Whether to remove image even if in use by one or more containers.</param>
        /// <returns></returns>
        public string RemoveImage(string imageName, bool force = false)
        {
            string forceArg = force ? "-f " : "";
            var args = $"image rm {forceArg}{imageName}";
            using (var process = GetDockerProcess(args))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                // remove trailing \n
                return process.StandardOutput.ReadToEnd().TrimEnd();
            }
        }

        /// <summary>
        /// Runs the specified image in a new container
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="containerName"></param>
        /// <returns>Newly created container ID</returns>
        public string RunImage(string imageName, string containerName)
        {
            return RunImage(imageName, null, new RunArguments {Name = containerName});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="args"></param>
        /// <returns>Newly created container ID</returns>
        public string RunImage(string imageName, RunArguments args)
        {
            return RunImage(imageName, null, args);
        }

        /// <summary>
        /// Runs the specified image in a new container
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="args"></param>
        /// <param name="tag"></param>
        /// <returns>Newly created container ID</returns>
        public string RunImage(string imageName, string tag, RunArguments args, string command = null)
        {
            var tagArg = tag != null ? $":{tag}" : "";

            using (var process = GetDockerProcess($"run {args.ToArgString()} {imageName}{tagArg} {command}"))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                // remove trailing \n
                return process.StandardOutput.ReadToEnd().TrimEnd();
            }
        }

        /// <summary>
        /// Pulls and runs an image as a named container.
        /// If a container by this name already exists, it will be stopped and removed
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public string RunOrReplace(string imageName, string containerName)
        {
            var arguments = new RunArguments
            {
                Name = containerName,
            };

            return RunOrReplace(imageName, null, arguments);
        }

        public string RunOrReplace(string imageName, string tag, RunArguments args, string command = null)
        {
            StopAndRemoveContainer(args.Name);
            PullImage(imageName, tag);
            return RunImage(imageName, tag, args, command);
        }

        /// <summary>
        /// Pulls the specified image.
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="tag"></param>
        public void PullImage(string imageName, string tag = null)
        {
            var tagArg = tag != null ? $":{tag}" : "";
            var args = $"pull {imageName}{tagArg}";

            using (var process = GetDockerProcess(args))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();
            }
        }

        /// <summary>
        /// starts an existing container by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the name of the started container</returns>
        public string StartContainer(string name)
        {
            var args = $"start {name}";

            using (var process = GetDockerProcess(args))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                // remove trailing \n
                return process.StandardOutput.ReadToEnd().TrimEnd();
            }
        }

        /// <summary>
        /// stops an existing container by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>the name of the stopped container</returns>
        public string StopContainer(string name)
        {
            var args = $"stop {name}";
            using (var process = GetDockerProcess(args))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                // remove trailing \n
                return process.StandardOutput.ReadToEnd().TrimEnd();
            }
        }

        /// <summary>
        /// removes an existing container by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string RemoveContainer(string name, bool force = false)
        {
            string forceArg = force ? "-f " : "";
            var args = $"rm {forceArg}{name}";
            using (var process = GetDockerProcess(args))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                // remove trailing \n
                return process.StandardOutput.ReadToEnd().TrimEnd();
            }
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

            List<ContainerInfo> containers;
            using (var process = GetDockerProcess(args.ToString()))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                var headers = process.StandardOutput.ReadLine();

                var parser = new ContainerInfoParser(headers);

                containers = new List<ContainerInfo>();

                while (!process.StandardOutput.EndOfStream)
                {
                    var fields = process.StandardOutput.ReadLine();
                    var container = parser.Parse(fields);
                    containers.Add(container);
                }
            }

            return containers;
        }

        /// <summary>
        /// Reads a container's logs until a specified value appears or until a timeout occurs. Useful for initialization purposes.
        /// </summary>
        /// <param name="container">the container to read the logs from</param>
        /// <param name="desiredLog">the value to check the logs for i.e. 'Database started'</param>
        /// <param name="timeout">Will throw a TimeoutException if the value has not been found after this time</param>
        /// <param name="breakOnError">true will throw an exception on any write to std err</param>
        /// <returns></returns>
        public async Task WaitForLogEntryAsync(string container, string desiredLog, TimeSpan timeout,
            bool breakOnError = true)
        {
            var args = $"logs {container}";
            var watch = new Stopwatch();
            watch.Start();

            while (watch.Elapsed < timeout)
            {
                using (var process = GetDockerProcess(args))
                {
                    process.Start();

                    process.WaitForExit((int) TimeSpan.FromSeconds(10).TotalMilliseconds);

                    if (!process.HasExited)
                    {
                        var output = process.StandardOutput.ReadToEnd();

                        if (output.Contains(desiredLog))
                            return;
                    }
                    else
                    {
                        if (process.ExitCode != 0 && breakOnError)
                            throw new TimeoutException($"An error has occured. {process.StandardError.ReadToEnd()}");

                        if (process.StandardOutput.ReadToEnd().Contains(desiredLog))
                            return;
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(20));
            }

            throw new TimeoutException("Timeout was reached before desired log value was observed.");
        }

        /// <summary>
        /// Inspects a container
        /// </summary>
        /// <param name="name">The name or container id</param>
        /// <param name="format">Format the output using the given Go template, should not be quoted</param>
        /// <returns></returns>
        public string Inspect(string name, string format = null)
        {
            var formatArg = format == null ? "" : $"--format=\"{format}\"";

            using (var process = GetDockerProcess($"inspect {formatArg} {name}"))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                //There's always a \n at the end, if you're using format and expecting
                //an IP or bool, this will be unexpected, so lets remove it.
                return process.StandardOutput.ReadToEnd().TrimEnd();
            }
        }

        /// <summary>
        /// Returns the logs for a container
        /// </summary>
        /// <param name="name">The name or container id</param>
        /// <returns>the logs</returns>
        public string Logs(string name)
        {
            using (var process = GetDockerProcess($"logs {name}"))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                return process.StandardOutput.ReadToEnd();
            }
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
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };
        }
    }
}
