using DemExam.Models;
using DemExam.Statics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            IdPlaceNavigation = equipment.IdPlaceNavigation;

            SetStatus();
            SetAuditoriumVisibility();
            SetEquipmentStatusVisibilityToUser();
        }

        public int Id { get; set; }

        public string InventoryNumber { get; set; } = null!;
        public double Weight { get; set; }

        public DateTime TransferToCompanyBalanceDate { get; set; }

        public string Photo { get; set; }

        public string PhotoPath
        {
            get
            {
                if (!string.IsNullOrEmpty(Photo))
                    return $"/Res/{Photo}";

                return $"/Res/stub.jpg";
            }
        }

        public int StandartTimeLimit { get; set; }

        public string NameEquipment { get; set; } = null!;

        public string Description { get; set; } = null!;

        public Place IdPlaceNavigation { get; set; } = null!;

        public Visibility Visibility { get; set; }
        public Visibility AuditoriumVisibility { get; set; }

        public Brush Background { get; set; }

        public string Text { get; set; }

        public ImageSource ImageSource { get; set; }

        private void SetStatus()
        {
            var lifeTime = TransferToCompanyBalanceDate.AddYears(StandartTimeLimit);
            if(lifeTime.Year < DateTime.Now.Year)
            {
                if (IdPlaceNavigation.IdOfficeNavigation.ShortName == "Склад")
                {
                    Text = "Списано";
                }
                else
                {
                    Background = (Brush)new BrushConverter().ConvertFromString("#E32636");
                    Text = "На списание";
                }
                return;
            }
            else if(lifeTime.Year == DateTime.Now.Year)
            {
                Background = (Brush)new BrushConverter().ConvertFromString("#FFA500");
                Text = "Срок службы истекает в этом году";
            }
            else
            {
                Text = "Срок службы до: " + lifeTime.Year + " года";
            }
        }

        private void SetAuditoriumVisibility()
        {
            if (IdPlaceNavigation == null)
            {
                AuditoriumVisibility = Visibility.Collapsed;
                return;
            }
            if (string.IsNullOrEmpty(IdPlaceNavigation.AuditoriumNumber))
            {
                AuditoriumVisibility = Visibility.Collapsed;
            }
            else
            {
                AuditoriumVisibility = Visibility.Visible;
            }
        }

        private void SetEquipmentStatusVisibilityToUser()
        {
            if (CurrentSession.CurrentUser != null && CurrentSession.CurrentUser.IdPostNavigation.PostName == "администратор бд")
            {
                Visibility = Visibility.Visible;
            }
            else if (CurrentSession.CurrentUser != null && (CurrentSession.CurrentUser.IdPostNavigation.PostName == "заведующий лабораторией" || CurrentSession.CurrentUser.IdPostNavigation.PostName == "заведующий складом"))
            {
                if (IdPlaceNavigation.IdOffice == CurrentSession.CurrentUser.IdOffice)
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
    }
}
