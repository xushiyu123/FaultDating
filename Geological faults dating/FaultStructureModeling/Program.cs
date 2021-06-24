using System;
using System.Windows.Forms;
using FaultStructureModeling.Controllers;

namespace FaultStructureModeling
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //绑定AE证书
            ESRI.ArcGIS.RuntimeManager.BindLicense(ESRI.ArcGIS.ProductCode.EngineOrDesktop, ESRI.ArcGIS.LicenseLevel.Standard);
            AuxiliaryTools.Config();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());           
        }
    }
}
