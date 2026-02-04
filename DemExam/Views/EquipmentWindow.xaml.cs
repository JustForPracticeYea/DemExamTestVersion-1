using DemExam.Models;
using DemExam.Statics;
using DemExam.ViewModels;
using Microsoft.EntityFrameworkCore;
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

namespace DemExam.Views
{
    /// <summary>
    /// Логика взаимодействия для EquipmentWindow.xaml
    /// </summary>
    public partial class EquipmentWindow : Window
    {
        private WorkersMumzhaContext _context = new WorkersMumzhaContext();
        public EquipmentWindow()
        {
            InitializeComponent();
            LoadEquipment();
        }
        public EquipmentWindow(Worker worker)
        {
            InitializeComponent();
            LoadEquipment();
        }

        private void LoadEquipment()
        {
            List<Equipment> equipment = _context.Equipment.Include(w => w.IdPlaceNavigation).
                ThenInclude(w => w.IdOfficeNavigation)
                .Where(w => w.IdPlaceNavigation.IdOfficeNavigation.ShortName == "Общее подразделение")
                .ToList();
            if (CurrentSession.CurrentUser != null)
            {
                if (CurrentSession.CurrentUser.IdPostNavigation.PostName == "Лаборант")
                {
                    equipment = _context.Equipment
                        .Where(w => CurrentSession.CurrentUser.IdOffice == w.IdPlaceNavigation.IdOffice)
                        .ToList();
                }
                if (CurrentSession.CurrentUser.IdPostNavigation.PostName == "Техник" || CurrentSession.CurrentUser.IdPostNavigation.PostName.Contains("заведующий"))
                {
                    equipment = _context.Equipment.Include(w => w.IdPlaceNavigation)
                        .ThenInclude(w => w.IdOfficeNavigation)
                        .Where(w => CurrentSession.CurrentUser.IdOffice == w.IdPlaceNavigation.IdOffice).ToList();
                }
                if (CurrentSession.CurrentUser.IdPostNavigation.PostName == "Инженер" || CurrentSession.CurrentUser.IdPostNavigation.PostName == "администратор бд")
                {
                    equipment = _context.Equipment.Include(w => w.IdPlaceNavigation)
                        .ThenInclude(w => w.IdOfficeNavigation)
                        .ToList();
                }
            }
            List<EquipmentViewModel> equipmentViewModels = new List<EquipmentViewModel>();
            foreach(var item in equipment)
            {
                equipmentViewModels.Add(new EquipmentViewModel(item));
            }
            EquipmentList.ItemsSource = equipmentViewModels;
            FullName.Text = GetFullName();
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentSession.CurrentUser = null;
            new MainWindow().Show();
            Close();
        }

        private string GetFullName()
        {
            if (CurrentSession.CurrentUser != null)
            {
                FullName.Text =  CurrentSession.CurrentUser.Surname + " " + CurrentSession.CurrentUser.Name + " " + CurrentSession.CurrentUser.Patronymic;
            }
            else
            {
                FullName.Text = "Гость";
            }
            return FullName.Text;
        }
    }
}
