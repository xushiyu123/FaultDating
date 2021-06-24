using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using FaultStructureModeling.Controllers;
using FaultStructureModeling.Entities;
using System.Windows.Forms;

namespace FaultStructureModeling.Views
{
    public partial class FaultOrder : Form
    {
        public IMap map = null;
        private IFeatureClass faultFeatures = null;
        private IFeatureClass strata = null;
        private StrataTable strataTable = null;

        public FaultOrder()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (faultFeatures != null)
            {
                FaultDating faultDating = new FaultDating();
                List<Fault> faults = faultDating.FaultSequence(faultFeatures);
                FeatureController.ExportFaults(Parameters.Workspace + @"\", faultFeatures.AliasName + "_s", faults);
                MessageBox.Show("计算完成！");
            }
            else
            {
                MessageBox.Show("请完善数据！");
            }
        }

        private void FaultOrder_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < map.LayerCount; i++)
            {
                if (map.Layer[i] is IFeatureLayer)
                {
                    comboBox1.Items.Add(map.Layer[i].Name);
                    comboBox2.Items.Add(map.Layer[i].Name);
                    comboBox4.Items.Add(map.Layer[i].Name);
                }
                else { }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            comboBox1.Text = AuxiliaryTools.FileBrowser(Parameters.Workspace, "shapefile|*.shp");
            faultFeatures = FeatureController.Reader(comboBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comboBox2.Text = AuxiliaryTools.FileBrowser(Parameters.Workspace, "shapefile|*.shp");
            faultFeatures = FeatureController.Reader(comboBox2.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string csvPath = textBox1.Text = AuxiliaryTools.FileBrowser("", "csv文件|*.csv");
            strataTable = new StrataTable();
            strataTable.FromCSV(csvPath);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (faultFeatures != null && strata != null && strataTable != null)
            {
                FaultDating faultDating = new FaultDating();
                List<Fault> faults = faultDating.IndirectDating(faultFeatures, strata, strataTable);
                FeatureController.ExportFaults(Parameters.Workspace + @"\", faultFeatures.AliasName + "_t", faults);
                MessageBox.Show("计算完成！");
            }
            else
            {
                MessageBox.Show("请完善数据！");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            IFeatureLayer featureLayer = AuxiliaryTools.GetLayerByName(comboBox4.SelectedItem.ToString(), map) as IFeatureLayer;
            strata = featureLayer.FeatureClass;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            IFeatureLayer featureLayer = AuxiliaryTools.GetLayerByName(comboBox1.SelectedItem.ToString(), map) as IFeatureLayer;
            faultFeatures = featureLayer.FeatureClass;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            IFeatureLayer featureLayer = AuxiliaryTools.GetLayerByName(comboBox2.SelectedItem.ToString(), map) as IFeatureLayer;
            faultFeatures = featureLayer.FeatureClass;
        }
    }
}
