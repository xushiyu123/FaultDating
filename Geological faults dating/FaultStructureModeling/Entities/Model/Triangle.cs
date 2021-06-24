using System.Collections.Generic;
using System;

namespace FaultStructureModeling.Entities.Geometry
{
    /// <summary>
    /// 空间三角面
    /// </summary>
    public class Triangle
    {
        private Vertex a;

        private Vertex b;

        private Vertex c;

        private Vertex normal;

        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            normal = Vertex.CrossProduct(b - a, c - b);//计算法向量
            normal.Normal();
        }

        public Triangle()
        {

        }

        public Vertex A { get => a; set => a = value; }
        public Vertex B { get => b; set => b = value; }
        public Vertex C { get => c; set => c = value; }
        public Vertex Normal { get => normal; set => normal = value; }

        /// <summary>
        /// 计算空间线段与三角面的交点，参考：recast&detour源码/https://blog.csdn.net/u012138730/article/details/80235813
        /// </summary>
        /// <param name="tr">三角面</param>
        /// <param name="s">线段</param>
        /// <returns>交点</returns>
        public static Vertex IntersectSegment(Triangle tr, Segment s)
        {
            // 终点->起点 的向量
            Vertex ab = tr.B - tr.A;
            Vertex ac = tr.C - tr.A;
            Vertex qp = s.A - s.B;
            Vertex ap = s.A - tr.A;
            // 计算法线，方向为三角形正面方向 
            Vertex norm = Vertex.CrossProduct(ab, ac);//计算向量叉积
            // d = 0.0 说明 qp 和 norm 垂直，说明三角形和 qp 平行。
            // d < 0.0 说明 qp 和 norm 是钝角 说明是从三角形的背面 进入和三角形相交的 
            double d = Vertex.Dot(qp, norm);//计算向量点积
            if (d == 0) return Vertex.Empty();
            double t = Vertex.Dot(ap, norm);
            if (t == 0 || Math.Abs(t) >= Math.Abs(d)) return Vertex.Empty();
            if (Math.Abs(t) > Math.Abs(d)) return Vertex.Empty();
            Vertex e = Vertex.CrossProduct(qp, qp);
            double v = Vertex.Dot(ac, e);
            if (Math.Abs(v) > Math.Abs(d)) return Vertex.Empty();
            double w = -Vertex.Dot(ab, e);
            if (Math.Abs(v) + Math.Abs(w) > Math.Abs(d)) return Vertex.Empty();
            t /= d;
            Vertex p = s.A + t * (s.B - s.A);
            //判断交点是否在三角形内部
            if (SameSide(tr.A, tr.B, tr.C, p) && SameSide(tr.B, tr.C, tr.A, p) && SameSide(tr.C, tr.A, tr.B, p))
                return p;
            else
                return Vertex.Empty();
        }

        public static List<Triangle> IntersectTriangle()
        {
            List<Triangle> triangles = new List<Triangle>();
            //TODO
            return triangles;
        }

        /// <summary>
        /// 判断点与三角形一顶点是否在三角形一边
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <param name="P"></param>
        /// <returns></returns>
        private static bool SameSide(Vertex A, Vertex B, Vertex C, Vertex P)
        {
            Vertex AB = B - A;
            Vertex AC = C - A;
            Vertex AP = P - A;
            Vertex v1 = Vertex.CrossProduct(AB, AC);
            Vertex v2 = Vertex.CrossProduct(AB, AP);
            // v1 and v2 should point to the same direction
            return Vertex.Dot(v1, v2) >= 0;
        }
    }
}
