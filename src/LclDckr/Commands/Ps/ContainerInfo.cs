using System.Collections.Generic;

namespace LclDckr.Commands.Ps
{
    public class ContainerInfo
    {
        public string ContainerId { get; set; }
        public string Image { get; set; }
        public string Command { get; set; }
        public string Created { get; set; }
        public string Status { get; set; }
        public string Ports { get; set; }
        public List<string> Names { get; set; }
    }
}
