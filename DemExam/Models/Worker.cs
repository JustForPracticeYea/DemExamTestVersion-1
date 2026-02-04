using System;
using System.Collections.Generic;

namespace DemExam.Models;

public partial class Worker
{
    public int Id { get; set; }

    public string Surname { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Patronymic { get; set; }

    public int BirthYear { get; set; }

    public int IdPost { get; set; }

    public int IdOffice { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual Office IdOfficeNavigation { get; set; } = null!;

    public virtual Post IdPostNavigation { get; set; } = null!;

    public virtual ICollection<Office> Offices { get; set; } = new List<Office>();
}
