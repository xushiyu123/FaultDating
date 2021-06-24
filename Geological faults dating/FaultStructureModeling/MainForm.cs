using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using FaultStructureModeling.Views;
using FaultStructureModeling.Controllers;
using System;
using System.Windows.Forms;

namespace FaultStructureModeling
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            axTOCControl1.SetBuddyControl(axMapControl1);
        }

        private void AxMapControl1_OnMapReplaced(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMapReplacedEvent e)
        {
            //主地图有地图或图层的时候鹰眼加载图层
            if (axMapControl1.LayerCount > 0)
            {
                axMapControl2.ClearLayers(); //先清除鹰眼的地图
                                             //图层自下而上加载，防止要素间互相压盖
                for (int i = axMapControl1.Map.LayerCount - 1; i >= 0; i--)
                {
                    axMapControl2.AddLayer(axMapControl1.get_Layer(i));
                }
                //设置鹰眼地图鱼主地图相同空间参考系
                //必要：防止由于图层放置顺序改变而改变了鹰眼的空间参考系
                axMapControl2.SpatialReference = axMapControl1.SpatialReference;
                //设置鹰眼的显示范围=完整显示（FullExtent)
                axMapControl2.Extent = axMapControl2.FullExtent;
                //每次加载或者删除图层之后都要刷新一次MapControl
                axMapControl2.Refresh();
            }
        }

        private void AxMapControl1_OnExtentUpdated(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            //刷新axMapControl2
            axMapControl2.Refresh();
            //以主地图的Extent作为鹰眼红线框的大小范围
            IEnvelope pEnvelope = axMapControl1.Extent;
            //鹰眼强制转换为IGraphicsContainer
            //IGraphicsContainer是绘图容器接口, 主要功能是在MapControl控件类上添加绘图要素。
            IGraphicsContainer pGraphicsContainer = axMapControl2.Map as IGraphicsContainer;
            //鹰眼强制转换为pActiveView
            IActiveView pActiveView = pGraphicsContainer as IActiveView;
            //删除鹰眼原有要素
            pGraphicsContainer.DeleteAllElements();
            //实例化矩形框要素
            IRectangleElement pRectangleElement = new RectangleElementClass();
            //强转矩形要素框为要素
            IElement pElement = pRectangleElement as IElement;
            //赋值几何实体的最小外接矩形, 即包络线
            pElement.Geometry = pEnvelope;

            //使用面要素刷新(存在覆盖注释问题)
            DrawPolyline2(pGraphicsContainer, pActiveView, pElement);
        }

        private void AxMapControl1_OnMouseMove(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseMoveEvent e)
        {
            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel2.Text = e.mapX.ToString("#0.0000") + "  " + e.mapY.ToString("#0.0000");
        }

        /// <summary>
        /// 使用面要素刷新(存在覆盖注释问题)
        /// </summary>
        /// <param name="pGraphicsContainer"></param>
        /// <param name="pActiveView"></param>
        /// <param name="pElement"></param>
        private static void DrawPolyline2(IGraphicsContainer pGraphicsContainer, IActiveView pActiveView, IElement pElement)
        {
            //以下代码设置要素外框边线的颜色、透明度属性
            IRgbColor pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;
            pColor.Transparency = 255;

            //以下代码设置要素外框边线的颜色、宽度属性
            ILineSymbol pOutline = new SimpleLineSymbolClass();
            pOutline.Width = 2;
            pOutline.Color = pColor;
            pColor = new RgbColorClass();
            pColor.NullColor = true;

            //以下代码设置要素内部的填充颜色、边线符号属性
            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pFillSymbol.Color = pColor;
            pFillSymbol.Outline = pOutline;

            //实现线框的生成
            IFillShapeElement pFillShapeElement = pElement as IFillShapeElement;
            pFillShapeElement.Symbol = pFillSymbol;
            pGraphicsContainer.AddElement((IElement)pFillShapeElement, 0);

            //刷新鹰眼视图的填充要素（绘图框）
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, pFillShapeElement, null);
        }

        private void AxMapControl2_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            if (axMapControl2.LayerCount > 0)
            {
                //如果e.button==1, 则表示按下的是鼠标左键
                if (e.button == 1)
                {
                    axMapControl2.Refresh();
                    //捕捉鼠标单击时的地图坐标
                    IPoint pPoint = new PointClass();
                    pPoint.PutCoords(e.mapX, e.mapY);
                    //将地图的中心点移动到鼠标点击的点pPoint
                    axMapControl1.CenterAt(pPoint);
                    axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                }
                else if (e.button == 2)
                {//如果e.button==2, 则表示按下的是鼠标右键
                 //鹰眼地图的TrackRectangle()方法, 随着鼠标拖动得到一个矩形框
                    IEnvelope pEnvelope = axMapControl2.TrackRectangle();
                    axMapControl1.Extent = pEnvelope;//鼠标拖动生成的矩形框范围
                    axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                }
            }
        }

        private void AxMapControl2_OnMouseMove(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseMoveEvent e)
        {
            //如果e.button==1, 则表示按下的是鼠标左键
            if (e.button == 1)
            {
                axMapControl2.Refresh();
                //捕捉鼠标单击时的地图坐标
                IPoint pPoint = new PointClass();
                pPoint.PutCoords(e.mapX, e.mapY);
                //将地图的中心点移动到鼠标点击的点pPoint
                axMapControl1.CenterAt(pPoint);
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }


        private void 三维面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FeatureController.Create3dSurface("try.shp", @"H:\Temp");
        }

        private void 基岩三维建模ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 地质界线模拟ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 工作区ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetForm setForm = new SetForm();
            setForm.Show();
        }

        private void 解析ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FaultOrder faultOrder = new FaultOrder();
            faultOrder.map = axMapControl1.Map;
            faultOrder.Show();
        }


        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        private void 用户手册ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Views.Help help = new Views.Help();
            help.Show();
        }
    }
}
