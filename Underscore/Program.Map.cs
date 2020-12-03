using System.Collections.Generic;
using System.Linq;

namespace Underscore
{
    partial class Program
    {
        private class Map
        {
            readonly Dictionary<string, FilePath> _sourceMap = new Dictionary<string, FilePath>();
            readonly Dictionary<string, List<FilePath>> _destinationMap = new Dictionary<string, List<FilePath>>();
            readonly Dictionary<string, FilePath> _invalidMap = new Dictionary<string, FilePath>();
            readonly HashSet<string> _conflicts = new HashSet<string>();
            readonly HashSet<string> _destinations = new HashSet<string>();

            public void Add(FilePath info)
            {
                if (info.HasConflict)
                {
                    _conflicts.Add(info.Source);
                }

                _sourceMap[info.Source] = info;

                if (!info.IsValidName)
                {
                    _invalidMap[info.Source] = info;
                }

                if (!_destinationMap.TryGetValue(info.Destination, out var list))
                {
                    _destinationMap[info.Destination] = list = new List<FilePath>();
                }

                list.Add(info);
                if (list.Count > 1)
                {
                    _destinations.Add(info.Destination);
                }
            }

            public bool HasProblems => _conflicts.Count != 0 || _destinations.Count != 0 || _invalidMap.Count != 0;
            public FilePath[] Conflicts => _conflicts.Select(f => _sourceMap[f]).ToArray();

            public ConflictDestination[] DestinationConflicts =>
                _destinations.Select(s => new ConflictDestination(s, _destinationMap[s])).ToArray();

            public FilePath[] Files => _sourceMap.Values.ToArray();
            public FilePath[] InvalidNames => _invalidMap.Values.ToArray();
        }
    }
}