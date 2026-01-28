using System;
using System.Collections.Generic;

namespace DemExam.Models;

public partial class Equipment
{
    public int Id { get; set; }

    public string InventoryNumber { get; set; } = null!;

    public int Weight { get; set; }

    public DateTime TransferToCompanyBalanceDate { get; set; }

    public string Photo { get; set; } = null!;

    public int StandartTimeLimit { get; set; }

    public string NameEquipment { get; set; } = null!;

    public int IdAuditorium { get; set; }

    public string Description { get; set; } = null!;

    public virtual Auditorium IdAuditoriumNavigation { get; set; } = null!;
}
