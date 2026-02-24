using DemExam.Models;
using DemExam.Statics;
using DemExam.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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
        private List<EquipmentViewModel> _equipmentViewModels = new();
        public EquipmentWindow()
        {
            InitializeComponent();
            LoadEquipment();
            LoadComboBoxes();
        }
        public EquipmentWindow(Worker worker)
        {
            InitializeComponent();
            LoadEquipment();
            LoadComboBoxes();
        }

        private void LoadEquipment()
        {
            var equipment = new List<Equipment>();
            if (CurrentSession.CurrentUser == null)
            {
                equipment = _context.Equipment.Include(w => w.IdPlaceNavigation).
                    ThenInclude(w => w.IdOfficeNavigation)
                    .Where(w => w.IdPlaceNavigation.IdOfficeNavigation.ShortName == "Общее подразделение")
                    .ToList();
                ControlPanel.Visibility = Visibility.Collapsed;
                EquipmentList.IsEnabled = false;
            }
            if (CurrentSession.CurrentUser != null)
            {
                if (CurrentSession.CurrentUser.IdPostNavigation.PostName == "Лаборант")
                {
                    equipment = _context.Equipment
                        .Where(w => CurrentSession.CurrentUser.IdOffice == w.IdPlaceNavigation.IdOffice)
                        .ToList();
                    ControlPanel.Visibility = Visibility.Collapsed;
                }
                if (CurrentSession.CurrentUser.IdPostNavigation.PostName == "Техник" || CurrentSession.CurrentUser.IdPostNavigation.PostName.Contains("заведующий"))
                {
                    equipment = _context.Equipment.Include(w => w.IdPlaceNavigation)
                        .ThenInclude(w => w.IdOfficeNavigation)
                        .Where(w => CurrentSession.CurrentUser.IdOffice == w.IdPlaceNavigation.IdOffice).ToList();
                    FilterComboBox.Visibility = Visibility.Collapsed;
                    SortComboBox.Visibility = Visibility.Collapsed;
                    SearchTextBox.Visibility = Visibility.Collapsed;

                    if (CurrentSession.CurrentUser.IdPostNavigation.PostName == "Техник")
                        ControlPanel.Visibility = Visibility.Collapsed;
                }
                if (CurrentSession.CurrentUser.IdPostNavigation.PostName == "Инженер" || CurrentSession.CurrentUser.IdPostNavigation.PostName == "администратор бд")
                {
                    equipment = _context.Equipment.Include(w => w.IdPlaceNavigation)
                        .ThenInclude(w => w.IdOfficeNavigation)
                        .ToList();

                    if (CurrentSession.CurrentUser.IdPostNavigation.PostName == "инженер")
                        ButtonCreateEquipment.Visibility = Visibility.Collapsed;
                }
            }
            foreach(var item in equipment)
            {
                var viewModel = new EquipmentViewModel(item);
                viewModel.ImageSource = LoadImageFromBytes(item.Photo);
                _equipmentViewModels.Add(viewModel);
            }
            EquipmentList.ItemsSource = _equipmentViewModels;
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

        private void LoadComboBoxes()
        {
            var offices = _context.Offices.ToList();

            FilterComboBox.Items.Add("Все подразделения");
            foreach (var item in offices)
            {
                FilterComboBox.Items.Add(item.ShortName);
            }

            FilterComboBox.SelectedIndex = 0;


            SortComboBox.Items.Add("По возрастанию веса");
            SortComboBox.Items.Add("По убыванию веса");
            SortComboBox.SelectedIndex = 0;
        }

        private void EquipmentListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var selectedEquipment = EquipmentList.SelectedItem as EquipmentViewModel;
            if (selectedEquipment != null)
            {
                var editWindow = new EquipmentInfoWindow(selectedEquipment);

                // Получение актуальных данныъх
                if (editWindow.ShowDialog() == true)
                {
                    _context = new WorkersMumzhaContext();
                    _equipmentViewModels.Clear();
                    LoadEquipment();
                    UpdateEquipmentList();

                }
            }
        }

        private void CreateEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            var createEquipmentWindow = new CreateEquipmentWindow();
            if (createEquipmentWindow.ShowDialog() == true)
            {
                _context = new WorkersMumzhaContext();
                _equipmentViewModels.Clear();
                LoadEquipment();
                UpdateEquipmentList();
            }
        }

        private List<EquipmentViewModel> Searching(List<EquipmentViewModel> source)
        {
            var searchText = SearchTextBox.Text.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(searchText))
                return source;

            var keyWords = searchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            List<EquipmentViewModel> filtered = new();

            foreach (var equipment in _equipmentViewModels)
            {
                bool allWordsFound = true;

                foreach (var key in keyWords)
                {
                    bool wordFound = (equipment.NameEquipment?.ToLower().Contains(key) ?? false) ||
                                    (equipment.InventoryNumber?.ToLower().Contains(key) ?? false) ||
                                    (equipment.Description?.ToLower().Contains(key) ?? false);

                    if (!wordFound)
                    {
                        allWordsFound = false;
                        break;
                    }
                }

                if (allWordsFound)
                {
                    filtered.Add(equipment);
                }
            }

            return filtered;
        }

        private List<EquipmentViewModel> Filtering(List<EquipmentViewModel> source)
        {
            if (FilterComboBox.SelectedItem?.ToString() == "Все подразделения")
                return source;

            return source
                .Where(e => e.IdPlaceNavigation.IdOfficeNavigation?.ShortName == FilterComboBox.SelectedItem?.ToString())
                .ToList();
        }
        private List<EquipmentViewModel> Sorting(List<EquipmentViewModel> source)
        {
            if (SortComboBox.SelectedItem == null)
                return source;

            var selectedSort = SortComboBox.SelectedItem.ToString();

            if (selectedSort == "Возрастание веса")
                return source
                    .OrderBy(e => e.Weight)
                    .ToList();

            else if (selectedSort == "Убывание веса")
                return source
                    .OrderByDescending(e => e.Weight)
                    .ToList();
            else
                return source;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEquipmentList();
        }

        private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateEquipmentList();
        }

        private void OnFilterOfficeChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateEquipmentList();
        }

        private void UpdateEquipmentList()
        {
            List<EquipmentViewModel> result = _equipmentViewModels;

            result = Searching(result);
            result = Filtering(result);

            result = Sorting(result);

            EquipmentList.ItemsSource = result.ToList();
        }

        private BitmapImage LoadImageFromBytes(string fileName)
        {
            string fullPath;
            if (string.IsNullOrEmpty(fileName))
            {
                fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res", "stub.jpg");
            }
            else
            {
                fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res", fileName);
            }

            if (!File.Exists(fullPath))
            {
                fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res", "stub.jpg");
            }

            var image = new BitmapImage();
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}
