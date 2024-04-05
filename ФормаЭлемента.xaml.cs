using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
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
        public TableInfo TableInfo;
        private UpdateTable UpdateTableDelegate;
        public ФормаЭлемента(Dictionary<string, string> element, string tablename, UpdateTable updatetable)
        {
            InitializeComponent();
            Element = element;
            UpdateTableDelegate = updatetable;

            TableInfo = db.GetTableInfo(tablename);

            foreach (var ColumnName_Value in Element)
            {
                ColumnInfo ColumnInfo = TableInfo.ColumnName_ColumnInfo[ColumnName_Value.Key];

                Label label = new Label();
                label.Content = ColumnName_Value.Key.Replace("_","__") + ":";

                Реквизиты.Children.Add(label);
                Control Box;
                if (ColumnInfo.LinkedTableName != null)
                { 
                    ComboBox comboBox = new ComboBox();
                    comboBox.Text = ColumnName_Value.Value;
                    comboBox.Name = ColumnInfo.LinkedTableName;
                    comboBox.DropDownOpened += ComboBox_DropDownOpened;
                    Box = comboBox;
                }
                else if (ColumnInfo.Type == ColumnInfo.Types.DATE) 
                {
                    DateTime date;
                    DatePicker datePicker = new DatePicker();
                    if (DateTime.TryParseExact(
                        ColumnName_Value.Value, 
                        "yyyy-MM-dd H:mm:ss", 
                        CultureInfo.CurrentCulture, 
                        DateTimeStyles.None, 
                        out date))
                    {
                        datePicker.SelectedDate = date;
                    }
                    Box = datePicker;
                }
                else
                {
                    TextBox textBox = new TextBox();
                    textBox.Text = ColumnName_Value.Value;
                    Box = textBox;
                }
                Box.Focusable = !ColumnInfo.IsItPrimaryKey;
                ЗначенияРеквизитов.Children.Add(Box);
            }

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Write_Click(object sender, RoutedEventArgs e)
        {
            bool Отказ = false;
            bool Истина = true;

            string UpdateQuery = $"UPDATE {TableInfo.TableName} SET ";
            string Where = "WHERE ";

            for (int i=0; i < Element.Count; i++)
            {
                var CurrenElement = Element.ElementAt(i);
                ColumnInfo ColumnInfo = TableInfo.ColumnName_ColumnInfo[CurrenElement.Key];

                Control CurrentBox = (Control)ЗначенияРеквизитов.Children[i];
                string CurrentValue;

                if (CurrentBox is TextBox textBox)
                    CurrentValue = textBox.Text;
                else if (CurrentBox is ComboBox comboBox)
                    CurrentValue = comboBox.Text;
                else if (CurrentBox is DatePicker dateBox)
                    CurrentValue = dateBox.SelectedDate == null ? string.Empty : ((DateTime)dateBox.SelectedDate).ToShortDateString();
                else
                    continue;

                // Создание запроса
                bool NextIsPrimaryKey_AndThisIsEnd = ((i+1)+1 == Element.Count) && TableInfo.ColumnName_ColumnInfo.ElementAt(i+1).Value.IsItPrimaryKey;
                bool CurrentIsEnd = i + 1 == Element.Count;
                if (!ColumnInfo.IsItPrimaryKey)
                    UpdateQuery += $"{ColumnInfo.ColumnName} = '{CurrentValue}'{( CurrentIsEnd || NextIsPrimaryKey_AndThisIsEnd ? "" : ",")} ";

                string OldValue = CurrenElement.Value;
                if (ColumnInfo.Type == ColumnInfo.Types.DATE)
                {
                    DateTime OldDate;
                    if (!DateTime.TryParseExact(
                            OldValue,
                            "yyyy-MM-dd H:mm:ss",
                            CultureInfo.CurrentCulture,
                            DateTimeStyles.None,
                            out OldDate))
                    {
                        //Отказ = Истина;
                        MessageBox.Show("Что то не то со старой датой");
                        //continue;
                    }
                    else
                    {
                        OldValue = OldDate.ToShortDateString();
                    }
                }
                Where += $"{ColumnInfo.ColumnName} = '{OldValue}'{( CurrentIsEnd ? "" : " AND ")} ";


                if (CurrentValue.Equals(string.Empty) && ColumnInfo.NotNull)
                {
                    Отказ = Истина;
                    MessageBox.Show($"Поле {ColumnInfo.ColumnName} обязательно для заполнения");
                    continue;
                }
                else if ( (! int.TryParse(CurrentValue, out int _)) && ColumnInfo.Type == ColumnInfo.Types.INTEGER)
                {
                    Отказ = Истина;
                    MessageBox.Show($"Поле {ColumnInfo.ColumnName} должно быть целым числом");
                }
                else if ((! double.TryParse(CurrentValue, out double _)) && ColumnInfo.Type == ColumnInfo.Types.REAL)
                {
                    Отказ = Истина;
                    MessageBox.Show($"Поле {ColumnInfo.ColumnName} должно быть рациональным числом");
                }
                else if (ColumnInfo.LinkedTableName != null)
                {
                    string LinkedTablePrimaryKey = TableInfo.LinkedTableName_LinkedTablePrimaryKey[ColumnInfo.LinkedTableName];
                    string check_query = $"SELECT 1 FROM {ColumnInfo.LinkedTableName} WHERE {LinkedTablePrimaryKey} = '{CurrentValue}'";
                    if (db.GetDataTableByQuery(check_query).Rows.Count == 0)
                    {
                        Отказ = Истина;
                        MessageBox.Show($"Поле {ColumnInfo.ColumnName} должно ссылаться на элемент таблицы {ColumnInfo.LinkedTableName}");
                    }
                }

            }
            
            UpdateQuery += Where;

            if (Отказ)
                return;

            int rowschanged = db.ExecuteNonQuery(UpdateQuery);
            MessageBox.Show("rows changed " + rowschanged.ToString());

            UpdateTableDelegate();
            this.Focus();
        }
        private void WriteAndClose_Click(object sender, RoutedEventArgs e)
        {
            Write_Click(sender, e);
            Close_Click(sender, e);
        }

        private void Choosed(string Table, string LinkedTablePrimaryKeyValue)
        {
            ComboBox comboBox = FindComboBox(Table);
            comboBox.Text = LinkedTablePrimaryKeyValue;
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            
            comboBox.IsDropDownOpen = false;
            
            string LinkedTableName = comboBox.Name;
            string LinkedTablePrimaryKey = TableInfo.LinkedTableName_LinkedTablePrimaryKey[LinkedTableName];
            string startPos = comboBox.Text;

            BackToElementForm del = new BackToElementForm(this.Choosed);

            ФормаВыбора ChooseForm = new ФормаВыбора(LinkedTableName, LinkedTablePrimaryKey, startPos, del);
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
