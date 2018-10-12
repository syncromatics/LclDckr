# LclDckr

A dotnet wrapper for Docker's CLI

## Quickstart

```c#
DockerClient client = new DockerClient();
client.PullImage("hello-world", "latest");
client.RunImage("hello-world", "hello-container");
client.StopContainer("hello-container");
client.RemoveContainer("hello-container");
```

## Building

[![Travis](https://img.shields.io/travis/syncromatics/LclDckr.svg)](https://travis-ci.org/syncromatics/LclDckr)
[![NuGet](https://img.shields.io/nuget/v/LclDckr.svg)](https://www.nuget.org/packages/LclDckr/)
[![NuGet Pre Release](https://img.shields.io/nuget/vpre/LclDckr.svg)](https://www.nuget.org/packages/LclDckr/)

## Code of Conduct

We are committed to fostering an open and welcoming environment. Please read our [code of conduct](CODE_OF_CONDUCT.md) before participating in or contributing to this project.

## Contributing

We welcome contributions and collaboration on this project. Please read our [contributor's guide](CONTRIBUTING.md) to understand how best to work with us.

## License and Authors

[![GMV Syncromatics Engineering logo](https://secure.gravatar.com/avatar/645145afc5c0bc24ba24c3d86228ad39?size=16) GMV Syncromatics Engineering](https://github.com/syncromatics)

[![license](https://img.shields.io/github/license/syncromatics/LclDckr.svg)](https://github.com/syncromatics/LclDckr/blob/master/LICENSE)
[![GitHub contributors](https://img.shields.io/github/contributors/syncromatics/LclDckr.svg)](https://github.com/syncromatics/LclDckr/graphs/contributors)

This software is made available by GMV Syncromatics Engineering under the MIT license.