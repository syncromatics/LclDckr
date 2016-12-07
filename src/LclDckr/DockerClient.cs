﻿using System;
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

                output = process.StandardOutput.ReadToEnd();
            }

            const string regex = "Successfully built (?<id>[^\\s]+)";
            var match = Regex.Match(output, regex);

            return match.Success ? match.Groups["id"].Value : null;
        }

        /// <summary>
        /// Runs the specified image in a new container
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="containerName"></param>
        /// <returns></returns>
        public string RunImage(string imageName, string containerName)
        {
            return RunImage(imageName, null, new RunArguments {Name = containerName});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
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
        /// <returns></returns>
        public string RunImage(string imageName, string tag, RunArguments args)
        {
            var tagArg = tag != null ? $":{tag}" : "";

            using (var process = GetDockerProcess($"run {args.ToArgString()} {imageName}{tagArg}"))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                return process.StandardOutput.ReadToEnd();
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

        public string RunOrReplace(string imageName, string tag, RunArguments args)
        {
            StopAndRemoveContainer(args.Name);
            PullImage(imageName, tag);
            return RunImage(imageName, tag, args);
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

                return process.StandardOutput.ReadToEnd();
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

                return process.StandardOutput.ReadToEnd();
            }
        }

        /// <summary>
        /// removes an existing container by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string RemoveContainer(string name)
        {
            var args = $"rm {name}";
            using (var process = GetDockerProcess(args))
            {
                process.Start();
                process.WaitForExit();
                process.ThrowForError();

                return process.StandardOutput.ReadToEnd();
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
        public async Task WaitForLogEntryAsync(string container, string desiredLog, TimeSpan timeout, bool breakOnError = true)
        {
            var args = $"logs -f {container}";

            TaskCompletionSource<bool> tcs;
            using (var process = GetDockerProcess(args))
            {
                tcs = new TaskCompletionSource<bool>();

                var timeoutTask = Task.Delay(timeout);

                process.OutputDataReceived += (_, eventArgs) =>
                {
                    if (eventArgs.Data == null)
                    {
                        return;
                    }
                    if (eventArgs.Data.Contains(desiredLog))
                    {
                        tcs.TrySetResult(true);
                    }
                };

                if (breakOnError)
                {
                    process.ErrorDataReceived += (_, eventArgs) =>
                    {
                        if (eventArgs.Data == null)
                        {
                            return;
                        }

                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                        throw new Exception($"error while waiting for log entry: {eventArgs.Data}");
                    };
                }

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

                if (!process.HasExited)
                {
                    process.Kill();
                }
            }

            if (!tcs.Task.IsCompleted)
            {
                throw new TimeoutException("Timeout was reached before desired log value was observed.");
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
                    UseShellExecute = false
                }
            };
        }
    }
}
