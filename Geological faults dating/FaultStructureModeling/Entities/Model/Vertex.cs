using System;
using System.Collections.Generic;

namespace FaultStructureModeling.Entities.Geometry
{
    /// <summary>
    /// 空间点坐标或不确定起终点的向量
    /// </summary>
    public class Vertex
    {
        double x;
        double y;
        double z;
        /// <summary>
        /// 三维坐标构造函数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Vertex(double x = double.NaN, double y = double.NaN, double z = double.NaN)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
        public double Z { get => z; set => z = value; }
        /// <summary>
        /// 重载减号
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vertex operator -(Vertex a, Vertex b)
        {
            return new Vertex(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        /// <summary>
        /// 重载加号
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vertex operator +(Vertex a, Vertex b)
        {
            return new Vertex(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        /// <summary>
        /// 重载乘号
        /// </summary>
        /// <param name="k"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vertex operator *(double k, Vertex a)
        {
            return new Vertex(a.x * k, a.y * k, a.z * k);
        }
        /// <summary>
        /// 重载除号
        /// </summary>
        /// <param name="a"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static Vertex operator /(Vertex a, double k)
        {
            return new Vertex(a.x / k, a.y / k, a.z / k);
        }
        /// <summary>
        /// 坐标为空
        /// </summary>
        /// <returns></returns>
        public static Vertex Empty()
        {
            return new Vertex(double.NaN, double.NaN, double.NaN);
        }
        /// <summary>
        /// 判断是否为空坐标
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public bool IsEmpty()
        {
            if (double.IsNaN(x) && double.IsNaN(y) && double.IsNaN(z))
                return true;
            else
                return false;
        }
        /// <summary>
        /// 判断两点是否相同，容差默为0.001
        /// </summary>
        /// <param name="p"></param>
        /// <param name="diff">容差</param>
        /// <returns></returns>
        public bool Equals(Vertex p, double diff = 0.001)
        {
            double diffx = Math.Abs(x - p.x);
            double diffy = Math.Abs(y - p.y);
            double diffz = Math.Abs(z - p.z);
            if (diffx < diff && diffy < diff && diffz < diff)
                return true;
            return false;
        }
        /// <summary>
        /// 计算向量叉积
        /// </summary>
        /// <param name="a">向量a</param>
        /// <param name="b">向量b</param>
        /// <returns>叉积向量</returns>
        public static Vertex CrossProduct(Vertex a, Vertex b)
        {
            double x = a.Y * b.Z - b.Y * a.Z;// Y1Z2-Y2Z1,Z1X2-Z2X1,X1Y2-X2Y1
            double y = a.Z * b.X - a.X * b.Z;
            double z = a.X * b.Y - b.X * a.Y;
            return new Vertex(x, y, z);
        }
        /// <summary>
        /// 计算空间向量点积
        /// </summary>
        /// <param name="a">向量a</param>
        /// <param name="b">向量b</param>
        /// <returns>向量积</returns>
        public static double Dot(Vertex a, Vertex b)
        {
            return (a.X * b.X + a.Y * b.Y + a.Z * b.Z);
        }
        /// <summary>
        /// 空间两点间距离
        /// </summary>
        /// <param name="a">点a</param>
        /// <param name="b">点b</param>
        /// <returns>距离</returns>
        public static double Distance(Vertex a, Vertex b)
        {
            if (double.IsNaN(a.Z))
                return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
            else
                return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
        }
        /// <summary>
        /// 计算单位向量
        /// </summary>
        /// <param name="a">输入向量</param>
        /// <returns>单位向量</returns>
        public void Normal()
        {
            if (x * y * z != 0)
            {
                double length = Math.Sqrt(x * x + y * y + z * z);///计算向量的模
                x /= length;
                y /= length;
                z /= length;
            }
        }
        /// <summary>
        /// 长度
        /// </summary>
        /// <returns></returns>
        public double Magnitude()
        {
            return Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
        }
        /// <summary>
        /// 长度平方
        /// </summary>
        /// <returns></returns>
        public double SqrMagnitude()
        {
            return Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2);
        }

        /// <summary>
        /// 长度平方
        /// </summary>
        /// <returns></returns>
        public double SqrMagnitude2D()
        {
            return Math.Pow(x, 2) + Math.Pow(y, 2);
        }
        /// <summary>
        /// 根据产状计算推演的方向，单位向量
        /// </summary>
        /// <param name="dipAngle">倾角</param>
        /// <param name="inclination">走向</param>
        /// <returns></returns>
        public static Vertex CalculateVector(double dipAngle, double inclination)
        {
            double x = Math.Cos(inclination / 180 * Math.PI) * Math.Cos(dipAngle / 180 * Math.PI);
            double y = Math.Sin(inclination / 180 * Math.PI) * Math.Cos(dipAngle / 180 * Math.PI);
            double z = -Math.Sin(dipAngle / 180 * Math.PI);
            return new Vertex(x, y, z);
        }
        /// <summary>
        /// 判断空间点q是否与p1p2共线
        /// </summary>
        /// <param name="q"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns>true or flase</returns>
        public static bool IsPointOnLine(Vertex q, Vertex p1, Vertex p2)
        {
            if ((p1.X * q.Y - q.X * p1.Y) + (q.X * p2.Y - p2.X * q.Y) + (p2.X * p1.Y - p2.Y * p1.X) == 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 翻转点列
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Vertex> ReverseList(List<Vertex> list)
        {
            List<Vertex> rlist = new List<Vertex>();
            for (int i = list.Count - 1; i >= 0; i--)
                rlist.Add(list[i]);
            return rlist;
        }
    }
}
