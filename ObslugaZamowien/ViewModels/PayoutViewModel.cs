using ObslugaZamowien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObslugaZamowien.ViewModels
{
    public class PayoutViewModel
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public bool Tip { get; set; }
    }
}
