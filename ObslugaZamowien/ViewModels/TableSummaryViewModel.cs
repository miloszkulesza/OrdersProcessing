using ObslugaZamowien.Models;
using System.Collections.Generic;

namespace ObslugaZamowien.ViewModels
{
    public class TableSummaryViewModel
    {
        public IEnumerable<Table> Tables { get; set; }
        public int SelectedTableId { get; set; }
    }
}
