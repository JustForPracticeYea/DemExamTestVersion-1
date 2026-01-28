using System;
using System.Collections.Generic;

namespace DemExam.Models;

public partial class Auditorium
{
    public int Id { get; set; }

    public string AuditoriumName { get; set; } = null!;

    public int? IdOffice { get; set; }

    public string Floor { get; set; } = null!;

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual Office? IdOfficeNavigation { get; set; }
}
