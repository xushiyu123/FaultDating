using System;
using FaultStructureModeling.Entities.Geometry;
using System.Collections.Generic;

namespace FaultStructureModeling.Entities.Model
{
    /// <summary>
    /// 模型面实体
    /// </summary>
    public class Face
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Face(List<Vertex> line1 = null, List<Vertex> line2 = null, double dipAngle = 0, double direction = 0, double inclination = 0)
        {
            UpperBoundary = new List<Vertex>();
            LowerBoundary = new List<Vertex>();
            Boundary = new List<Vertex>();
            Triangles = new List<Triangle>();
            if (line1 != null && line2 != null)
            {
                UpperBoundary = line1;
                LowerBoundary = line2;
                GenTriangles(line1, line2);
            }
            DipAngle = dipAngle;
            Direction = direction;
            Inclination = inclination;
            Vector = Vertex.CalculateVector(DipAngle, Inclination);
        }
        /// <summary>
        /// 根据上下边界生成三角面片集合
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        private void GenTriangles(List<Vertex> line1, List<Vertex> line2)
        {
            Triangles = new List<Triangle>();
            //保证line1点数更多
            int m = line1.Count, n = line2.Count;
            int i = 0;
            //上边界点的索引i区间为index + 1,index + face.UpperBoundary.Count
            //所有三角面的顶点顺序为逆时针
            for (; i < Math.Min(m, n) - 1; i++)
            {
                Triangle triangle1 = new Triangle(line1[i], line1[i + 1], line2[i]);
                Triangle triangle2 = new Triangle(line2[i + 1], line2[i], line1[i + 1]);
                Triangles.Add(triangle1);
                Triangles.Add(triangle2);
            }
            //若上边界还有多余点，则依次与下边界右端点构成三角面
            while (i < m - 1)
            {
                Triangle triangle = new Triangle(line1[i], line1[i + 1], line2[n - 1]);
                Triangles.Add(triangle);
                i++;
            }
            //若下边界还有多余点，则依次与上边界右端点构成三角形
            while (i < n - 1)
            {
                Triangle triangle = new Triangle(line2[i + 1], line2[i], line1[m - 1]);
                Triangles.Add(triangle);
                i++;
            }
        }

        public List<Vertex> UpperBoundary { get; set; }//上边界

        public List<Vertex> LowerBoundary { get; set; }//下边界

        public List<Triangle> Triangles { get; set; }//三角面片集合

        public List<Vertex> Boundary { get; set; }//多边形面片集合，仅多边形有

        public double DipAngle { get; set; }//倾角

        public double Direction { get; set; }//走向

        public double Inclination { get; set; }//倾向

        public Vertex Vector { get; set; }//方向向量
    }
}
