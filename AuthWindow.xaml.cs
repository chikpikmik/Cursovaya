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
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Cursovaya
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    /// 
    
    public partial class AuthWindow : Window
    {
        public List<User> Users { get; set; }
        public AuthWindow()
        {
            InitializeComponent();
            this.Users = db.GetUsersList();
            DataContext = this;
            UserBox.SelectedIndex = 0;
        }

        private void Auth_Click(object sender, RoutedEventArgs e)
        {
            int UserId = UserBox.SelectedIndex;
            string Password = PassBox.Password.Trim();

            if (UserId == -1)
                MessageBox.Show("Выберите пользователя");

            else if (Users[UserId].Password == Password)
            {
                App.CurrentUser = Users[UserId];
                App.UserAccess = db.GetAccessRights(UserId);
                MainWindow MainWindow = new MainWindow();
                MainWindow.Show();
                this.Close();
            }

            else
                MessageBox.Show("Идентификация пользователя не выполнена");

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
