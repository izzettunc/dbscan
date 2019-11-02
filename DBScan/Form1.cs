using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace DBScan
{
    public partial class Form1 : Form
    {
        DBScan dbscan1;
        int[,] data;
        string[] labels;
        public Form1()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == comboBox1.SelectedIndex)
            {
                MessageBox.Show("İki özellikte aynı olamaz", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 1;
            }

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == comboBox1.SelectedIndex)
            {
                MessageBox.Show("İki özellikte aynı olamaz", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 1;
            }
        }

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            HitTestResult result = chart1.HitTest(e.X, e.Y);
            DataPoint clickedPoint;
            int index;
            int cluster;
            if (result.Object is DataPoint)
            {
                clickedPoint = result.Series.Points[result.PointIndex];
                index = dbscan1.GetDataPointIndex(Convert.ToInt32(clickedPoint.XValue), Convert.ToInt32(clickedPoint.YValues[0]), comboBox1.SelectedIndex, comboBox2.SelectedIndex);
                cluster = dbscan1.GetDataPointCluster(Convert.ToInt32(clickedPoint.XValue), Convert.ToInt32(clickedPoint.YValues[0]), comboBox1.SelectedIndex, comboBox2.SelectedIndex);
                if (index >= 0)
                {
                    if(cluster!=-1)
                        MessageBox.Show("( " + clickedPoint.XValue + " , " + clickedPoint.YValues[0] + " )\nSeçilen değer " + (index + 1) + ". kayıttır ve "+(cluster+1)+".kümeye aittir .");
                    else
                        MessageBox.Show("( " + clickedPoint.XValue + " , " + clickedPoint.YValues[0] + " )\nSeçilen değer " + (index + 1) + ". kayıttır ve herhangi bir kümeye atanamamıştır .");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(Double.TryParse(textBox1.Text,out double result))
            {
                if(Int32.TryParse(textBox2.Text,out int result2))
                {
                    dbscan1 = new DBScan(data, labels, chart1);
                    dbscan1.StartDBScan(comboBox1.SelectedIndex, comboBox2.SelectedIndex, Convert.ToDouble(textBox1.Text), Convert.ToInt32(textBox2.Text));
                    button2.Enabled = true;
                    button4.Enabled = true;
                    dbscan1.PrintChart(comboBox1.SelectedIndex, comboBox2.SelectedIndex);
                }
                else if (textBox2.Text == "")
                {
                    MessageBox.Show("Lutfen minimum nokta sayısını boş bırakmayınız", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Lutfen minimum nokta sayısını tam sayi olarak giriniz", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if(textBox1.Text=="")
            {
                MessageBox.Show("Lutfen epsilon değerini boş bırakmayınız", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Lutfen epsilon değerini (x,xx...) yada (x.xx..) olarak giriniz", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dbscan1.PrintChart(comboBox1.SelectedIndex, comboBox2.SelectedIndex);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();
            if (fileDialog.FileName != "")
            {
                data = DBScan.ReadData(fileDialog.FileName);
                labels = DBScan.ReadLabel(fileDialog.FileName);
                comboBox1.Items.Clear();
                comboBox2.Items.Clear();
                foreach (string s in labels)
                {
                    comboBox1.Items.Add(s);
                    comboBox2.Items.Add(s);
                }
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 1;
                button1.Enabled = true;
                button2.Enabled = false;
                label5.Text = "Dosya : " + fileDialog.SafeFileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            dbscan1.calculateStats();
            DBScan.PointsStatistic[] stats = dbscan1.Stats;
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Metin dosyaları(*.txt)| *.txt | Tüm dosyalar(*.*) | *.* ";
            fileDialog.ShowDialog();
            if(fileDialog.FileName!="")
            {
                StreamWriter writer = new StreamWriter(fileDialog.FileName);
                int[] counter = new int[dbscan1.NumberOfClusters+1];
                for(int i=0;i<stats.Length;i++)
                {
                    if(stats[i].clusterID!=-1)
                    {
                        writer.WriteLine("Kayıt " + (i + 1) + ":\tKüme " + (stats[i].clusterID + 1));
                        counter[stats[i].clusterID]++;
                    }
                    else
                    {
                        writer.WriteLine("Kayıt " + (i + 1) + ":\t?");
                        counter[dbscan1.NumberOfClusters]++;
                    }
                }
                for (int i = 0; i < counter.Length; i++)
                {
                    if (i<dbscan1.NumberOfClusters)
                    {
                        writer.WriteLine("Küme " + (i + 1)+" : \t"+counter[i]+" kayıt");
                    }
                    else
                    {
                        writer.WriteLine("Kümeye atanmayan (sapan değer) : \t" + counter[i] + " kayıt");
                    }
                    
                }
                writer.Close();
                MessageBox.Show("Dosyaya başarıyla yazdırıldı", "İşlem Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            

        }
    }
}
