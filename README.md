# LclDckr [![Build Status](https://travis-ci.org/syncromatics/LclDckr.svg?branch=feature%2Fenable-travis-builds)](https://travis-ci.org/syncromatics/LclDckr)
A dotnet wrapper for Docker's CLI

## Example usage

```c#
	DockerClient client = new DockerClient();
	client.PullImage("hello-world", "latest");
	client.RunImage("hello-world", "hello-container");
	client.StopContainer("hello-container");
	client.RemoveContainer("hello-container");
```