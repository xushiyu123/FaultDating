using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using FaultStructureModeling.Entities;
using FaultStructureModeling.Entities.Geography;
using FaultStructureModeling.Entities.Geometry;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GeoJson;

namespace FaultStructureModeling.Controllers
{
    /// <summary>
    /// 要素类相关控制类
    /// </summary>
    class FeatureController
    {
        /// <summary>
        /// 点列转二维面
        /// </summary>
        /// <param name="points"></param>
        /// <param name="name"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static IFeatureClass Points2Surface(IFeature feature, string name, string folder)
        {
            IPolygon polygon = feature.Shape as IPolygon;
            IPointCollection ipc = polygon as IPointCollection;
            IFeatureClass pFC = ShpFromIPC(folder, name, ipc);
            return pFC;
        }
        /// <summary>
        /// 点列转三维面
        /// </summary>
        /// <param name="points"></param>
        /// <param name="name"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static IFeatureClass Points23dSurface(List<Vertex> points, string name, string folder)
        {
            IPointCollection ipc = PolygonFromPoints(points);
            string surfacePath = Create3dSurface(name, folder);
            IFeatureClass pFC = Reader(surfacePath);
            IFeature pFeature = pFC.CreateFeature();
            IPolygon polygon = ipc as IPolygon;
            int index = pFeature.Fields.FindField("Shape");
            IGeometryDef pGeometryDef = pFeature.Fields.get_Field(index).GeometryDef as IGeometryDef;
            if (pGeometryDef.HasZ)
            {
                IZAware zAware = polygon as IZAware;
                zAware.ZAware = true;
                pFeature.Shape = zAware as IPolygon;
                pFeature.Store();
            }
            return pFC;
        }
        /// <summary>
        /// 将点列创建面要素并保存至本地
        /// </summary>
        /// <param name="shpFolder">面要素保存地址</param>
        /// <param name="shpName">要素名称</param>
        /// <param name="ipc">点列</param>
        /// <returns>是否执行完毕</returns>
        public static IFeatureClass ShpFromIPC(string shpFolder, string shpName,
            IPointCollection ipc)
        {
            if (File.Exists(shpFolder + shpName + ".shp"))
            {
                File.Delete(shpFolder + shpName + ".shp");
                File.Delete(shpFolder + shpName + ".sbn");
                File.Delete(shpFolder + shpName + ".sbx");
                File.Delete(shpFolder + shpName + ".dbf");
                File.Delete(shpFolder + shpName + ".shx");
                File.Delete(shpFolder + shpName + ".shp.xml");
            }
            IWorkspaceFactory pWF = new ShapefileWorkspaceFactoryClass();
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = fcDescription as IObjectClassDescription;
            IFields pFields = new Fields();
            IGeometryDef pGeometryDef = new GeometryDefClass();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new Field();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = "SHAPE";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFieldEdit.GeometryDef_2 = pGeometryDef;
            pFieldEdit.IsNullable_2 = true;
            pFieldsEdit.AddField(pField);
            IFeatureWorkspace pFWs = pWF.OpenFromFile(shpFolder, 0) as IFeatureWorkspace;
            IFeatureClass pFC = pFWs.CreateFeatureClass(shpName, pFields, null,
                null, esriFeatureType.esriFTSimple, "SHAPE", "");
            IFeature pFeature = pFC.CreateFeature();
            pFeature.Shape = ipc as IPolygon;
            pFeature.Store();
            Marshal.ReleaseComObject(pFeature);
            Marshal.ReleaseComObject(pWF);
            Marshal.ReleaseComObject(pFC);
            Marshal.ReleaseComObject(pFWs);
            Marshal.ReleaseComObject(pFields);
            Marshal.ReleaseComObject(pGeometryDef);
            Marshal.ReleaseComObject(pGeometryDefEdit);
            return Reader(shpFolder + @"\" + shpName);
        }
        /// <summary>
        /// 基于GP工具创建三维面
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="name"></param>
        /// <param name="folder"></param>
        public static string Create3dSurface(string name, string folder)
        {
            Geoprocessor gp = new Geoprocessor();//创建GP工具对象
            gp.OverwriteOutput = true;
            CreateFeatureclass create = new CreateFeatureclass();
            create.geometry_type = "POLYGON";
            create.template = Parameters.Template;
            create.has_z = "ENABLED";
            create.has_m = "DISABLED";
            create.out_name = name;
            create.out_path = folder;
            GeoProcessorResult gpResult = new GeoProcessorResult();
            try
            {
                gp.Execute(create, null);
            }
            catch (COMException e)
            {
                string message = "";
                for (int i = 0; i < gp.MessageCount; i++)
                {
                    message += gp.GetMessage(i) + "\r\n";
                }
                MessageBox.Show(message + e.ToString());
            }
            if (!folder.EndsWith(@"\"))
                folder += @"\";
            return folder + name;
        }

        /// <summary>
        /// 读取矢量要素
        /// </summary>
        /// <param name="featureFile">要素文件</param>
        /// <returns>矢量类容器</returns>
        public static IFeatureClass Reader(string featureFile)
        {
            IFeatureClass ifc = null;
            try
            {
                if (featureFile != "")
                {
                    IWorkspaceFactory iwf = new ShapefileWorkspaceFactoryClass();
                    IWorkspaceFactoryLockControl ipWsFactoryLock = (IWorkspaceFactoryLockControl)iwf;
                    if (ipWsFactoryLock.SchemaLockingEnabled)
                    {
                        ipWsFactoryLock.DisableSchemaLocking();
                    }
                    IWorkspace iw = iwf.OpenFromFile(System.IO.Path.GetDirectoryName(featureFile), 0);
                    IFeatureWorkspace ifw = iw as IFeatureWorkspace;
                    if (ifw != null)
                    {
                        ifc = ifw.OpenFeatureClass(System.IO.Path.GetFileName(featureFile));
                    }
                    //释放过程对象
                    Marshal.ReleaseComObject(iwf);
                    Marshal.ReleaseComObject(iw);
                    Marshal.ReleaseComObject(ifw);
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            return ifc;
        }
        /// <summary>
        /// 根据属性名从要素属性中取出对应值
        /// </summary>
        /// <param name="feature">要素</param>
        /// <param name="fieldName">属性名</param>
        /// <returns>属性值</returns>
        private static string ValueFromField(IFeature feature, string fieldName)
        {
            int fieldNum = feature.Fields.FindField(fieldName);
            string value = feature.Value[fieldNum].ToString();
            return value;
        }
        /// <summary>
        /// 点列转ipointcollection
        /// </summary>
        /// <param name="points">点列</param>
        /// <returns></returns>
        private static IPointCollection PolygonFromPoints(List<Vertex> points)
        {
            IPolygon polygon = new PolygonClass();
            IPointCollection pointCollection = polygon as IPointCollection;
            for (int i = 0; i < points.Count; i++)
            {
                IPoint point = new PointClass();
                point.X = points[i].X; point.Y = points[i].Y; point.Z = points[i].Z;
                pointCollection.AddPoint(point);
            }
            return pointCollection;
        }
        /// <summary>
        /// 点列转ipointcollection
        /// </summary>
        /// <param name="points">点列</param>
        /// <returns></returns>
        private static IPointCollection PolylineFromPoints(List<Vertex> points)
        {
            IPolyline polyline = new PolylineClass();
            IPointCollection pointCollection = polyline as IPointCollection;
            for (int i = 0; i < points.Count; i++)
            {
                IPoint point = new PointClass();
                point.X = points[i].X; point.Y = points[i].Y; point.Z = points[i].Z;
                pointCollection.AddPoint(point);
            }
            return pointCollection;
        }
        /// <summary>
        /// 翻转点列
        /// </summary>
        /// <param name="line"></param>
        /// <returns>翻转的结果</returns>
        public static List<Vertex> ReverseLine(List<Vertex> line)
        {
            List<Vertex> coordinates = new List<Vertex>();
            for (int i = line.Count - 1; i >= 0; i--)
            {
                coordinates.Add(line[i]);
            }
            return coordinates;
        }
        /// <summary>
        /// 为二维要素赋予Z值
        /// </summary>
        /// <param name="inFeature"></param>
        /// <param name="raster"></param>
        /// <param name="outFeature"></param>
        /// <returns></returns>
        public static IFeatureClass InterpolateShape3d(IFeatureClass inFeature, IRaster raster, string outFeature)
        {
            Geoprocessor gp = new Geoprocessor();//创建GP工具对象
            gp.OverwriteOutput = true;
            ESRI.ArcGIS.Analyst3DTools.InterpolateShape interpolate = new ESRI.ArcGIS.Analyst3DTools.InterpolateShape();
            interpolate.in_feature_class = inFeature;
            interpolate.in_surface = raster;
            interpolate.out_feature_class = outFeature;
            GeoProcessorResult gpResult = new GeoProcessorResult();
            try
            {
                gp.Execute(interpolate, null);
            }
            catch (COMException e)
            {
                string message = "";
                for (int i = 0; i < gp.MessageCount; i++)
                {
                    message += gp.GetMessage(i) + "\r\n";
                }
                MessageBox.Show(message + e.ToString());
            }
            IFeatureClass featureClass = Reader(outFeature);
            return featureClass;
        }

        public static List<FaultStratum> GenerateFaultStrata(IFeatureClass features)
        {
            List<FaultStratum> faultStrata = new List<FaultStratum>();
            try
            {
                int fidIndex = features.FindField("FID_faults");
                int sidIndex = features.FindField("FID_strata");
                for (int i = 0; i < features.FeatureCount(null); i++)
                {
                    FaultStratum faultStratum = new FaultStratum();
                    IFeature feature = features.GetFeature(i);
                    faultStratum.FaultID = Convert.ToInt32(feature.get_Value(fidIndex));
                    faultStratum.StraID = Convert.ToInt32(feature.get_Value(sidIndex));
                    faultStrata.Add(faultStratum);
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            return faultStrata;
        }
        /// <summary>
        /// 基于断层线要素生成断层对象
        /// </summary>
        /// <param name="features">断层线要素</param>
        /// <returns></returns>
        public static List<Fault> GenerateFaults(IFeatureClass features)
        {
            List<Fault> faults = new List<Fault>();
            try
            {
                int angleIndex = features.FindField("dip");
                int direcIndex = features.FindField("strike");
                int inclineIndex = features.FindField("incli");
                int widthIndex = features.FindField("width");
                int IdIndex = features.FindField("Id");
                int codeIndex = features.FindField("code");
                int startIndex = features.FindField("start");
                int endIndex = features.FindField("end");
                for (int i = 0; i < features.FeatureCount(null); i++)
                {
                    Fault fault = new Fault();
                    IFeature feature = features.GetFeature(i);
                    IPolyline line = feature.Shape as IPolyline;
                    IPointCollection ipc = line as IPointCollection;
                    List<Vertex> coordinates = new List<Vertex>();
                    for (int j = 0; j < ipc.PointCount; j++)
                    {
                        Vertex coordinate = new Vertex(ipc.Point[j].X, ipc.Point[j].Y, double.NaN);
                        coordinates.Add(coordinate);
                    }
                    fault.Line = coordinates;
                    if (IdIndex > -1)
                        fault.ID = Convert.ToInt32(feature.get_Value(IdIndex));
                    if (angleIndex > -1)
                        fault.DipAngle = Convert.ToDouble(feature.get_Value(angleIndex));
                    if (direcIndex > -1)
                        fault.Direction = Convert.ToDouble(feature.get_Value(direcIndex));
                    if (inclineIndex > -1)
                        fault.Inclination = Convert.ToDouble(feature.get_Value(inclineIndex));
                    if (widthIndex > -1)
                        fault.Width = Convert.ToDouble(feature.get_Value(widthIndex));
                    if (codeIndex > -1)
                        fault.Code = Convert.ToString(feature.get_Value(codeIndex));
                    if (startIndex > -1)
                        fault.Start = Convert.ToString(feature.get_Value(startIndex));
                    if (endIndex > -1)
                        fault.End = Convert.ToString(feature.get_Value(endIndex));
                    fault.Vector = Vertex.CalculateVector(fault.DipAngle, fault.Direction);
                    faults.Add(fault);
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
            return faults;
        }
       
       
        /// <summary>
        /// 基于地层面要素创建地层对象数组
        /// </summary>
        /// <param name="features"></param>
        /// <returns></returns>
        public static Stratum[] GenerateStrata(IFeatureClass features)
        {
            Stratum[] strata = new Stratum[features.FeatureCount(null)];
            int idIndex = features.FindField("Id");
            int areaIndex = features.FindField("area");
            int angleIndex = features.FindField("dip");
            int direcIndex = features.FindField("strike");
            int inclineIndex = features.FindField("incli");
            int nameIndex = features.FindField("name");
            for (int i = 0; i < features.FeatureCount(null); i++)
            {
                Stratum stratum = new Stratum();
                IFeature feature = features.GetFeature(i);
                stratum.Id = Convert.ToInt32(feature.get_Value(idIndex));
                if (areaIndex > -1)
                    stratum.Area = Convert.ToDouble(feature.get_Value(areaIndex));
                stratum.DipAngle = Convert.ToDouble(feature.get_Value(angleIndex));
                stratum.Direction = Convert.ToDouble(feature.get_Value(direcIndex));
                stratum.Inclination = Convert.ToDouble(feature.get_Value(inclineIndex));
                if (nameIndex > -1)
                    stratum.Name = feature.get_Value(nameIndex).ToString();
                strata[i] = stratum;
            }
            return strata;
        }

        public static GeoJson.GeoJson Featuress2Json(IFeatureClass ifeatures)
        {
            GeoJson.GeoJson json = new GeoJson.GeoJson(FeatureType.FeatureCollection);
            for (int i = 0; i < ifeatures.FeatureCount(null); i++)
            {
                GeoJson.Feature feature = new GeoJson.Feature(FeatureType.Feature);
                IFeature ifeature = ifeatures.GetFeature(i);
                GeoJson.Geometry geometry = new GeoJson.Geometry(EsriType2JsonType(ifeature.Shape.GeometryType));

            }
            return json;
        }

        private static GeometryType EsriType2JsonType(esriGeometryType type)
        {
            switch (type)
            {
                case esriGeometryType.esriGeometryPoint: return GeometryType.Point;
                case esriGeometryType.esriGeometryPolyline: return GeometryType.LineString;
                case esriGeometryType.esriGeometryLine: return GeometryType.LineString;
                case esriGeometryType.esriGeometryPolygon: return GeometryType.Polygon;
                case esriGeometryType.esriGeometryMultipoint: return GeometryType.MultiPoint;
                case esriGeometryType.esriGeometryRing: return GeometryType.LineString;
                case esriGeometryType.esriGeometryTriangles: return GeometryType.MultiPlygon;
                default: return GeometryType.MultiPoint;
            }
        }
        /// <summary>
        /// 矢量线转二维点集
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public static List<Vertex> Polyline2Line(IFeature feature)
        {
            List<Vertex> vertices = new List<Vertex>();
            IPolyline polyline = feature.Shape as IPolyline;
            IPointCollection points = polyline as IPointCollection;
            for (int i = 0; i < points.PointCount; i++)
            {
                Vertex v = new Vertex(points.Point[i].X, points.Point[i].Y);
                vertices.Add(v);
            }
            return vertices;
        }
        /// <summary>
        /// 地质界线插值
        /// </summary>
        public static List<List<Vertex>> LineInterpolate(List<Vertex> start, List<Vertex> end, double k)
        {
            Vertex p = start[0];
            Vertex q = end[0];
            //偏移量基准点
            int count = start.Count;
            double dx = p.X - q.X;
            double dy = p.Y - q.Y;
            List<List<Vertex>> interLines = new List<List<Vertex>>();
            for (int i = 0; i < k; i++)
            {
                List<Vertex> inter = new List<Vertex>();
                for (int j = 0; j < count; j++)
                {
                    double ddx = dx * Convert.ToDouble(i + 1) / (k + 1) * (count - 1 - j) / (count - 1);
                    double ddy = dy * Convert.ToDouble(i + 1) / (k + 1) * (count - 1 - j) / (count - 1);
                    Vertex v = new Vertex(end[j].X + ddx, end[j].Y + ddy);
                    inter.Add(v);
                }
                interLines.Add(inter);
            }
            return interLines;
        }

        public static void ExportFaults(string shpFolder, string shpName, List<Fault> faults)
        {
            if (File.Exists(shpFolder + shpName + ".shp"))
            {
                File.Delete(shpFolder + shpName + ".shp");
                File.Delete(shpFolder + shpName + ".sbn");
                File.Delete(shpFolder + shpName + ".sbx");
                File.Delete(shpFolder + shpName + ".dbf");
                File.Delete(shpFolder + shpName + ".shx");
                File.Delete(shpFolder + shpName + ".shp.xml");
            }
            IWorkspaceFactory pWF = new ShapefileWorkspaceFactoryClass();
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = fcDescription as IObjectClassDescription;
            IFields pFields = new Fields();
            IGeometryDef pGeometryDef = new GeometryDefClass();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new Field();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = "SHAPE";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFieldEdit.GeometryDef_2 = pGeometryDef;
            pFieldEdit.IsNullable_2 = true;
            pFieldsEdit.AddField(pField);
            AddField(ref pFields, "Id", esriFieldType.esriFieldTypeInteger);
            AddField(ref pFields, "dip", esriFieldType.esriFieldTypeInteger);
            AddField(ref pFields, "strike", esriFieldType.esriFieldTypeInteger);
            AddField(ref pFields, "incli", esriFieldType.esriFieldTypeInteger);
            AddField(ref pFields, "width", esriFieldType.esriFieldTypeInteger);
            AddField(ref pFields, "code", esriFieldType.esriFieldTypeString);
            AddField(ref pFields, "start", esriFieldType.esriFieldTypeString);
            AddField(ref pFields, "end", esriFieldType.esriFieldTypeString);
            IFeatureWorkspace pFWs = pWF.OpenFromFile(shpFolder, 0) as IFeatureWorkspace;
            IFeatureClass pFC = pFWs.CreateFeatureClass(shpName, pFields, null,
                null, esriFeatureType.esriFTSimple, "SHAPE", "");
            IFeature pFeature = null;
            for (int i = 0; i < faults.Count; i++)
            {
                pFeature = pFC.CreateFeature();
                pFeature.Shape = faults[i].Line as IPolyline;
                pFeature.Value[2] = faults[i].ID;
                pFeature.Value[3] = faults[i].DipAngle;
                pFeature.Value[4] = faults[i].Direction;
                pFeature.Value[5] = faults[i].Inclination;
                pFeature.Value[6] = faults[i].Width;
                pFeature.Value[7] = faults[i].Code;
                pFeature.Value[8] = faults[i].Start;
                pFeature.Value[9] = faults[i].End;
                pFeature.Store();
            }
            IWorkspaceFactoryLockControl control = pWF as IWorkspaceFactoryLockControl;
            if (control.SchemaLockingEnabled)
            {
                control.DisableSchemaLocking();
            }
        }

        public static void ExportStrata(string shpFolder, string shpName, List<Stratum> strata)
        {
            if (File.Exists(shpFolder + shpName + ".shp"))
            {
                File.Delete(shpFolder + shpName + ".shp");
                File.Delete(shpFolder + shpName + ".sbn");
                File.Delete(shpFolder + shpName + ".sbx");
                File.Delete(shpFolder + shpName + ".dbf");
                File.Delete(shpFolder + shpName + ".shx");
                File.Delete(shpFolder + shpName + ".shp.xml");
            }
            IWorkspaceFactory pWF = new ShapefileWorkspaceFactoryClass();
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = fcDescription as IObjectClassDescription;
            IFields pFields = new Fields();
            IGeometryDef pGeometryDef = new GeometryDefClass();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new Field();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = "SHAPE";
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFieldEdit.GeometryDef_2 = pGeometryDef;
            pFieldEdit.IsNullable_2 = true;
            pFieldsEdit.AddField(pField);
            AddField(ref pFields, "Id", esriFieldType.esriFieldTypeInteger);
            AddField(ref pFields, "dip", esriFieldType.esriFieldTypeInteger);
            AddField(ref pFields, "strike", esriFieldType.esriFieldTypeInteger);
            AddField(ref pFields, "incli", esriFieldType.esriFieldTypeInteger);
            AddField(ref pFields, "name", esriFieldType.esriFieldTypeString);
            IFeatureWorkspace pFWs = pWF.OpenFromFile(shpFolder, 0) as IFeatureWorkspace;
            IFeatureClass pFC = pFWs.CreateFeatureClass(shpName, pFields, null,
                null, esriFeatureType.esriFTSimple, "SHAPE", "");
            IFeature pFeature = null;
            for (int i = 0; i < strata.Count; i++)
            {
                pFeature = pFC.CreateFeature();
                pFeature.Shape = strata[i].Polygon as IPolygon;
                pFeature.Value[2] = strata[i].Id;
                pFeature.Value[3] = strata[i].DipAngle;
                pFeature.Value[4] = strata[i].Direction;
                pFeature.Value[5] = strata[i].Inclination;
                pFeature.Value[6] = strata[i].Name;
                pFeature.Store();
            }
            IWorkspaceFactoryLockControl control = pWF as IWorkspaceFactoryLockControl;
            if (control.SchemaLockingEnabled)
            {
                control.DisableSchemaLocking();
            }
        }
        /// <summary>
        /// add attribute field
        /// </summary>
        /// <param name="pFields"></param>
        /// <param name="FieldName"></param>
        /// <param name="esriFieldType"></param>
        public static void AddField(ref IFields pFields, string FieldName, esriFieldType esriFieldType)
        {
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new Field();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = FieldName;
            pFieldEdit.Type_2 = esriFieldType;
            pFieldEdit.IsNullable_2 = true;
            pFieldsEdit.AddField(pField);
        }

        public static void InterLine2Shp(List<Vertex> side1, List<Vertex> side2, List<Vertex> inters1, List<Vertex> inters2,
            List<Vertex> inters3, List<Vertex> inters4, string shpFolder, string shpName)
        {
            if (File.Exists(shpFolder + shpName + ".shp"))
            {
                File.Delete(shpFolder + shpName + ".shp");
                File.Delete(shpFolder + shpName + ".sbn");
                File.Delete(shpFolder + shpName + ".sbx");
                File.Delete(shpFolder + shpName + ".dbf");
                File.Delete(shpFolder + shpName + ".shx");
                File.Delete(shpFolder + shpName + ".shp.xml");
            }
            IWorkspaceFactory pWF = new ShapefileWorkspaceFactoryClass();
            IFeatureClassDescription fcDescription = new FeatureClassDescriptionClass();
            IObjectClassDescription ocDescription = fcDescription as IObjectClassDescription;
            IFields pFields = new Fields();
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new Field();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            IGeometryDef pGeometryDef = new GeometryDefClass();
            IGeometryDefEdit pGeometryDefEdit = pGeometryDef as IGeometryDefEdit;
            pGeometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFieldEdit.GeometryDef_2 = pGeometryDef;
            pFieldEdit.Name_2 = "SHAPE";
            pFieldsEdit.AddField(pField);
            IFeatureWorkspace pFWs = pWF.OpenFromFile(shpFolder, 0) as IFeatureWorkspace;
            IFeatureClass pFC = pFWs.CreateFeatureClass(shpName, pFields, null,
                null, esriFeatureType.esriFTSimple, "Shape", "");
            IFeature pFeature = null;
            pFeature = pFC.CreateFeature();
            pFeature.Shape = PolylineFromPoints(side1) as IPolyline;
            pFeature.Store();
            pFeature = pFC.CreateFeature();
            pFeature.Shape = PolylineFromPoints(side2) as IPolyline;
            pFeature.Store();
            pFeature = pFC.CreateFeature();
            pFeature.Shape = PolylineFromPoints(inters1) as IPolyline;
            pFeature.Store();
            pFeature = pFC.CreateFeature();
            pFeature.Shape = PolylineFromPoints(inters2) as IPolyline;
            pFeature.Store();
            pFeature = pFC.CreateFeature();
            pFeature.Shape = PolylineFromPoints(inters3) as IPolyline;
            pFeature.Store();
            pFeature = pFC.CreateFeature();
            pFeature.Shape = PolylineFromPoints(inters4) as IPolyline;
            pFeature.Store();
            Marshal.ReleaseComObject(pFeature);
            Marshal.ReleaseComObject(pWF);
            Marshal.ReleaseComObject(pFC);
            Marshal.ReleaseComObject(pFWs);
            Marshal.ReleaseComObject(pFields);
            Marshal.ReleaseComObject(pGeometryDef);
            Marshal.ReleaseComObject(pGeometryDefEdit);
        }
    }
}
