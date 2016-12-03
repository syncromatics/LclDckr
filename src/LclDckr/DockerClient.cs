using System.Diagnostics;
using System.Linq;

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

        public void PullImage(string imageName, string tag)
        {
            var args = $"pull {imageName}:{tag}";

            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();
        }

        public string StartContainer(string name)
        {
            var args = $"start {name}";

            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            return process.StandardOutput.ReadToEnd();
        }

        public string StopContainer(string name)
        {
            var args = $"stop {name}";
            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            return process.StandardOutput.ReadToEnd();
        }

        public string RemoveContainer(string name)
        {
            var args = $"rm {name}";
            var process = GetDockerProcess(args);
            process.Start();
            process.WaitForExit();
            process.ThrowForError();

            return process.StandardOutput.ReadToEnd();
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
