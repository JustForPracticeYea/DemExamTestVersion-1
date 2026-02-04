using System;
using System.Collections.Generic;

namespace DemExam.Models;

public partial class Place
{
    public int Id { get; set; }

    public string? AuditoriumNumber { get; set; }

    public int? IdOffice { get; set; }

    public string Floor { get; set; } = null!;

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual Office? IdOfficeNavigation { get; set; }
}
