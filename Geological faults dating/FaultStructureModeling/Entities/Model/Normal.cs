//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FaultStructureModeling.Entities.Geometry
//{

//    class Normal
//    {
//        /// <summary>
//        /// 生成法线列表
//        /// </summary>
//        /// <param name="triangles"></param>
//        /// <param name="vertexes"></param>
//        /// <returns></returns>
//        public IList<Vertex> GenNormals(IList<Triangle> triangles, IList<Vertex> vertexes)
//        {
//            Vertex center = Center(vertexes);
//        }
//        /// <summary>
//        /// 计算中心点
//        /// </summary>
//        /// <param name="vertexes"></param>
//        /// <returns></returns>
//        public Vertex Center(IList<Vertex> vertexes)
//        {
//            int count = vertexes.Count;
//            Vertex center = new Vertex(0, 0, 0);
//            foreach (Vertex v in vertexes)
//            {
//                center += v;
//            }
//            center /= count;
//        }

//        /// <summary>
//        /// 计算法向量
//        /// </summary>
//        /// <param name="t"></param>
//        /// <returns></returns>
//        public Vertex Normal(Triangle t)
//        {
//            Vertex normal = Vertex.CrossProduct(t.b - t.a, t.c - t.b);//计算法向量
//            return Normalize(normal);
//        }
//        /// <summary>
//        /// 标准化
//        /// </summary>
//        /// <param name="v"></param>
//        /// <returns></returns>
//        public Vertex Normalize(Vertex v)
//        {
//            if (v.x * v.y * v.z != 0)
//            {
//                double length = Math.Sqrt(v.x * v.X.x + v.y * v.y + v.z * v.z);///计算向量的模
//                v.x /= length;
//                v.y /= length;
//                v.z /= length;
//            }
//            return v;
//        }
//    }
//}
