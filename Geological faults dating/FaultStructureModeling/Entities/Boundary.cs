using FaultStructureModeling.Entities.Geometry;
using System.Collections.Generic;

namespace FaultStructureModeling.Entities.Geography
{
    class Boundary
    {
        private int[] stratumIds;//所属层面id

        private double dipAngle;//倾向

        private double direction;//走向

        private double inclination;//倾向

        private Vertex vector;//推演方向

        private List<Vertex> line;//坐标集合

        public double DipAngle { get => dipAngle; set => dipAngle = value; }
        public double Direction { get => direction; set => direction = value; }
        public List<Vertex> Line { get => line; set => line = value; }
        public Vertex Vector { get => vector; set => vector = value; }
        public int[] StratumIds { get => stratumIds; set => stratumIds = value; }
        public double Inclination { get => inclination; set => inclination = value; }

        public Boundary(int[] stratumIds = null, double dipAngle = double.NaN, double direction = double.NaN, double inclination = double.NaN, Vertex vector = null, List<Vertex> line = null)
        {
            this.stratumIds = stratumIds;
            if (this.stratumIds == null)
                this.stratumIds = new int[2];
            this.dipAngle = dipAngle;
            this.direction = direction;
            this.inclination = inclination;
            this.vector = vector;
            if (this.vector == null)
                this.vector = new Vertex();
            this.line = new List<Vertex>();
            if (line != null)
            {
                foreach (Vertex point in line)
                    this.line.Add(point);
            }
        }
    }
}
