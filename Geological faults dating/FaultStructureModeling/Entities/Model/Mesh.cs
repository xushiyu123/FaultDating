using System;
using System.Collections;
using System.Collections.Generic;
using FaultStructureModeling.Entities.Geometry;

namespace FaultStructureModeling.Entities.Model
{
    public class Mesh
    {
        //哈希规则，key统一为坐标xyz值的拼接字符串
        Hashtable verticesHash;//存储顶点和顶点索引值的哈希

        List<Vertex> vertices;//顶点列表

        Hashtable normalsHash;//存储法线向量和法线索引值的哈希

        Vertex[] normals;//法线列表

        List<List<int>> topology;//拓扑关系，顶点邻接表

        List<Face> faces;//面数据

        string name;//模型名称

        string mtlpath;//材质路径
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="faces"></param>
        /// <param name="name"></param>
        public Mesh(string name = null, List<Face> faces = null)
        {
            verticesHash = new Hashtable();
            normalsHash = new Hashtable();
            vertices = new List<Vertex>();
            this.faces = new List<Face>();
            Topology = new List<List<int>>();
            if (name != null)
                this.name = name;
            if (faces != null)
            {
                this.faces = faces;
                GenHash();
                normals = new Vertex[vertices.Count];
                GenVerticeNormals();
            }
        }

        public Hashtable VerticesHash { get => verticesHash; set => verticesHash = value; }
        public Hashtable NormalsHash { get => normalsHash; set => normalsHash = value; }
        public List<Face> Faces { get => faces; set => faces = value; }
        public string Name { get => name; set => name = value; }
        public string Mtlpath { get => mtlpath; set => mtlpath = value; }
        public List<Vertex> Vertices { get => vertices; set => vertices = value; }
        public Vertex[] Normals { get => normals; set => normals = value; }
        public List<List<int>> Topology { get => topology; set => topology = value; }

