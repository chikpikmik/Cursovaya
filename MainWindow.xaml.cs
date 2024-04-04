using System;
using System.Collections.Generic;
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

            foreach (UIElement element in MenuButtons.Children)
            {
                if (element is Button)
                {
                    Button button = element as Button;

                    if (button.Content.ToString() != "123")
                    {
                        button.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        button.Visibility = Visibility.Collapsed;
                    }
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
