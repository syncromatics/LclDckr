# LclDckr
A dotnet wrapper for Docker's CLI

## Example usage

```c#
	DockerClient client = new DockerClient();
	client.PullImage("hello-world", "latest");
	client.RunImage("hello-world", "hello-container");
	client.StopContainer("hello-container");
	client.RemoveContainer("hello-container");
```