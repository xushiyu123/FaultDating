using System;
using System.Collections.Generic;
using FaultStructureModeling.Entities.Geometry;
using FaultStructureModeling.Entities.Model;
using FaultStructureModeling.Controllers;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace FaultStructureModeling.Entities.Geography
{
    /// <summary>
    /// 地层实体类
    /// </summary>
    class Stratum
    {
        public int Id { get; set; }//地层编号
        public List<Boundary> Boundaries { get; set; }//地层界线集合
        public List<Face> SideFaces { get; set; }//层面集合
        public double Area { get; set; }//地层面积
        internal Face BottomFace { get; set; }//地层底面
        public List<Vertex> BottomBoundary { get; set; }//地层底面边界
        public List<Vertex> SurBoundary { get; set; }//地层裸露表面边界
        public Face Surface { get; set; }//上表面
        public double DipAngle { get; set; }//倾角
        public double Direction { get; set; }//走向
        public double Inclination { get; set; }//倾角
        public string Name { get; set; }
        public IPolygon Polygon { get; set; }

        public Stratum(double dipAngle = 0, double direction = 0, double inclination = 0)
        {
            Boundaries = new List<Boundary>();
            SideFaces = new List<Face>();
            BottomBoundary = new List<Vertex>();
            BottomFace = new Face();
            DipAngle = dipAngle;
            Direction = direction;
            Inclination = inclination;
        }
       
        /// <summary>
        /// 按照逆时针，调整边界方向
        /// </summary>
        public void AlterBoundary()
        {
            int counter = 1;
            int[] visited = new int[Boundaries.Count];
            for (int i = 0; i < Boundaries.Count; i++)
                visited[i] = 0;
            int pre = 0;
            visited[pre] = 1;
            while (counter < Boundaries.Count)
            {
                Vertex start = Boundaries[pre].Line[0];
                Vertex end = Boundaries[pre].Line[Boundaries[pre].Line.Count - 1];
                //调整边界方向
                for (int i = 0; i < Boundaries.Count; i++)
                {
                    if (visited[i] == 0)
                    {
                        Vertex a = Boundaries[i].Line[0];
                        Vertex b = Boundaries[i].Line[Boundaries[i].Line.Count - 1];
                        //该线段为顺时针
                        if (b.Equals(end, 0.1))
                        {
                            Boundaries[i].Line = Vertex.ReverseList(Boundaries[i].Line);
                            pre = i;
                            visited[i] = 1;
                            counter++;
                            break;
                        }
                        else if (a.Equals(end, 0.1))
                        {
                            pre = i;
                            visited[i] = 1;
                            counter++;
                            break;
                        }
                    }
                }
            }
            SortBoundary();
            MergeBoundary();
        }
        /// <summary>
        /// 调整边界顺序,以第一条边为起点，排序
        /// </summary>
        private void SortBoundary()
        {
            for (int i = 0; i < Boundaries.Count - 1; i++)
            {
                Vertex end = Boundaries[i].Line[Boundaries[i].Line.Count - 1];
                for (int j = i + 1; j < Boundaries.Count; j++)
                {
                    int count = Boundaries[j].Line.Count;
                    Vertex startj = Boundaries[j].Line[0];
                    Vertex endj = Boundaries[j].Line[count - 1];
                    if (end.Equals(startj) || end.Equals(endj))
                    {
                        if (end.Equals(endj))
                            Boundaries[j].Line = Vertex.ReverseList(Boundaries[j].Line);
                        //如果位置顺序，break
                        if (j == i + 1)
                            break;
                        // 否则，交换到i+1位
                        else
                        {
                            Boundary t = Boundaries[i + 1];
                            Boundaries[i + 1] = Boundaries[j];
                            Boundaries[j] = t;
                            break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 合并产状相同的边界线
        /// </summary>
        private void MergeBoundary()
        {
            int i = 0;
            //当至少有两条边界时
            while (i < Boundaries.Count && Boundaries.Count >= 2)
            {
                int j = (i + 1) % Boundaries.Count;
                //判断是否相同
                if (Boundaries[i].DipAngle == Boundaries[j].DipAngle && Boundaries[i].Direction == Boundaries[j].Direction)
                {
                    for (int k = 1; k < Boundaries[j].Line.Count; k++)
                        Boundaries[i].Line.Add(Boundaries[j].Line[k]);
                    Boundaries.RemoveAt(j);
                }
                else
                    i++;
            }
        }
        /// <summary>
        /// 不相邻的层面交切计算
        /// </summary>
        public void PinchSideFaces()
        {
            int count = SideFaces.Count;
            for (int i = 0; i < count; i++)
            {
                for (int j = i + 2; j < count; j++)
                {
                    //非相邻边
                    if (Math.Abs(i - j) >= 2 && Math.Abs(i - j) < count - 1)
                    {
                        //产状不同的层面作尖灭
                        if (!SideFaces[i].Vector.Equals(SideFaces[j].Vector))
                        {
                            //SideFaces[i] = FaceController.PinchOut(SideFaces[i], SideFaces[j]);
                            //SideFaces[j] = FaceController.PinchOut(SideFaces[j], SideFaces[i]);
                        }
                    }
                }
            }
        }
       
        /// <summary>
        /// 生成底面
        /// </summary>
        public void GenerateBottomFace()
        {
            for (int i = 0; i < SideFaces.Count; i++)
                BottomBoundary.AddRange(SideFaces[i].LowerBoundary);
            BottomFace.Boundary = BottomBoundary;
        }
        /// <summary>
        /// 计算地层面的几何中心
        /// </summary>
        /// <returns></returns>
        private Vertex Center()
        {
            //计算几何中心
            Vertex center = new Vertex(0, 0);
            int count = 0;
            for (int i = 0; i < Boundaries.Count; i++)
            {
                for (int j = 0; j < Boundaries[i].Line.Count; j++)
                {
                    count++;
                    Vertex p = Boundaries[i].Line[j];
                    center.X += p.X;
                    center.Y += p.Y;
                }
            }
            center.X /= count;
            center.Y /= count;
            return center;
        }
    }
}
