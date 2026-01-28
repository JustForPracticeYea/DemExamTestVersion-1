using System;
using System.Collections.Generic;

namespace DemExam.Models;

public partial class Office
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string ShortName { get; set; } = null!;

    public int? IdWorker { get; set; }

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    public virtual Worker? IdWorkerNavigation { get; set; }

    public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
}
