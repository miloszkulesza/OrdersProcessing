using System;
using System.Collections.Generic;

namespace ObslugaZamowien.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public List<DishOrder> Dishes { get; set; }

        public decimal DishesValue
        {
            get
            {
                decimal value = 0;
                foreach (var dish in Dishes)
                {
                    value += dish.TotalPrice;
                }
                return value;
            }
        }

        public bool Tip { get; set; }
        public decimal TipValue
        {
            get
            {
                return DishesValue * 0.05M;
            }
        }

        public decimal TotalValueWithTip
        {
            get
            {
                return DishesValue + TipValue;
            }
        }

        public bool IsPaid { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
