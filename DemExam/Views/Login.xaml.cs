using DemExam.Helpers;
using DemExam.Models;
using DemExam.Statics;
using DemExam.Views;
using System.Windows;
using System.Windows.Input;

namespace DemExam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WorkersMumzhaContext _context = new WorkersMumzhaContext();
        private MessageHelper _message = new MessageHelper();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new EquipmentWindow().Show();
            Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginEnter.Text;
            string password = PasswordEnter.Password;

            var user = _context.Workers.Where(u => u.Login == login && u.Password == password).FirstOrDefault();
            if (user == null)
            {
                _message.ShowError("Введён не правильный логин или пароль");
            }
            else
            {
                CurrentSession.CurrentUser = user;
                new EquipmentWindow(user).Show();
                Close();
            }
        }
    }
}