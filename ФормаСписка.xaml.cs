using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO.Packaging;
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
    public delegate void UpdateTable(TableUpdateTypes updateType);
    public enum TableUpdateTypes { INSERT, UPDATE, DELETE };

    public partial class ФормаСписка : Page
    {
        private TableInfo TableInfo;

        private Dictionary<string, string> Element = new Dictionary<string, string>();
        public ФормаСписка(string tablename)
        {
            InitializeComponent();
            TextBlock_TableName.Text = tablename;
            TableInfo = db.GetTableInfo(tablename);

            if( App.UserAccess[tablename] != AccessRights.Запись || TableInfo.Type == TableInfo.Types.view )
            {
                Создать_Button.IsEnabled = false;

                // Это контекстное меню
                DataGrid.RowStyle.Setters.RemoveAt(1);
                // Это событие при двойном щелчке по строке
                DataGrid.RowStyle.Setters.RemoveAt(0);
            }

            string query = $"SELECT * FROM {tablename}";
            if (tablename == "График" && App.CurrentUser.ДоступныеОтделы != null)
            {
                query = $"SELECT Дата, Работник_id, КоличествоРабочихЧасов FROM {tablename} JOIN Работники w ON Работник_id = w.id WHERE w.Отдел_id IN ({string.Join(", ", App.CurrentUser.ДоступныеОтделы)})";
            }
            else if (tablename == "Работники" && App.CurrentUser.ДоступныеОтделы != null)
            {
                query = $"SELECT * FROM {tablename} WHERE Отдел_id IN ({string.Join(", ", App.CurrentUser.ДоступныеОтделы)})";
            }

            DataGrid.ItemsSource = db.GetDataTableByQuery(query).DefaultView;
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
                    Element[columns[i].ColumnName] = rowValues[i].ToString();
                }

                UpdateTable del = new UpdateTable(this.UpdateTable);

                ФормаЭлемента ElementForm = new ФормаЭлемента(Element, TableInfo, del, false);
                ElementForm.ShowDialog();

            }
        }
        private void Создать_Click(object sender, RoutedEventArgs e)
        {
            UpdateTable del = new UpdateTable(this.UpdateTable);

            ФормаЭлемента ElementForm = new ФормаЭлемента(Element, TableInfo, del, true);
            ElementForm.ShowDialog();
        }

        private void Удалить_Click(object sender, RoutedEventArgs e)
        {
            var selectedRows = DataGrid.SelectedItems;

            string query = $"DELETE FROM {TableInfo.TableName} WHERE ";

            for (int i = 0; i < selectedRows.Count; i++)
            {
                object[] RowValues = ((System.Data.DataRowView)selectedRows[i]).Row.ItemArray;

                for (int j = 0; j < Element.Count; j++)
                {
                    // Добавить проверку на дату
                    string Column = Element.ElementAt(j).Key;
                    ColumnInfo ColumnInfo = TableInfo.ColumnName_ColumnInfo[Column];

                    string Value = RowValues[j].ToString();

                    if (ColumnInfo.Type == ColumnInfo.Types.DATE)
                    {
                        DateTime Date;
                        if (DateTime.TryParseExact(
                                Value,
                                "yyyy-MM-dd H:mm:ss",
                                CultureInfo.CurrentCulture,
                                DateTimeStyles.None,
                                out Date) ||
                            DateTime.TryParseExact(
                                Value,
                                "yyyy-MM-dd",
                                CultureInfo.CurrentCulture,
                                DateTimeStyles.None,
                                out Date))
                            Value = Date.ToShortDateString();
                        else
                        {
                            MessageBox.Show("Что то не то c удаляемой датой");
                        }
                    }

                    query += $"{Column} = '{Value}' {(j + 1 == Element.Count ? "" : "AND")} ";

                }

                query += (i + 1 == selectedRows.Count ? "" : "OR ");
            }

            try
            {
                int rowschanged = db.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            UpdateTable(TableUpdateTypes.DELETE);
        }

        private void UpdateTable(TableUpdateTypes updateType)
        {
            int OldSelectedIndex = DataGrid.SelectedIndex;
            Element.Clear();

            DataGrid.ItemsSource = db.GetDataTableByQuery($"SELECT * FROM {TableInfo.TableName}").DefaultView;
            int newCount = DataGrid.Items.Count;

            if (updateType == TableUpdateTypes.UPDATE)
                DataGrid.SelectedItem = DataGrid.Items[OldSelectedIndex];
                
            else if (updateType == TableUpdateTypes.INSERT)
                DataGrid.SelectedItem = DataGrid.Items[newCount-1];

            else if (updateType == TableUpdateTypes.DELETE)
                DataGrid.SelectedItem = DataGrid.Items[newCount - 1];

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
