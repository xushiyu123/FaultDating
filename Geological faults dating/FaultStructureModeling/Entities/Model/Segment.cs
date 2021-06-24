using System;
using System.Collections.Generic;

namespace FaultStructureModeling.Entities.Geometry
{
    /// <summary>
    /// 空间线段， 或确定起终点的向量
    /// </summary>
    public class Segment
    {
        private Vertex a;
        private Vertex b;

        public Segment(Vertex a = null, Vertex b = null)
        {
            this.a = a;
            this.b = b;
        }

        public Vertex A { get => a; set => a = value; }
        public Vertex B { get => b; set => b = value; }

        /// <summary>
        /// 计算空间线段与线段交点
        /// </summary>
        /// <param name="s1">线段1</param>
        /// <param name="s2">线段2</param>
        /// <returns>交点</returns>
        public static Vertex Intersect(Segment s1, Segment s2)
        {
            Vertex v1 = s1.b - s1.a;
            Vertex v2 = s2.b - s2.a;
            if (Vertex.Dot(v1, v2) == 1)
                // 两线平行
                return Vertex.Empty();
            Vertex startPointSeg = s2.a - s1.a;
            Vertex vecS1 = Vertex.CrossProduct(v1, v2);            // 有向面积1
            Vertex vecS2 = Vertex.CrossProduct(startPointSeg, v2); // 有向面积2
            double num = Vertex.Dot(startPointSeg, vecS1);
            // 判断两这直线是否共面
            if (num != 0)
                return Vertex.Empty();
            if (double.IsNaN(vecS1.SqrMagnitude()))
                return Vertex.Empty();
            // 有向面积比值，利用点乘是因为结果可能是正数或者负数
            double num2 = Vertex.Dot(vecS2, vecS1) / vecS1.SqrMagnitude();
            Vertex point = s1.a + num2 * v1;
            //判断交点是否在线段内
            if (!IsPointOnSegment(point, s1) || !IsPointOnSegment(point, s2))
                return Vertex.Empty();
            return point;
        }
        //public static Coordinate Intersect2D(Segment s1, Segment s2)
        //{
        //    Coordinate p1 = s1.a, p2 = s1.b, p3 = s2.a, p4 = s2.b;
        //    double f1 = fArea(p1, p2, p3), f2 = fArea(p1, p2, p4);
        //    Coordinate point = new Coordinate(Math.Round((p4.X * f1 + p3.X * f2) / (f1 + f2), 2),
        //        Math.Round((p4.Y * f1 + p3.Y * f2) / (f1 + f2), 2));
        //    if (!IsPointOnLine(point, s1) || !IsPointOnLine(point, s2))
        //        return Coordinate.Empty();
        //    return point;
        //}

        //private static double Cross(Coordinate p1, Coordinate p2, Coordinate p3, Coordinate p4)
        //{
        //    return (p2.X - p1.X) * (p4.Y - p3.Y) - (p2.Y - p1.Y) * (p4.X - p3.X);
        //}

        //private static double Area(Coordinate p1, Coordinate p2, Coordinate p3)
        //{
        //    return Cross(p1, p2, p1, p3);
        //}

        //private static double fArea(Coordinate p1, Coordinate p2, Coordinate p3)
        //{
        //    return Math.Abs(Area(p1, p2, p3));
        //}
        /// <summary>
        /// 两线段所在的射线相交
        /// </summary>
        /// <param name="s1">线段1</param>
        /// <param name="s2">线段2</param>
        /// <returns>交点</returns>
        public static Vertex RayIntersect(Segment s1, Segment s2)
        {
            Vertex v1 = s1.a - s1.b;
            Vertex v2 = s2.a - s2.b;
            if (Math.Abs(Vertex.Dot(v1, v2)) == 1)
            {
                // 两线平行
                return Vertex.Empty();
            }
            Vertex startPointSeg = s2.a - s1.a;
            Vertex vecS1 = Vertex.CrossProduct(v1, v2);            // 有向面积1
            Vertex vecS2 = Vertex.CrossProduct(startPointSeg, v2); // 有向面积2
            double num = Vertex.Dot(startPointSeg, vecS1);
            // 判断两这直线是否共面
            if (num >= 1E-02f || num <= -1E-02f)
            {
                return Vertex.Empty();
            }
            // 有向面积比值，利用点乘是因为结果可能是正数或者负数
            double num2 = Vertex.Dot(vecS2, vecS1) / vecS1.SqrMagnitude();
            return s1.a + num2 * s2.a;
        }
        /// <summary>
        /// 判断空间点是否在线段上
        /// </summary>
        /// <param name="p">点</param>
        /// <param name="s">线段</param>
        /// <returns>true or flase</returns>
        public static bool IsPointOnSegment(Vertex p, Segment s)
        {
            if ((p.X - s.A.X) * (p.X - s.B.X) <= 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 判断空间点是否在线上
        /// </summary>
        /// <param name="p">点</param>
        /// <param name="s">线段</param>
        /// <returns>true or flase</returns>
        public static bool IsPointOnLine(Vertex p, List<Vertex> l)
        {
            for (int i = 0; i < l.Count - 2; i++)
            {
                Vertex A = l[i], B = l[i + 1];
                if ((p.X - A.X) * (p.X - B.X) <= 0)//在内部
                {
                    Vertex v1 = p - A, v2 = p - B;
                    if (Math.Abs(v1.X * v2.Y - v2.X*v1.Y) < 0.1)
                    {
                        //向量共线
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
