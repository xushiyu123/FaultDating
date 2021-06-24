using System.Windows.Forms;

namespace FaultStructureModeling.Entities
{
    /// <summary>
    /// 全局变量
    /// </summary>
    class Parameters
    {
        public static string Workspace = Application.StartupPath;

        public static int Step = 30;//边界点插值步长

        public static double BottomElevation = -100;//默认下底面推演到-100m

        public static string TempDirectory = Application.StartupPath + @"\temp";//临时文件存储位置

        public static string Template = Application.StartupPath + @"\temp\template.shp";//3D地层属性矢量数据模板

        public static string SurfaceTemp = Application.StartupPath + @"\temp\surface";//上顶面中间数据保存位置
    }
}