        /// <summary>
        /// 构建顶点法线
        /// </summary>
        private void GenVerticeNormals()
        {
            //法向量中间数组
            List<Vertex>[] pNormals = new List<Vertex>[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
                pNormals[i] = new List<Vertex>();
            for (int i = 0; i < faces.Count; i++)
            {
                //顶点写入哈希
                if (faces[i].Boundary.Count == 0)
                {
                    for (int j = 0; j < faces[i].Triangles.Count; j++)
                    {
                        if (faces[i].Triangles[j].Normal == null)
                        {
                            faces[i].Triangles[j].Normal = Vertex.CrossProduct(faces[i].Triangles[j].B - faces[i].Triangles[j].A, faces[i].Triangles[j].C - faces[i].Triangles[j].A);
                            faces[i].Triangles[j].Normal.Normal();
                        }
                        int indexA = queryVertice(Faces[i].Triangles[j].A) - 1;
                        int indexB = queryVertice(Faces[i].Triangles[j].B) - 1;
                        int indexC = queryVertice(Faces[i].Triangles[j].C) - 1;
                        pNormals[indexA].Add(faces[i].Triangles[j].Normal);
                        pNormals[indexB].Add(faces[i].Triangles[j].Normal);
                        pNormals[indexC].Add(faces[i].Triangles[j].Normal);
                    }
                }
                else
                {
                    for (int j = 0; j < faces[i].Boundary.Count; j++)
                    {
                        int index = queryVertice(faces[i].Boundary[j]) - 1;
                        pNormals[index].Add(new Vertex(0, 0, -1));
                    }
                }
            }
            //计算顶点法向量，平均不加权
            for (int i = 0; i < vertices.Count; i++)
            {
                int count = pNormals[i].Count;
                Vertex normal = new Vertex(0, 0, 0);
                for (int j = 0; j < count; j++)
                {
                    normal += pNormals[i][j];
                }
                normal /= count;
                normal.Normal();//归一化
                normals[i] = normal;
            }
        }
        /// <summary>
        /// 构建哈希
        /// </summary>
        private void GenHash()
        {
            for (int i = 0; i < faces.Count; i++)
            {
                //顶点写入哈希
                if (faces[i].Boundary.Count == 0)
                {
                    for (int j = 0; j < faces[i].Triangles.Count; j++)
                    {
                        addVertice(faces[i].Triangles[j].A);
                        addVertice(faces[i].Triangles[j].B);
                        addVertice(faces[i].Triangles[j].C);
                    }
                }
                else
                    for (int j = 0; j < faces[i].Boundary.Count; j++)
                        addVertice(faces[i].Boundary[j]);
            }
        }
        /// <summary>
        /// 构建顶点拓扑
        /// </summary>
        private void GenTopology()
        {
            //拓扑列表初始化
            for (int i = 0; i < vertices.Count; i++)
            {
                List<int> list = new List<int>();
                topology.Add(list);
            }
            for (int i = 0; i < faces.Count; i++)
            {
                for (int j = 0; j < faces[i].Triangles.Count; j++)
                {
                    int indexA = queryVertice(faces[i].Triangles[j].A) - 1;
                    int indexB = queryVertice(faces[i].Triangles[j].B) - 1;
                    int indexC = queryVertice(faces[i].Triangles[j].C) - 1;
                    //修改A的邻接表
                    if (!topology[indexA].Contains(indexB))
                        topology[indexA].Add(indexB);
                    if (!topology[indexA].Contains(indexC))
                        topology[indexA].Add(indexC);
                    //修改B的邻接表
                    if (!topology[indexB].Contains(indexA))
                        topology[indexB].Add(indexA);
                    if (!topology[indexB].Contains(indexC))
                        topology[indexB].Add(indexC);
                    //修改C的邻接表
                    if (!topology[indexC].Contains(indexB))
                        topology[indexC].Add(indexB);
                    if (!topology[indexC].Contains(indexA))
                        topology[indexC].Add(indexA);
                }
            }
        }
        /// <summary>
        /// 查询顶点索引
        /// </summary>
        /// <param name="vertice"></param>
        /// <returns></returns>
        public int queryVertice(Vertex vertice)
        {
            string x = vertice.X.ToString("0.000000");
            string y = vertice.Y.ToString("0.000000");
            string z = vertice.Z.ToString("0.00");
            string key = x + "," + y + "," + z;
            if (verticesHash.Contains(key))
                return Convert.ToInt32(verticesHash[key]);
            else
                return -1;
        }
        /// <summary>
        /// 添加顶点哈希
        /// </summary>
        /// <param name="vertice"></param>
        public void addVertice(Vertex vertice)
        {
            string x = vertice.X.ToString("0.000000");
            string y = vertice.Y.ToString("0.000000");
            string z = vertice.Z.ToString("0.00");
            string key = x + "," + y + "," + z;
            if (!verticesHash.Contains(key))
            {
                verticesHash.Add(key, verticesHash.Count + 1);
                Vertices.Add(vertice);
            }
        }
        /// <summary>
        /// 添加法线哈希
        /// </summary>
        /// <param name="normal"></param>
        public void addNormal(Vertex normal)
        {
            string x = normal.X.ToString("0.000000");
            string y = normal.Y.ToString("0.000000");
            string z = normal.Z.ToString("0.00");
            string key = x + "," + y + "," + z;
            if (!normalsHash.Contains(key))
                normalsHash.Add(key, normalsHash.Count + 1);
        }
        /// <summary>
        /// 查询法线索引
        /// </summary>
        /// <param name="normal"></param>
        /// <returns></returns>
        public int queryNormal(Vertex normal)
        {
            string x = normal.X.ToString("0.000000");
            string y = normal.Y.ToString("0.000000");
            string z = normal.Z.ToString("0.00");
            string key = x + "," + y + "," + z;
            if (normalsHash.Contains(key))
                return Convert.ToInt32(normalsHash[key]);
            else
                return -1;
        }
    }
}
