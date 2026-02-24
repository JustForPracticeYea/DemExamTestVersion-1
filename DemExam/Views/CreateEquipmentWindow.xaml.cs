using DemExam.Models;
using DemExam.Statics;
using Microsoft.Win32;
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

namespace DemExam.Views
{
    /// <summary>
    /// Логика взаимодействия для CreateEquipmentWindow.xaml
    /// </summary>
    public partial class CreateEquipmentWindow : Window
    {
        private bool _isImageChanged;
        private string _selectedImagePath;
        private string _currentUserRole;

        public Equipment NewEquipment { get; private set; }
        public CreateEquipmentWindow()
        {
            InitializeComponent();

            _currentUserRole = CurrentSession.CurrentUser?.IdPostNavigation?.PostName;

            using (var context = new WorkersMumzhaContext())
            {
                // Загрузка подразделений
                var offices = context.Offices.ToList();
                OfficeComboBox.ItemsSource = offices;
                OfficeComboBox.DisplayMemberPath = "ShortName";

                if (_currentUserRole == "заведующий лабораторией" || _currentUserRole == "заведующий складом")
                {
                    //заведущий может выбрать только свое подразделение
                    var userOffice = offices.FirstOrDefault(o => o.IdDesignatedWorker == CurrentSession.CurrentUser.Id);
                    OfficeComboBox.SelectedItem = userOffice;
                    OfficeComboBox.IsEnabled = false;
                }
            }

            TransferDatePicker.SelectedDate = DateTime.Today;
        }
        private void OfficeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (OfficeComboBox.SelectedItem is Office selectedOffice)
            {
                using (var context = new WorkersMumzhaContext())
                {
                    var auditoriums = context.Places
                        .Where(a => a.IdOffice == selectedOffice.Id &&
                                   a.AuditoriumNumber != null &&
                                   a.AuditoriumNumber != "")
                        .ToList();

                    var noAuditorium = new Place
                    {
                        Id = -1,
                        AuditoriumNumber = "Без аудитории",
                        IdOffice = selectedOffice.Id
                    };

                    auditoriums.Insert(0, noAuditorium);

                    AuditoriumComboBox.ItemsSource = auditoriums;
                    AuditoriumComboBox.DisplayMemberPath = "AuditoriumNumber";
                }
            }
        }

        private void BrowsePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
            ofd.Title = "Выберите изображение";

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    _selectedImagePath = ofd.FileName;

                    var image = new BitmapImage();
                    using (var stream = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.DecodePixelWidth = 300;
                        image.DecodePixelHeight = 200;
                        image.StreamSource = stream;
                        image.EndInit();
                    }
                    image.Freeze();

                    PhotoImage.Source = image;
                    _isImageChanged = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии изображения: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearErrors();

                bool isValid = true;

                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    NameError.Text = "Введите название";
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(InventoryNumberTextBox.Text))
                {
                    InventoryError.Text = "Введите инвентарный номер";
                    isValid = false;
                }

                using (var checkContext = new WorkersMumzhaContext())
                {
                    bool exists = checkContext.Equipment
                        .Any(e => e.InventoryNumber == InventoryNumberTextBox.Text);

                    if (exists)
                    {
                        InventoryError.Text = "Такой инвентарный номер уже существует";
                        isValid = false;
                    }
                }

                if (!double.TryParse(WeightTextBox.Text, out double weight) || weight <= 0)
                {
                    WeightError.Text = "Введите корректный вес";
                    isValid = false;
                }

                if (!int.TryParse(StandartTimeLimitTextBox.Text, out int years) || years <= 0)
                {
                    TimeLimitError.Text = "Введите корректный срок";
                    isValid = false;
                }

                if (OfficeComboBox.SelectedItem == null)
                {
                    OfficeError.Text = "Выберите подразделение";
                    isValid = false;
                }

                if (AuditoriumComboBox.SelectedItem == null)
                {
                    AuditoriumError.Text = "Выберите аудиторию";
                    isValid = false;
                }

                if (!isValid)
                {
                    MessageBox.Show("Заполните все поля правильно", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var context = new WorkersMumzhaContext())
                {
                    var newEquipment = new Equipment
                    {
                        NameEquipment = NameTextBox.Text,
                        Description = DescriptionTextBox.Text,
                        Weight = double.Parse(WeightTextBox.Text),
                        TransferToCompanyBalanceDate = TransferDatePicker.SelectedDate ?? DateTime.Today,
                        InventoryNumber = InventoryNumberTextBox.Text,
                        StandartTimeLimit = int.Parse(StandartTimeLimitTextBox.Text),
                    };

                    if (_isImageChanged && !string.IsNullOrEmpty(_selectedImagePath))
                    {
                        string originalFileName = Path.GetFileName(_selectedImagePath);
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
                        string extension = Path.GetExtension(originalFileName);
                        string newFileName = $"{fileNameWithoutExt}_{DateTime.Now:yyyyMMddHHmmss}{extension}";

                        string projectResPath = Path.Combine(GetProjectDirectory(), "Res");
                        if (!Directory.Exists(projectResPath))
                            Directory.CreateDirectory(projectResPath);

                        // Сохранение в корень проекта и в выходную папку
                        string projectDestPath = Path.Combine(projectResPath, newFileName);
                        File.Copy(_selectedImagePath, projectDestPath, true);

                        string binResPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res");
                        if (!Directory.Exists(binResPath))
                            Directory.CreateDirectory(binResPath);

                        string binDestPath = Path.Combine(binResPath, newFileName);
                        File.Copy(_selectedImagePath, binDestPath, true);

                        newEquipment.Photo = newFileName;
                    }


                    if (AuditoriumComboBox.SelectedItem is Place selectedAuditorium)
                    {
                        // Выбор поразделения без аудитории
                        if (selectedAuditorium.Id == -1)
                        {
                            var office = (Office)OfficeComboBox.SelectedItem;
                            var existingAuditorium = context.Places
                                .FirstOrDefault(a => a.IdOffice == office.Id &&
                                                     (a.AuditoriumNumber == null || a.AuditoriumNumber == ""));

                            if (existingAuditorium != null)
                            {
                                newEquipment.IdPlace = existingAuditorium.Id;
                            }
                            else
                            {
                                var newAuditorium = new Place
                                {
                                    AuditoriumNumber = null,
                                    IdOffice = office.Id,
                                    Floor = "0"
                                };

                                context.Places.Add(newAuditorium);
                                context.SaveChanges();

                                newEquipment.IdPlace = newAuditorium.Id;
                            }
                        }
                        else
                        {
                            newEquipment.IdPlace = selectedAuditorium.Id;
                        }
                    }


                    context.Equipment.Add(newEquipment);
                    context.SaveChanges();

                    NewEquipment = newEquipment;
                }

                MessageBox.Show("Оборудование успешно добавлено");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}");
            }
        }

        private void ClearErrors()
        {
            NameError.Text = "";
            InventoryError.Text = "";
            WeightError.Text = "";
            TimeLimitError.Text = "";
            OfficeError.Text = "";
            AuditoriumError.Text = "";
        }

        private string GetProjectDirectory()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var directoryInfo = new DirectoryInfo(baseDirectory);

            while (directoryInfo != null && !directoryInfo.Name.Equals("bin", StringComparison.OrdinalIgnoreCase))
            {
                directoryInfo = directoryInfo.Parent;
            }

            return directoryInfo?.Parent?.FullName ?? baseDirectory;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
