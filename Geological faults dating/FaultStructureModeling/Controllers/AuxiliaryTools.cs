using ESRI.ArcGIS.Carto;
using System;
using System.IO;
using System.Windows.Forms;
using FaultStructureModeling.Entities;

namespace FaultStructureModeling.Controllers
{
    /// <summary>
    /// 辅助工具类
    /// </summary>
    class AuxiliaryTools
    {
        /// <summary>
        /// 循环递归删除文件夹下的所有文件
        /// </summary>
        /// <param name="folderName">文件夹路径</param>
        public static void ClearFolder(string folderName)
        {
            DirectoryInfo d = new DirectoryInfo(folderName);
            FileInfo[] files = d.GetFiles();//文件
            DirectoryInfo[] directs = d.GetDirectories();//文件夹
            foreach (FileInfo f in files)
            {
                File.Delete(f.FullName);//添加文件名到列表中
            }
            //获取子文件夹内的文件列表，递归遍历  
            foreach (DirectoryInfo dd in directs)
            {
                ClearFolder(dd.FullName);
                Directory.Delete(dd.FullName);
            }
        }
        /// <summary>
        /// 文件夹浏览器，选择文件夹
        /// </summary>
        /// <param name="root">起始路径</param>
        /// <returns>文件夹路径</returns>
        public static string FolderBrowser(string root)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog
            {
                SelectedPath = root
            };
            string folderName = "";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                folderName = folderBrowser.SelectedPath;
            }
            return folderName;
        }
        /// <summary>
        /// 浏览文件夹，选择文件
        /// </summary>
        /// <param name="root">起始目录</param>
        /// <param name="fliter">文件格式筛选</param>
        /// <returns>文件路径</returns>
        public static string FileBrowser(string root, string fliter)
        {
            string fileName = string.Empty;
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = fliter,
                InitialDirectory = root
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fileName = ofd.FileName;
            }
            return fileName;
        }
        /// <summary>
        /// 通过图层名称获取目标图层
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public static ILayer GetLayerByName(string layerName, IMap map)
        {
            // 通过图层名称获取目标图层，传入参数为layerName
            ILayer pLayer = null;
            //遍历MapControl中所有图层，找到与layerName名称相同的图层
            for (int i = 0; i < map.LayerCount; i++)
            {
                if (map.get_Layer(i).Name == layerName)
                {
                    pLayer = map.get_Layer(i);
                    break;
                }
            }
            return pLayer;
        }

        public static void Config()
        {
            StreamReader reader = new StreamReader(Application.StartupPath + @"\config.txt");
            Parameters.Workspace = reader.ReadLine();
            Parameters.TempDirectory = reader.ReadLine();
            Parameters.Step = Convert.ToInt32(reader.ReadLine());
            reader.Close();
        }
    }
}
