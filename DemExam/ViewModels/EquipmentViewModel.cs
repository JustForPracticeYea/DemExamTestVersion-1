using DemExam.Models;
using DemExam.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DemExam.ViewModels
{
    public class EquipmentViewModel
    {
        public EquipmentViewModel(Equipment equipment)
        {
            Id = equipment.Id;
            InventoryNumber = equipment.InventoryNumber;
            Weight = equipment.Weight;
            TransferToCompanyBalanceDate = equipment.TransferToCompanyBalanceDate;
            Photo = equipment.Photo;
            StandartTimeLimit = equipment.StandartTimeLimit;
            NameEquipment = equipment.NameEquipment;
            Description = equipment.Description;
            IdAuditoriumNavigation = equipment.IdAuditoriumNavigation;

            GetStatus();
            GetUserRole();
            GetPhoto();
        }
        public int Id { get; set; }

        public string InventoryNumber { get; set; } = null!;

        public int Weight { get; set; }

        public DateTime TransferToCompanyBalanceDate { get; set; }

        public string Photo { get; set; } = null!;

        public int StandartTimeLimit { get; set; }

        public string NameEquipment { get; set; } = null!;

        public string Description { get; set; } = null!;

        public Auditorium IdAuditoriumNavigation { get; set; } = null!;

        public Visibility Visibility { get; set; }

        public Brush Background { get; set; }

        public string Text { get; set; }

        public string FullName { get; set; }

        private void GetStatus()
        {
            var lifeTime = TransferToCompanyBalanceDate.AddYears(StandartTimeLimit);
            if(lifeTime < DateTime.Now)
            {
                Background = (Brush)new BrushConverter().ConvertFromString("#E32636");
                Text = "На списание";
                return;
            }
            else if(lifeTime > DateTime.Now && lifeTime.Year == DateTime.Now.Year)
            {
                Background = (Brush)new BrushConverter().ConvertFromString("#FFA500");
                Text = "Срок службы истекает в этом году";
            }
            else
            {
                Text = "Срок службы до: " + lifeTime.Year + " года";
            }
        }

        private void GetUserRole()
        {
            if(CurrentSession.CurrentUser != null && CurrentSession.CurrentUser.IdPost == 6)
            {
                Visibility = Visibility.Visible;
            }
            else if(CurrentSession.CurrentUser != null && (CurrentSession.CurrentUser.IdPost == 4 || CurrentSession.CurrentUser.IdPost == 5))
            {
                if (IdAuditoriumNavigation.IdOffice == CurrentSession.CurrentUser.IdOffice)
                {
                    Visibility = Visibility.Visible;
                }
                else 
                { 
                    Visibility = Visibility.Collapsed; 
                }
            }
            else
            {
                Visibility = Visibility.Collapsed;
            }
        }

        private void GetPhoto()
        {
            if(!string.IsNullOrEmpty(Photo))
            {
                Photo = "/Res/" + Photo;
            }
            else
            {
                Photo = "/Res/stub.jpg";
            }
        }
    }
}
