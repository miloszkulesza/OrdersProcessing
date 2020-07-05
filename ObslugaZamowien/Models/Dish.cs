using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ObslugaZamowien.Models
{
    public class Dish
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Podaj nazwę potrawy")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Podaj cenę potrawy")]
        [Range(0.00, 999.99, ErrorMessage = "Cena potrawy powinna mieścić się w zakresie 0 - 999,99 PLN")]
        public decimal Price { get; set; }
        public bool IsSelected { get; set; }
        public int? Amount { get; set; }
    }
}
