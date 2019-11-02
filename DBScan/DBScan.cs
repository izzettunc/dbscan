using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Collections;

namespace DBScan
{
    class DBScan
    {
        private class DBScanPoint
        {
            public int[] properties;
            public int inputID;
            public int clusterID;
            public bool visited;
            public DBScanPoint(int[] input, int inputID)
            {
                properties = new int[input.Length];
                for (int i = 0; i < input.Length; i++)
                {
                    properties[i] = input[i];
                }
                this.inputID = inputID;
                clusterID = -1;
                visited = false;
            }
        }
        public struct PointsStatistic
        {
            public int[] properties;
            public int inputID;
            public int clusterID;
            public PointsStatistic(int[] input, int inputID, int clusterID)
            {
                properties = new int[input.Length];
                for (int i = 0; i < input.Length; i++)
                {
                    properties[i] = input[i];
                }
                this.inputID = inputID;
                this.clusterID = clusterID;
            }
        }
        private int[,] DataSet;
        private string[] DataLabels;
        private Chart DataChart;
        private DBScanPoint[] DataPoints;
        private int numberOfClusters = 0;
        private PointsStatistic[] stats;
        public void calculateStats()
        {
            PointsStatistic[] Stats = new PointsStatistic[DataPoints.Length];
            for (int i = 0; i < DataPoints.Length; i++)
            {
                Stats[i] = new PointsStatistic(DataPoints[i].properties, DataPoints[i].inputID, DataPoints[i].clusterID);
            }
            stats = Stats;
        }
        public int NumberOfClusters { get => numberOfClusters; }
        public PointsStatistic[] Stats { get => stats; }
        public static int[,] ReadData(string fileName)
        {
            ;
            string[] temp = File.ReadAllLines(fileName);
            string[] lines = temp.Skip(1).ToArray();
            string[] firstElement = lines[0].Split(',');
            int[,] data = new int[lines.Length, firstElement.Length];
            int i = 0, j = 0;
            foreach (string line in lines)
            {
                string[] elements = line.Split(',');
                foreach (string element in elements)
                {
                    data[i, j] = Convert.ToInt32(element);
                    j++;
                }
                i++;
                j = 0;
            }
            return data;
        }
        public static string[] ReadLabel(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);
            string labels = sr.ReadLine();
            sr.Close();
            return labels.Split(',');
        }
        private void CreateElements()
        {
            DataPoints = new DBScanPoint[DataSet.GetLength(0)];
            for (int i = 0; i < DataSet.GetLength(0); i++)
            {
                int[] input = new int[DataSet.GetLength(1)];
                for (int j = 0; j < DataSet.GetLength(1); j++)
                {
                    input[j] = DataSet[i, j];
                }
                DataPoints[i] = new DBScanPoint(input, i);
            }
        }
        public DBScan(int[,] data, string[] label, Chart ch)
        {
            DataSet = data;
            DataLabels = label;
            DataChart = ch;
            CreateElements();
        }
        private int FindMax(int index)
        {
            int max = Int32.MinValue;
            for (int i = 0; i < DataPoints.Length; i++)
            {
                if (DataPoints[i].properties[index] > max)
                {
                    max = DataPoints[i].properties[index];
                }
            }
            return max;
        }
        public void PrintChart(int indexX, int indexY)
        {
            DataChart.Series.Clear();
            for (int i = 0; i < numberOfClusters; i++)
            {
                DataChart.Series.Add("Küme " + (i + 1));
                DataChart.Series[i].ChartType = SeriesChartType.Point;
            }
            DataChart.Series.Add("Outlier");
            DataChart.Series[numberOfClusters].ChartType = SeriesChartType.Point;
            DataChart.Series[numberOfClusters].Color = Color.Black;
            foreach (DBScanPoint p in DataPoints)
            {
                if (p.clusterID != -1)
                    DataChart.Series[p.clusterID].Points.AddXY(p.properties[indexX], p.properties[indexY]);
                else
                    DataChart.Series[numberOfClusters].Points.AddXY(p.properties[indexX], p.properties[indexY]);
            }
            int max;
            if (FindMax(indexX) > FindMax(indexY)) max = FindMax(indexX);
            else max = FindMax(indexY);
            DataChart.ChartAreas[0].AxisX.Maximum = max + Convert.ToInt32(max / 100.0) * 5;
            DataChart.ChartAreas[0].AxisY.Maximum = max + Convert.ToInt32(max / 100.0) * 5;
        }
        public int GetDataPointIndex(int x, int y, int indexX, int indexY)
        {
            for (int i = 0; i < DataPoints.Length; i++)
            {
                if (DataPoints[i].properties[indexX] == x && DataPoints[i].properties[indexY] == y)
                {
                    return i;
                }
            }
            return -1;
        }
        public int GetDataPointCluster(int x, int y, int indexX, int indexY)
        {
            for (int i = 0; i < DataPoints.Length; i++)
            {
                if (DataPoints[i].properties[indexX] == x && DataPoints[i].properties[indexY] == y)
                {
                    return DataPoints[i].clusterID;
                }
            }
            return -2;
        }
        private double FindDistance(DBScanPoint x, DBScanPoint y)
        {
            double sumOfSquares = 0;
            for (int i = 0; i < x.properties.Length; i++)
            {
                sumOfSquares += (y.properties[i] - x.properties[i]) * (y.properties[i] - x.properties[i]);
            }
            return Math.Sqrt(sumOfSquares);
        }
        private void Scan(DBScanPoint point, DBScanPoint[] points, double epsilon, int minPoint)
        {
            if (!point.visited)//Eğerki nokta daha önce ziyaret edilmemişse(sonsuz döngüyü kırmak için)
            {
                List<DBScanPoint> withinRange = new List<DBScanPoint>();
                point.visited = true;
                for (int i = 0; i < points.Length; i++)
                {
                    if (FindDistance(point, points[i]) <= epsilon)//Eğerki sınırlar içerisindeyse listeye ekle
                    {
                        withinRange.Add(points[i]);

                    }
                }
                if (withinRange.Count >= minPoint)//eğerki yeterli eleman var ise kümele
                {
                    if (point.clusterID == -1)//eğerki seçilen nokta bir kümeye sahip değilse 
                    {
                        point.clusterID = numberOfClusters;//bu noktayı kümeye ata
                        numberOfClusters++;//bir sonraki eleman için yeni küme oluştur
                    }
                    foreach (DBScanPoint p in withinRange)//ardından sınırlar içindeki tüm elemanlar içinde
                    {
                        p.clusterID = point.clusterID;//kümeye ata
                        Scan(p, points, epsilon, minPoint);//ardından o elemanın çevresini tara
                    }
                }
            }
            /* Algoritma
             * 
             * Önce ilk küme oluşturulur ve ilk küme daha fazla büyüyemeyene kadar "recursive" bir şekilde büyümeye devam eder
             * ilk küme ile yapılabilecek tüm işlemler bitince scan in ilk başta çağrıldığı yer olan dbscan fonksiyonundaki for
             * dönemye devam eder daha önce ziyaret edilmemiş bir nokta bulur ve aynı şeyler tüm noktalar ziyaret edilene kadar tekrar edilir
             */
        }
        public void StartDBScan(int indexX, int indexY, double epsilon, int minPoint)
        {
            for (int i = 0; i < DataPoints.Length; i++)
            {
                if (!DataPoints[i].visited)//Daha önce ziyaret edilmemiş tüm noktaları gezer
                {
                    Scan(DataPoints[i], DataPoints, epsilon, minPoint);
                }
            }
        }
    }
}
