using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObslugaZamowien.Models
{
    public class DishOrder
    {
        public int Id { get; set; }
        public Dish Dish { get; set; }
        public int DishId { get; set; }
        public Order Order { get; set; }
        public int OrderId { get; set; }
        public int Amount { get; set; }
        public decimal TotalPrice
        {
            get
            {
                return Dish.Price * Amount;
            }
        }
    }
}
