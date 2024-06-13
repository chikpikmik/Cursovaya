using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Xml.Linq;

namespace Cursovaya
{

    public partial class ФормаОтчета : Page
    {
        public ReportInfo Report;
        public ФормаОтчета(ReportInfo report)
        {
            InitializeComponent();
            Report = report;
            TextBlock_TableName.Text = report.Name;
            foreach (ParameterInfo Parameter in Report.Parameters.Values)
            {
                Label label = new Label();
                label.Content = Parameter.Name.Replace("_", "__") + ":";

                ParametersPanel.Children.Add(label);

                if (Parameter.Type == ColumnInfo.Types.DATE)
                {
                    DatePicker datePicker = new DatePicker();
                    datePicker.Name = Parameter.Name;
                    ParametersPanel.Children.Add(datePicker);
                }
                else
                {
                    TextBox textBox = new TextBox();
                    textBox.Name = Parameter.Name;
                    ParametersPanel.Children.Add(textBox);
                }
               
            }
        }
        private void Сформировать_Click(object sender, RoutedEventArgs e)
        {
            string Query = Report.Query;

            bool Отказ = false;
            bool Истина = true;

            foreach (Control CurrentBox in ParametersPanel.Children)
            {
                string CurrentValue;

                if (CurrentBox is TextBox textBox)
                    CurrentValue = textBox.Text;
                else if (CurrentBox is DatePicker dateBox)
                    CurrentValue = dateBox.SelectedDate == null ? string.Empty : ((DateTime)dateBox.SelectedDate).ToShortDateString();
                else
                    continue;

                ParameterInfo ParameterInfo = Report.Parameters[CurrentBox.Name];

                if (CurrentValue == string.Empty)
                {
                    Отказ = Истина;
                    MessageBox.Show($"Поле {ParameterInfo.Name} обязательно для заполнения");
                    continue;
                }
                else if ((!int.TryParse(CurrentValue, out int _)) && ParameterInfo.Type == ColumnInfo.Types.INTEGER)
                {
                    Отказ = Истина;
                    MessageBox.Show($"Поле {ParameterInfo.Name} должно быть целым числом");
                }
                else if ((!double.TryParse(CurrentValue, out double _)) && ParameterInfo.Type == ColumnInfo.Types.REAL)
                {
                    Отказ = Истина;
                    MessageBox.Show($"Поле {ParameterInfo.Name} должно быть рациональным числом");
                }

                Query = Query.Replace("{"+ParameterInfo.Name+"}", CurrentValue);
                
            }
            
            if (Отказ)
                return;

            try
            {
                //MessageBox.Show(Query);
                DataGrid.ItemsSource = db.GetDataTableByQuery(Query).DefaultView;
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();
            e.Column.Header = header.Replace("_", "__");
            if (e.PropertyType == typeof(DateTime))
            {
                (e.Column as DataGridTextColumn).Binding.StringFormat = "yyyy-MM-dd";
            }
        }

    }
}
