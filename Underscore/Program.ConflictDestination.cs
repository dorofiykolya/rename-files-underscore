using System.Collections.Generic;

namespace Underscore
{
    partial class Program
    {
        private class ConflictDestination
        {
            public ConflictDestination(string destination, List<FilePath> list)
            {
                Destination = destination;
                Conflicts = list;
            }

            public List<FilePath> Conflicts { get; }

            public string Destination { get; }
        }
    }
}