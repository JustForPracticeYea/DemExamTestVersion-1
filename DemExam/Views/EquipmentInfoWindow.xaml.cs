using DemExam.Models;
using DemExam.Statics;
using DemExam.ViewModels;
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
    /// Логика взаимодействия для EquipmentInfoWindow.xaml
    /// </summary>
    public partial class EquipmentInfoWindow : Window
    {
        private EquipmentViewModel _equipmentViewModel;
        private WorkersMumzhaContext _context = new();
        private bool _isImageChanged;
        private string _selectedImagePath;
        private string _currentUserRole;
        public EquipmentInfoWindow(EquipmentViewModel equipment)
        {
            InitializeComponent();
            DataContext = equipment;
            _equipmentViewModel = equipment;
            _currentUserRole = CurrentSession.CurrentUser.IdPostNavigation.PostName;

            LoadOffices();

            if (!string.IsNullOrEmpty(equipment.Photo))
                PhotoImage.Source = LoadImage(equipment.Photo);
            SetControlsAccess();
        }
        private void SetControlsAccess()
        {
            // Разрешено только администратору бд и заведующим
            bool canEdit = _currentUserRole == "администратор бд" || _currentUserRole.Contains("заведующий");

            if (!canEdit)
            {
                NameTextBox.IsReadOnly = true;
                InventoryNumberTextBox.IsReadOnly = true;
                DescriptionTextBox.IsReadOnly = true;
                WeightTextBox.IsReadOnly = true;
                StandartTimeLimitTextBox.IsReadOnly = true;
                TransferDatePicker.IsEnabled = false;
                OfficeComboBox.IsEnabled = false;
                AuditoriumComboBox.IsEnabled = false;
                ChangePhotoButton.IsEnabled = false;

                SaveButton.Visibility = Visibility.Collapsed;
                ChangePhotoButton.Visibility = Visibility.Collapsed;

                this.Title = "Просмотр оборудования";
            }

            if (_currentUserRole.Contains("заведующий"))
            {
                OfficeComboBox.IsEnabled = false;

                var office = _context.Offices
                    .FirstOrDefault(o => o.IdDesignatedWorker == CurrentSession.CurrentUser.Id);
                OfficeComboBox.SelectedItem = office;
            }


            //Можно удалить запись, только администратору, только со скалда и с превышенным сроком использования
            if (_currentUserRole == "администратор бд")
            {
                bool isInWarehouse = _equipmentViewModel.IdPlaceNavigation?.IdOfficeNavigation?.ShortName == "Склад";

                int writeOffYear = _equipmentViewModel.TransferToCompanyBalanceDate.Year + _equipmentViewModel.StandartTimeLimit;
                bool isExpired = writeOffYear < DateTime.Now.Year;

                if (isInWarehouse && isExpired)
                {
                    DeleteButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void LoadOffices()
        {
            var offices = _context.Offices.ToList();
            OfficeComboBox.ItemsSource = offices;

            if (_currentUserRole.Contains("заведующий"))
            {
                var office = offices.FirstOrDefault(o => o.IdDesignatedWorker == CurrentSession.CurrentUser.Id);
                OfficeComboBox.SelectedItem = office;
            }
            else if (_equipmentViewModel?.IdPlaceNavigation?.IdOfficeNavigation != null)
            {
                var officeId = _equipmentViewModel.IdPlaceNavigation.IdOfficeNavigation.Id;
                OfficeComboBox.SelectedItem = offices.FirstOrDefault(o => o.Id == officeId);
            }
        }

        private void OfficeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OfficeComboBox.SelectedItem is Office selectedOffice)
            {
                var auditoriums = _context.Places
                    .Where(a => a.IdOffice == selectedOffice.Id &&
                               a.AuditoriumNumber != null && a.AuditoriumNumber != "")
                    .ToList();


                var noAuditorium = new Place
                {
                    Id = -1,
                    AuditoriumNumber = "Без аудитории",
                    IdOffice = selectedOffice.Id
                };
                auditoriums.Insert(0, noAuditorium);

                AuditoriumComboBox.ItemsSource = auditoriums;

                // При выборе без аудитории выбирается либо существующая пустая аудитория в бд, либо создаетс яновая
                if (_equipmentViewModel.IdPlaceNavigation != null)
                {
                    if (string.IsNullOrEmpty(_equipmentViewModel.IdPlaceNavigation.AuditoriumNumber))
                    {
                        AuditoriumComboBox.SelectedItem = noAuditorium;
                    }
                    else
                    {
                        AuditoriumComboBox.SelectedItem = auditoriums
                            .FirstOrDefault(a => a.Id == _equipmentViewModel.IdPlaceNavigation.Id);
                    }
                }
            }
            else
            {
                AuditoriumComboBox.ItemsSource = null;
            }
        }

        private void ChangePhotoButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

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
                        image.StreamSource = stream;
                        image.EndInit();
                    }
                    image.Freeze();

                    PhotoImage.Source = image;
                    _isImageChanged = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии изображения: {ex.Message}");
                }
            }
        }

        private void DeletePhoto()
        {
            if (!string.IsNullOrEmpty(_equipmentViewModel.Photo) &&
                _equipmentViewModel.Photo != "stub.jpg")
            {
                string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res", _equipmentViewModel.Photo);
                if (File.Exists(binPath))
                    File.Delete(binPath);

                string projectPath = GetProjectDirectory();
                string projectFilePath = Path.Combine(projectPath, "Res", _equipmentViewModel.Photo);
                if (File.Exists(projectFilePath))
                    File.Delete(projectFilePath);
            }
        }

        private string SavePhoto(string sourcePath)
        {
            string newFileName = $"photo_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(sourcePath)}";

            // Сохраняется в bin
            string binResPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res");
            Directory.CreateDirectory(binResPath);
            File.Copy(sourcePath, Path.Combine(binResPath, newFileName), true);

            // Сохраняется в корень проекта
            string projectResPath = Path.Combine(GetProjectDirectory(), "Res");
            Directory.CreateDirectory(projectResPath);
            File.Copy(sourcePath, Path.Combine(projectResPath, newFileName), true);

            return newFileName;
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

        private BitmapImage LoadImage(string fileName)
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res", fileName);

                if (!File.Exists(fullPath))
                    fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res", "stub.jpg");

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
            catch
            {
                return null;
            }
        }

        private void SaveEquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new WorkersMumzhaContext())
                {
                    var equipment = context.Equipment
                        .FirstOrDefault(eq => eq.Id == _equipmentViewModel.Id);

                    if (equipment != null)
                    {
                        // Проверка уникальности инвентарного номера
                        if (equipment.InventoryNumber != _equipmentViewModel.InventoryNumber)
                        {
                            bool exists = context.Equipment
                                .Any(eq => eq.InventoryNumber == _equipmentViewModel.InventoryNumber
                                        && eq.Id != _equipmentViewModel.Id);

                            if (exists)
                            {
                                MessageBox.Show("Оборудование с таким инвентарным номером уже существует",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }

                        equipment.NameEquipment = _equipmentViewModel.NameEquipment;
                        equipment.Description = _equipmentViewModel.Description;
                        equipment.Weight = _equipmentViewModel.Weight;
                        equipment.TransferToCompanyBalanceDate = _equipmentViewModel.TransferToCompanyBalanceDate;
                        equipment.InventoryNumber = _equipmentViewModel.InventoryNumber;
                        equipment.StandartTimeLimit = _equipmentViewModel.StandartTimeLimit;

                        if (_isImageChanged && !string.IsNullOrEmpty(_selectedImagePath))
                        {
                            DeletePhoto();

                            string newFileName = SavePhoto(_selectedImagePath);
                            equipment.Photo = newFileName;
                            _equipmentViewModel.Photo = newFileName;
                        }

                        // Обработка аудитории
                        if (AuditoriumComboBox.SelectedItem is Place selectedAuditorium)
                        {
                            if (selectedAuditorium.Id == -1)
                            {
                                var office = (Office)OfficeComboBox.SelectedItem;
                                var existingAuditorium = context.Places
                                    .FirstOrDefault(a => a.IdOffice == office.Id &&
                                                        (a.AuditoriumNumber == null || a.AuditoriumNumber == ""));

                                if (existingAuditorium != null)
                                {
                                    equipment.IdPlace = existingAuditorium.Id;
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
                                    equipment.IdPlace = newAuditorium.Id;
                                }
                            }
                            else
                            {
                                equipment.IdPlace = selectedAuditorium.Id;
                            }
                        }

                        context.SaveChanges();

                        MessageBox.Show("Информация успешно изменена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonClickDeleteEquipment(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить оборудование '{_equipmentViewModel.NameEquipment}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new WorkersMumzhaContext())
                    {
                        var equipment = context.Equipment
                            .FirstOrDefault(eq => eq.Id == _equipmentViewModel.Id);

                        if (equipment != null)
                        {
                            DeletePhoto();

                            context.Equipment.Remove(equipment);
                            context.SaveChanges();

                            MessageBox.Show("Оборудование успешно удалено", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            DialogResult = true;
                            Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
