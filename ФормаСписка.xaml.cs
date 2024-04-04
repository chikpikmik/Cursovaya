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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cursovaya
{
    /// <summary>
    /// Логика взаимодействия для ФормаСписка.xaml
    /// </summary>
    public partial class ФормаСписка : Page
    {
        private string TableName;
        public ФормаСписка(string TableName)
        {
            InitializeComponent();
            this.TableName = TableName;
            TextBlock_TableName.Text = TableName;
            DataGrid.ItemsSource = db.GetDataTableByQuery($"SELECT * FROM {TableName}").DefaultView;
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DataGridRow selectedRow = sender as DataGridRow;

                object[] rowValues = ((System.Data.DataRowView)selectedRow.Item).Row.ItemArray;
                DataColumnCollection columns = ((System.Data.DataRowView)selectedRow.Item).Row.Table.Columns;

                Dictionary<string, string> Element = new Dictionary<string, string>();

                for (int i = 0; i < columns.Count; i++)
                {
                    Element.Add(columns[i].ColumnName, rowValues[i].ToString());
                }

                ФормаЭлемента ElementForm = new ФормаЭлемента(Element, TableName);
                ElementForm.Show();

            }
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            e.Column.Header = header.Replace("_", "__");
        }
    }
}
