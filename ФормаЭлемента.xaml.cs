using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Cursovaya
{
    /// <summary>
    /// Логика взаимодействия для ФормаЭлемента.xaml
    /// </summary>
    public delegate void BackToElementForm(string Table, string PrimaryKeyValue);
    public partial class ФормаЭлемента : Window
    {
        public Dictionary<string, string> Element;
        public string TableName;
        public Dictionary<string, string> Table_PrimaryKey = new Dictionary<string, string>();
        public ФормаЭлемента(Dictionary<string, string> Element, string TableName)
        {
            InitializeComponent();
            this.Element = Element;
            this.TableName = TableName;

            
            string query = "" +
                "SELECT " +
                    "table_info.name AS [Column]," +
                    "table_info.type AS Type," +
                    "table_info.[notnull] AS NotNullVal," +
                    "foreign_key_list.[table] AS Linked_Table," +
                    "foreign_key_list.[to] AS Linked_Table_Primary_Key " +
                "FROM " +
                    $"pragma_table_info('{TableName}') AS table_info " +
                "LEFT JOIN " +
                    $"pragma_foreign_key_list('{TableName}') AS foreign_key_list " +
                "ON foreign_key_list.[from] = table_info.name";

            DataTable ColumnInfo = db.GetDataTableByQuery(query);

            foreach (var pair in Element)
            {
                DataRow row = ColumnInfo.Select($"Column = '{pair.Key}'")[0];

                var Column = row.ItemArray[0];
                var Type = row.ItemArray[1];
                var NotNullVal = row.ItemArray[2];
                var LinkedTable = row.ItemArray[3];
                var LinkedTablePrimaryKey = row.ItemArray[4];

                Label label = new Label();
                label.Content = pair.Key.Replace("_","__") + ":";
                Реквизиты.Children.Add(label);

                if (LinkedTable == DBNull.Value)
                {
                    TextBox textBox = new TextBox();
                    textBox.Text = pair.Value;
                    ЗначенияРеквизитов.Children.Add(textBox);
                }
                else
                {
                    ComboBox comboBox = new ComboBox();
                    comboBox.Text = pair.Value;
                    comboBox.Name = LinkedTable.ToString();
                    Table_PrimaryKey.Add(LinkedTable.ToString(), LinkedTablePrimaryKey.ToString());
                    comboBox.DropDownOpened += ComboBox_DropDownOpened;
                    ЗначенияРеквизитов.Children.Add(comboBox);
                }

                
            }

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Write_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("writed");
            // тут проверяются типы и заполнение
            // тут создается UPDATE запрос
            // и тут вызвается делегат который в ФормеСписка обновляет таблицу
        }
        private void WriteAndClose_Click(object sender, RoutedEventArgs e)
        {
            Write_Click(sender, e);
            Close_Click(sender, e);
        }

        private void Choosed(string Table, string PrimaryKeyValue)
        {
            ComboBox comboBox = FindComboBox(Table);
            comboBox.Text = PrimaryKeyValue;
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            comboBox.IsDropDownOpen = false;
            string Table = comboBox.Name;
            string PrimaryKey = Table_PrimaryKey[Table];
            string startPos = comboBox.Text;

            BackToElementForm del = new BackToElementForm(this.Choosed);

            ФормаВыбора ChooseForm = new ФормаВыбора(Table, PrimaryKey, startPos, del);
            ChooseForm.Show();
        }

        public ComboBox FindComboBox(string name)
        {
            foreach (Control control in ЗначенияРеквизитов.Children)
            {
                if (control is ComboBox comboBox && control.Name == name)
                {
                    return comboBox;
                }
            }

            return null;
        }

    }
}
