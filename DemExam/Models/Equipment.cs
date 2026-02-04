using System;
using System.Collections.Generic;

namespace DemExam.Models;

public partial class Equipment
{
    public int Id { get; set; }

    public string InventoryNumber { get; set; } = null!;

    public double Weight { get; set; }

    public DateTime TransferToCompanyBalanceDate { get; set; }

    public string? Photo { get; set; }

    public int StandartTimeLimit { get; set; }

    public string NameEquipment { get; set; } = null!;

    public int IdPlace { get; set; }

    public string? Description { get; set; }

    public virtual Place IdPlaceNavigation { get; set; } = null!;
}
