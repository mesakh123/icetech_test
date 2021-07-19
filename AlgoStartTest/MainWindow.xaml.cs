using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace AlgoStartTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> mainData = new List<string>();
        int splitCount = 100;
        int currentAnchor = 0;
        int totalAnchor = 0;
        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            InitializeDataGrid();

           
            timer.Tick += new EventHandler(UpdateDataGrid);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void UpdateDataGrid(object sender, EventArgs e)
        {

            DataTable  dt = ((DataView)dataGrid.ItemsSource).ToTable();
            dt.PrimaryKey = new DataColumn[] { dt.Columns["S0"] };
            string currentData;
            int sizeAnchor = Math.Min(currentAnchor + splitCount, currentAnchor + mainData.Count - totalAnchor);
  
            if (currentAnchor >= 0)
            {
                for (int i = currentAnchor; i < sizeAnchor; i++)
                {
                    currentData = mainData.ElementAt(i);

                    DataRow row = dt.NewRow();
                    foreach (string data in currentData.Split(';'))
                    {

                        if (data != "")
                        {
                            row[data.Split('=')[0].Replace(":","")] = data.Split('=')[1];
                        }
                    }
                    
                    var existing = dt.Rows.Find(new Object[] { row["S0"] });
                    if (existing==null)
                    {
                        dt.Rows.Add(row);
                    }
                    else
                    {
                        existing.ItemArray = row.ItemArray;
                    }

                }
                currentAnchor = sizeAnchor;
                totalAnchor += currentAnchor;
            }
            else
            {
                timer.Stop();
            }
            dataGrid.DataContext = dt.DefaultView;

        }

        private string clean_Vstring(String v)
        {
            string[] string_list = v.Split(';');
            string result =String.Empty;
            string temp;
            int index;
            for(int i = 0; i < string_list.Length-1; i++)
            {
                index = Convert.ToInt32(string_list[i].Split('=')[0].Split(':')[1]);
                temp = string_list[i].Split('=')[1];
                if (i == 2)
                {
                    temp = temp.Substring(0,temp.Length-4)+":"+temp.Substring(temp.Length-4,2)+":"+temp.Substring(temp.Length - 2, 2);
                }
                else if(float.Parse(temp)> 10000000000)
                {
                    float temp_float = float.Parse(temp) / 10000000000;
                    temp = String.Format("{0:0.00}", temp_float);
                }
                result += "V:" + index.ToString() + "=" + temp+";";
            }

            return result;

        }


        private void InitializeDataGrid()
        {
            //Read file

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = @"../../Resources/TickData.txt";


            StreamReader sr = File.OpenText(resourceName);

            string line, data_v = String.Empty, data_s = String.Empty;
            while( (line = sr.ReadLine())!=null)
            {
                data_v = line.Split(new string[] { "S:0=" }, StringSplitOptions.None)[0];
                data_s = "S:0=" + line.Split(new string[] { "S:0=" }, StringSplitOptions.None)[1];
                data_s = data_s.Remove(data_s.Length - 1, 1);
                data_v = clean_Vstring(data_v);
                mainData.Add(data_s +  data_v);
            }


            var table = new DataTable();
            var keys = new DataColumn[1];
            DataColumn column;
            if (data_s != String.Empty)
            {

                string[] header_s = data_s.Split(';');
                
                for (int i = 0; i < header_s.Length - 1; i++)
                {

                    string temp = "S" + header_s[i].Split(':')[1].Split('=')[0];
                    column = new DataColumn();
                    column.DataType = Type.GetType("System.String");
                    column.ColumnName = temp;
                    table.Columns.Add(column);
                   

                }
            }

            if (data_v != String.Empty)
            {

                string[] header_v = data_v.Split(';');
                for (int i = 0; i < header_v.Length - 1; i++)
                {
                    string temp = "V" + header_v[i].Split(':')[1].Split('=')[0];
                    column = new DataColumn();
                    column.DataType = System.Type.GetType("System.String");
                    column.ColumnName = temp;
                    table.Columns.Add(column);
                }

            }
            table.PrimaryKey = new DataColumn[] { table.Columns["S0"]};

            dataGrid.DataContext = table.DefaultView;


        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".txt";
            ofd.Filter = "Text Document (.txt)|*.txt";
            if (ofd.ShowDialog() == true)
            {
                string fileName = ofd.FileName;
            }
        }


        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
