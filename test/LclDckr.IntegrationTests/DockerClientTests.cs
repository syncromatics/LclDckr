using System.Linq;
using LclDckr.Commands.Ps.Filters;
using Xunit;

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

            var id = client.RunImage("ubuntu", containerName, interactive: true);
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

            var imageId = client.Build();

            var containerName = "lcldckr-build-test";

            client.RunImage(imageId, containerName);

            var container = client
                .Ps(true, new[] { new NameFilter(containerName) })
                .SingleOrDefault();

            Assert.NotNull(container);

            client.StopAndRemoveContainer(containerName);
        }
    }
}
