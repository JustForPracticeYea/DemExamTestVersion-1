using DemExam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemExam.Statics
{
    public static class CurrentSession
    {
        public static Worker CurrentUser { get;set; }
    }
}
