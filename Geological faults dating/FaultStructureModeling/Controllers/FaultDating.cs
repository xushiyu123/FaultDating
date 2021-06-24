using System;
using ESRI.ArcGIS.Geodatabase;
using System.Collections.Generic;
using FaultStructureModeling.Entities.Geometry;
using FaultStructureModeling.Entities;
using FaultStructureModeling.Entities.Geography;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.esriSystem;

namespace FaultStructureModeling.Controllers
{
    class FaultDating
    {
        public List<Fault> FaultSequence(IFeatureClass features)
        {
            List<Fault> faults = FeatureController.GenerateFaults(features);
            //获取最大断层ID（默认从0开始），得到断层数量
            int maxID = -1;
            for (int i = 0; i < faults.Count; i++)
            {
                if (maxID < faults[i].ID)
                    maxID = faults[i].ID;
            }
            int[,] adjMatrix = AdjMatrix(faults, maxID);
            int[] faultSeqence = new int[maxID + 1];
            int[] visited = new int[maxID + 1];
            int count = maxID + 1;
            int curIndex = 1;//当前批次
            for (int i = 0; i < maxID + 1; i++)
            {
                visited[i] = 0;
            }
            //计算断层批次
            while (count > 0)
            {
                //寻找当前最新一批的断层
                for (int i = 0; i < maxID + 1; i++)
                {
                    //未被排序过
                    if (visited[i] == 0)
                    {
                        bool flag = true;
                        for (int j = 0; j < maxID + 1; j++)
                        {
                            if (adjMatrix[i, j] > 0)
                            {
                                flag = false;
                                break;
                            }
                        }
                        //是最新的
                        if (flag)
                        {
                            faultSeqence[i] = curIndex;
                            visited[i] = -1;
                            count--;
                        }
                    }
                }
                //去除当前批次断层，并修改当前批的邻接关系
                for (int i = 0; i < faultSeqence.Length; i++)
                {
                    if (faultSeqence[i] == curIndex)
                    {
                        for (int j = 0; j < maxID + 1; j++)
                        {
                            if (adjMatrix[i, j] < 0)
                            {
                                adjMatrix[i, j] = 0;
                                adjMatrix[j, i] = 0;
                            }
                        }
                    }
                }
                //批次递增
                curIndex++;
            }
            for (int i = 0; i < faults.Count; i++)
            {
                Sequence s = (Sequence)faultSeqence[faults[i].ID];
                faults[i].Code = s.ToString();
            }
            return faults;
        }

        private int CrossCut(Fault f1, Fault f2)
        {
            int count = f1.Line.Count;
            if (Segment.IsPointOnLine(f1.Line[0], f2.Line) || Segment.IsPointOnLine(f1.Line[count - 1], f2.Line))
                return 1;
            else
                return 0;
        }

