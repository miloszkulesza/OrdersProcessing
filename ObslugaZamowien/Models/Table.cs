using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ObslugaZamowien.Models
{
    public class Table
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Podaj nazwę stolika")]
        public string Name { get; set; }
        public List<Order> Orders { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
