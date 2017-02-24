using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LclDckr.Commands.Ps.Filters;
using LclDckr.Commands.Run;
using Xunit;
using System.Net;

namespace LclDckr.IntegrationTests
{
    public class DockerClientTests
    {
        [Fact]
        public void Pulls_runs_stops_image()
        {
            var client = new DockerClient();
            client.PullImage("ubuntu");

            var containerName = "lcldckr-test-container";

            var id = client.RunImage("ubuntu", "latest", new RunArguments {Name = containerName, Interactive = true});
            Assert.NotNull(id);

            var runningContainer = client
                .Ps(true, new[] {new NameFilter(containerName)})
                .SingleOrDefault();

            Assert.NotNull(runningContainer);

            Assert.Equal(containerName, runningContainer.Names.Single());

            Assert.Equal(id.Substring(0, 12), runningContainer.ContainerId);

            id = client.StopContainer(containerName);
            Assert.NotNull(id);

            id = client.RemoveContainer(containerName);
            Assert.NotNull(id);
        }

        [Fact]
        public void Runs_and_replaces_container()
        {
            var client = new DockerClient();

            var containerName = "lcldckr-test-2";

            client.RunOrReplace("ubuntu", containerName);

            var runningContainer = client
                .Ps(true, new[] { new NameFilter(containerName) })
                .SingleOrDefault();

            Assert.NotNull(runningContainer);

            client.RunOrReplace("ubuntu", containerName);

            client.StopAndRemoveContainer(containerName);

            runningContainer = client
                .Ps(true, new[] { new NameFilter(containerName) })
                .SingleOrDefault();

            Assert.Null(runningContainer);
        }

        [Fact]
        public void Builds_and_runs_container()
        {
            var client = new DockerClient();

            var contextPath = AppContext.BaseDirectory;
            var dockerFilePath = Path.Combine(contextPath, "Dockerfile");

            var imageId = client.Build(contextPath, dockerFilePath);

            var containerName = "lcldckr-build-test";

            client.RunImage(imageId, containerName);

            var container = client
                .Ps(true, new[] { new NameFilter(containerName) })
                .SingleOrDefault();

            Assert.NotNull(container);

            client.StopAndRemoveContainer(containerName);
        }

        [Fact]
        public async Task Waits_for_log_entry()
        {
            var client = new DockerClient();

            var containerName = "lcldkr-log-test";

            client.RunOrReplace("hello-world", containerName);

            await client.WaitForLogEntryAsync(containerName, "Hello from Docker!", TimeSpan.FromSeconds(30));

            client.StopAndRemoveContainer(containerName);
        }

        [Fact]
        public void Inspects()
        {
            var containerName = "lcldkr-inspect-test";
            var client = new DockerClient();

            try
            {
                client.RunOrReplace(
                    "ubuntu",
                    tag: null,
                    args: new RunArguments() { Name = containerName },
                    command: "tail -f /dev/null"); //we want the container to stay running

                var ipString = client.Inspect(containerName, "{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}");

                IPAddress ip;
                bool isIp = IPAddress.TryParse(ipString, out ip);

                Assert.True(isIp);
            }
            finally
            {
                client.StopAndRemoveContainer(containerName);
            }
        }
    }
}
