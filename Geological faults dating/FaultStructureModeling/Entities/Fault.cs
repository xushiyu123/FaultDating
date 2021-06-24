using FaultStructureModeling.Entities.Geometry;
using System.Collections.Generic;

namespace FaultStructureModeling.Entities
{
    class Fault
    {
        private double dipAngle;//倾角

        private double direction;//走向

        private double inclination;//倾向

        private double width;//宽度

        private Vertex vector;//推演方向

        private List<Vertex> line;//坐标集合

        private int Id;//断层ID

        private string code;//断层次序

        private string start;//断层起始时间

        private string end;//断层结束时间

        public double DipAngle { get => dipAngle; set => dipAngle = value; }
        public double Direction { get => direction; set => direction = value; }
        public List<Vertex> Line { get => line; set => line = value; }
        public double Width { get => width; set => width = value; }
        public Vertex Vector { get => vector; set => vector = value; }
        public double Inclination { get => inclination; set => inclination = value; }
        public int ID { get => Id; set => Id = value; }
        public string Code { get => code; set => code = value; }
        public string Start { get => start; set => start = value; }
        public string End { get => end; set => end = value; }
    }
}
