using FaultStructureModeling.Entities.Geometry;

namespace FaultStructureModeling.Entities.Model
{
    class IntersectionPoint
    {
        public Vertex Point { get; set; }
        public int Start1 { get; set; }
        public int End1 { get; set; }
        public int Start2 { get; set; }
        public int End2 { get; set; }

        public IntersectionPoint()
        {
            Point = new Vertex();
        }
    }
}
