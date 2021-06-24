using System.Collections.Generic;
using FaultStructureModeling.Entities.Geometry;

namespace FaultStructureModeling.Entities.Model
{
    class IntersectionLine
    {
        public List<Vertex> Line { get; set; }
        public List<int> Flags { get; set; }
        public int Start { get; set; }
        public int End { get; set; }

        public IntersectionLine()
        {
            Line = new List<Vertex>();
            Flags = new List<int>();
        }
    }
}
