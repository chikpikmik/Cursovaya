using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Cursovaya
{
    /// <summary>
    /// Логика взаимодействия для ФормаВыбора.xaml
    /// </summary>
    public partial class ФормаВыбора : Window
    {
        private TableInfo TableInfo;
        private Dictionary<string, string> Element = new Dictionary<string, string>();
        private string StartPos;
        private BackToElementForm BackToElementForm;
        
        public ФормаВыбора(string tablename, string startpos, BackToElementForm backtoelementform)
        {
            InitializeComponent();

            TableInfo = db.GetTableInfo(tablename);
            StartPos = startpos;
            Создать_Button.IsEnabled = App.UserAccess[tablename] == AccessRights.Запись;

            BackToElementForm = backtoelementform;
            TextBlock_TableName.Text = TableInfo.TableName;
            DataGrid.ItemsSource = db.GetDataTableByQuery($"SELECT * FROM {tablename}").DefaultView;
        }


        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataGrid.Items.Count > 0)
            {
                var dataGrid = sender as DataGrid;
                var columns = dataGrid.Columns;

                // Если не будет найдено значения
                dataGrid.SelectedItem = dataGrid.Items[0];

                int ColumnPrimarykeyIndex=-1;

                // нужно так как в headers заменяется тоже
                string PrimaryKey2 = TableInfo.PrimaryKey.Replace("_", "__");

                for (int i = 0; i < columns.Count; i++)
                {
                    if (columns[i].Header.ToString() == PrimaryKey2)
                    {
                        ColumnPrimarykeyIndex = i;
                        break;
                    }
                }
                if (ColumnPrimarykeyIndex != -1) 
                {
                    foreach (var item in dataGrid.Items)
                    {
                        var rowArray = ((System.Data.DataRowView)item).Row.ItemArray;
                        if (rowArray[ColumnPrimarykeyIndex].ToString() == StartPos)
                        {
                            dataGrid.SelectedItem = item;
                            break;
                        }
                    }
                }

                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
                dataGrid.Focus();
            }
        }



        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Choose(object sender, RoutedEventArgs e)
        {
            object[] rowValues = ((System.Data.DataRowView)DataGrid.SelectedItem).Row.ItemArray;
            DataColumnCollection columns = ((System.Data.DataRowView)DataGrid.SelectedItem).Row.Table.Columns;

            for (int i = 0; i < columns.Count; i++)
            {
                if (columns[i].ColumnName == TableInfo.PrimaryKey)
                {
                    BackToElementForm(TableInfo.TableName, rowValues[i].ToString());
                    break;
                }
            }
            this.Close();
        }

        private void Создать_Click(object sender, RoutedEventArgs e)
        {
            UpdateTable del = new UpdateTable(this.UpdateTable);

            ФормаЭлемента ElementForm = new ФормаЭлемента(Element, TableInfo, del, true);
            ElementForm.Show();
        }
        private void UpdateTable(TableUpdateTypes updateType)
        {
            int OldSelectedIndex = DataGrid.SelectedIndex;
            Element.Clear();

            DataGrid.ItemsSource = db.GetDataTableByQuery($"SELECT * FROM {TableInfo.TableName}").DefaultView;
            DataGrid.SelectedItem = DataGrid.Items[OldSelectedIndex];
            DataGrid.ScrollIntoView(DataGrid.SelectedItem);
            DataGrid.Focus();

        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            Element.Add(header, "");
            e.Column.Header = header.Replace("_", "__");
            if (e.PropertyType == typeof(DateTime))
            {
                (e.Column as DataGridTextColumn).Binding.StringFormat = "yyyy-MM-dd";
            }
        }

    }
}
