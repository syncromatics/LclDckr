using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LclDckr.Commands.Ps
{
    /// <summary>
    /// Parses output of Ps command
    /// </summary>
    internal class ContainerInfoParser
    {
        private readonly string[] _expectedHeaders =
        {
            "CONTAINER ID",
            "IMAGE",
            "COMMAND",
            "CREATED",
            "STATUS",
            "PORTS",
            "NAMES"
        };

        private readonly Tuple<int, int>[] _fieldLocations;

        public ContainerInfoParser(string headers)
        {
            _fieldLocations = new Tuple<int, int>[_expectedHeaders.Length];

            for (int i = 0; i < _expectedHeaders.Length; i++)
            {
                var idRegex = new Regex($"({_expectedHeaders[i]}) *");
                var match = idRegex.Match(headers);
                if (!match.Success)
                {
                    throw new Exception($"Returned headers from ps command did not match expected headers {headers}");
                }

                _fieldLocations[i] = Tuple.Create(match.Index, match.Length);
            }
        }

        public ContainerInfo Parse(string fields)
        {
            Func<int, string> getField = i => fields.Substring(_fieldLocations[i].Item1, i < _fieldLocations.Length - 1 ? _fieldLocations[i].Item2 : fields.Length - _fieldLocations[i].Item1);

            return new ContainerInfo
            {
                ContainerId = getField(0).Trim(),
                Image = getField(1).Trim(),
                Command = getField(2).Trim(),
                Created = getField(3).Trim(),
                Status = getField(4).Trim(),
                Ports = getField(5).Trim(),
                Names = getField(6).Trim().Split(',').ToList()
            };
        }
    }
}