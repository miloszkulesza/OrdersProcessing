using ObslugaZamowien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObslugaZamowien.ViewModels
{
    public class NewOrderViewModel
    {
        public Table Table { get; set; }
        public int TableId { get; set; }
        public Order Order { get; set; }
        public List<Dish> Dishes { get; set; }
        public bool IsEditing { get; set; } = false;
    }
}
