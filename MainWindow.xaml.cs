using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cursovaya
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            //ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd";
            Thread.CurrentThread.CurrentCulture = ci;

            string query = "" +
                "SELECT name " +
                "FROM sqlite_master " +
                "WHERE type='table' AND name NOT LIKE 'sqlite_%'";

            DataTable Tables = db.GetDataTableByQuery(query);

            foreach(DataRow row in Tables.Rows)
            {
                string TableName = row["Name"].ToString();
                if (true)
                {
                    Button button = new Button();
                    button.Content = TableName;
                    MenuButtons.Children.Add(button);
                }
            }

        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            var window = this;

            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal; // Restore window
            }
            else
            {
                window.WindowState = WindowState.Maximized; // Maximize window
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Table_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            MainFrame.Navigate(new ФормаСписка(b.Content.ToString()));
        }
    }
}
