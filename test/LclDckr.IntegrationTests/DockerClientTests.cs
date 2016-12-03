using Xunit;

namespace LclDckr.IntegrationTests
{
    public class DockerClientTests
    {
        [Fact]
        public void Pulls_runs_stops_image()
        {
            var client = new DockerClient();
            client.PullImage("ubuntu", "latest");

            var id = client.RunImage("ubuntu", "test-container", interactive: true);
            Assert.NotNull(id);

            id = client.StopContainer("test-container");
            Assert.NotNull(id);

            id = client.RemoveContainer("test-container");
            Assert.NotNull(id);
        }
    }
}
