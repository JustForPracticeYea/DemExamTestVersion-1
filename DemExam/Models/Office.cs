using System;
using System.Collections.Generic;

namespace DemExam.Models;

public partial class Office
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string ShortName { get; set; } = null!;

    public int? IdDesignatedWorker { get; set; }

    public virtual Worker? IdDesignatedWorkerNavigation { get; set; }

    public virtual ICollection<Place> Places { get; set; } = new List<Place>();

    public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
}