        public int[,] AdjMatrix(List<Fault> faults, int maxID)
        {
            //初始化穿切矩阵
            int[,] adjMatrix = new int[maxID + 1, maxID + 1];
            for (int i = 0; i < maxID + 1; i++)
            {
                for (int j = 0; j < maxID + 1; j++)
                    adjMatrix[i, j] = 0;
            }
            //计算邻接关系
            for (int i = 0; i < faults.Count; i++)
            {
                for (int j = 0; j < faults.Count; j++)
                {
                    if (i != j)
                    {
                        int value = CrossCut(faults[i], faults[j]);
                        int u = faults[i].ID, v = faults[j].ID;
                        adjMatrix[u, v] += value;
                        //adjMatrix[v, u] -= value;
                    }
                }
            }
            //计先后关系矩阵，0无先后关系，3早于，-3晚于
            for (int i = 0; i < maxID + 1; i++)
            {
                for (int j = 0; j < maxID + 1; j++)
                {
                    if (adjMatrix[i, j] == 1)
                    {
                        adjMatrix[j, i] = 3;
                        adjMatrix[i, j] = -3;
                    }
                    if (adjMatrix[i, j] == 2)
                    {
                        adjMatrix[i, j] = 3;
                        adjMatrix[j, i] = -3;
                    }
                }
            }
            return adjMatrix;
        }
        /// <summary>
        /// 间接定年法
        /// </summary>
        /// <param name="faultFeatures"></param>
        /// <param name="stratumFeatures"></param>
        /// <param name="strataTable"></param>
        public List<Fault> IndirectDating(IFeatureClass faultFeatures, IFeatureClass stratumFeatures, StrataTable strataTable)
        {
            GenerateBody(faultFeatures, stratumFeatures);
            List<Fault> faults = FeatureController.GenerateFaults(faultFeatures);
            Stratum[] strata = FeatureController.GenerateStrata(stratumFeatures);
            List<FaultStratum> faultStrata = GenerateBody(faultFeatures, stratumFeatures);
            for (int i = 0; i < faults.Count; i++)
            {
                int fid = i;
                //提出fid为i对应的盘体地层
                List<Stratum> pStrata = new List<Stratum>();
                for (int j = 0; j < faultStrata.Count; j++)
                {
                    if (fid == faultStrata[j].FaultID)
                        pStrata.Add(strata[faultStrata[j].StraID]);
                }
                //初步计算时间
                string start = strataTable.Strata[strataTable.Strata.Count - 1];
                string end = strataTable.Strata[0];
                for (int j = 0; j < pStrata.Count; j++)
                {
                    string name = pStrata[j].Name;
                    int id = Convert.ToInt32(strataTable.TimeTable[name]);
                    int pid = Convert.ToInt32(strataTable.TimeTable[start]);
                    if (id < pid)
                        start = name;
                }
                faults[i].Start = start;
                faults[i].End = end;
            }
            return faults;
        }

        //
        public List<FaultStratum> GenerateBody(IFeatureClass faults, IFeatureClass strata)
        {
            string bufferPath = Parameters.Workspace + @"\" + faults.AliasName + "_buffer.shp";
            string intersectPath = Parameters.Workspace + @"\" + faults.AliasName + "_intersect.shp";
            BufferTool(faults, 10, bufferPath);
            IntersectTool(strata, bufferPath, intersectPath);
            List<FaultStratum> faultStrata = FeatureController.GenerateFaultStrata(FeatureController.Reader(intersectPath));
            return faultStrata;
        }
        /// <summary>
        /// 求相交
        /// </summary>
        public void IntersectTool(IFeatureClass features1, string features2, string outPath)
        {
            Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;
            IGeoProcessorResult result = new GeoProcessorResultClass();
            Intersect intersect = new Intersect();
            IGpValueTableObject gpValueTableObject = new GpValueTableObjectClass();//对两个要素类进行相交运算 
            gpValueTableObject.SetColumns(2);
            object o1 = features1;//输入IFeatureClass 1  
            object o2 = features2;//输入IFeatureClass 2  
            gpValueTableObject.AddRow(ref o1);
            gpValueTableObject.AddRow(ref o2);
            intersect.in_features = gpValueTableObject;
            intersect.out_feature_class = outPath;
            gp.Execute(intersect, null);
        }
        /// <summary>
        /// 求buffer
        /// </summary>
        /// <param name="features"></param>
        /// <param name="distance"></param>
        /// <param name="outPath"></param>
        public void BufferTool(IFeatureClass features, double distance, string outPath)
        {
            Geoprocessor gp = new Geoprocessor();
            gp.OverwriteOutput = true;
            IGeoProcessorResult result = new GeoProcessorResultClass();
            ESRI.ArcGIS.AnalysisTools.Buffer buffer = new ESRI.ArcGIS.AnalysisTools.Buffer();
            buffer.in_features = features;
            buffer.out_feature_class = outPath;
            buffer.buffer_distance_or_field = distance;
            gp.Execute(buffer, null);
        }
    }
}
