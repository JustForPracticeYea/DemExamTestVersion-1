using System;
using System.Collections.Generic;

namespace DemExam.Models;

public partial class Post
{
    public int Id { get; set; }

    public string PostName { get; set; } = null!;

    public int SalaryAmount { get; set; }

    public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
}
