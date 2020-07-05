using ObslugaZamowien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObslugaZamowien.ViewModels
{
    public class CreateOrderForManyTablesViewModel
    {
        public List<Table> Tables { get; set; }
        public List<Dish> Dishes { get; set; }
    }
}
