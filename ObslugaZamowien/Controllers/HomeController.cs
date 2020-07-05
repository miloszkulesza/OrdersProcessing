using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ObslugaZamowien.Consts;
using ObslugaZamowien.Models;
using ObslugaZamowien.Services;
using ObslugaZamowien.ViewModels;

namespace ObslugaZamowien.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static List<Table> tables;
        private static List<Dish> dishes;
        private static List<Order> orders;
        private static List<DishOrder> dishOrders;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            SetTables();
            SetDishes();
            SetOrders();
            var viewModel = new TableSummaryViewModel()
            {
                Tables = tables
            };
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void SetTables()
        {
            tables = SessionService.GetJson<List<Table>>(HttpContext.Session, SessionKeys.TablesKey);
            if (tables == null)
            {
                tables = new List<Table>()
                {
                    new Table { Id = 1, Name = "Stolik 1" },
                    new Table { Id = 2, Name = "Stolik 2" },
                    new Table { Id = 3, Name = "Stolik 3" },
                    new Table { Id = 4, Name = "Stolik 4" },
                    new Table { Id = 5, Name = "Stolik 5" },
                    new Table { Id = 6, Name = "Stolik 6" },
                    new Table { Id = 7, Name = "Stolik 7" },
                    new Table { Id = 8, Name = "Stolik 8" },
                    new Table { Id = 9, Name = "Stolik 9" },
                    new Table { Id = 10, Name = "Stolik 10" },
                };
                SessionService.SetJson(HttpContext.Session, SessionKeys.TablesKey, tables);
            }
            tables = tables.OrderBy(x => x.Name.Length).ThenBy(x => x.Name).ToList();
        }

        private void SetDishes()
        {
            dishes = SessionService.GetJson<List<Dish>>(HttpContext.Session, SessionKeys.DishesKey);
            if (dishes == null)
            {
                dishes = new List<Dish>()
                {
                    new Dish { Id = 1, Name = "Chrupiące Skrzydełka", Price = 19.95M, Amount = 0 },
                    new Dish { Id = 2, Name = "Tatar z polędwicy Wołowej", Price = 31.95M, Amount = 0 },
                    new Dish { Id = 3, Name = "Panierowane kalmary", Price = 29.95M, Amount = 0 },
                    new Dish { Id = 4, Name = "Chrupiące krewetki", Price = 29.95M, Amount = 0 },
                    new Dish { Id = 5, Name = "Carpaccio z polędwicy wołowej", Price = 30.95M, Amount = 0 },
                    new Dish { Id = 6, Name = "Burger Italian", Price = 35.95M, Amount = 0 },
                    new Dish { Id = 7, Name = "Burger Classic", Price = 29.95M, Amount = 0 },
                    new Dish { Id = 8, Name = "Burger California", Price = 34.95M, Amount = 0 },
                    new Dish { Id = 9, Name = "Perła 500ml", Price = 9.95M, Amount = 0 },
                    new Dish { Id = 10, Name = "Zwierzyniec 500ml", Price = 10.95M, Amount = 0 },
                    new Dish { Id = 11, Name = "Perła Export 500ml", Price = 11.95M, Amount = 0 },
                    new Dish { Id = 12, Name = "Łomża Miodowa 500ml", Price = 10.95M, Amount = 0 }
                };
                SessionService.SetJson(HttpContext.Session, SessionKeys.DishesKey, dishes);
            }
            dishes = dishes.OrderBy(x => x.Name).ToList();
        }

        private void SetOrders()
        {
            orders = SessionService.GetJson<List<Order>>(HttpContext.Session, SessionKeys.OrdersKey);
            if (orders != null)
            {
                dishOrders = new List<DishOrder>();
                foreach (var order in orders)
                {
                    dishOrders.AddRange(order.Dishes.ToList());
                }
            }
            else
            {
                orders = new List<Order>();
                dishOrders = new List<DishOrder>();
            }
        }

        public IEnumerable<Order> GetTableOrders(int tableId)
        {
            if (tableId != 0)
            {
                if (orders.Any(x => x.TableId == tableId))
                {
                    var tableOrders = orders.Where(x => x.TableId == tableId).OrderByDescending(x => x.CreateDate).ToList();
                    return tableOrders;
                }
            }
            return new List<Order>();
        }

        public decimal GetDishValue(int dishId)
        {
            return dishes.FirstOrDefault(x => x.Id == dishId).Price;
        }

        public IActionResult CreateNewOrder(int tableId)
        {
            var viewModel = new NewOrderViewModel()
            {
                Table = tables.FirstOrDefault(x => x.Id == tableId),
                TableId = tableId,
                Dishes = dishes.ToList()
            };
            if (orders.Any())
                viewModel.Order = new Order { Id = orders.LastOrDefault().Id + 1 };
            else
                viewModel.Order = new Order { Id = 1 };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult CreateNewOrder(NewOrderViewModel viewModel)
        {
            if (viewModel.Dishes.Any(x => x.IsSelected))
            {
                var selectedDishes = viewModel.Dishes.Where(x => x.IsSelected).ToList();
                if (viewModel.IsEditing)
                {
                    var order = orders.FirstOrDefault(x => x.Id == viewModel.Order.Id);
                    var oldDishes = order.Dishes;
                    order.Dishes = new List<DishOrder>();
                    foreach (var dish in viewModel.Dishes)
                    {
                        DishOrder newDishOrder = new DishOrder();
                        if (dish.IsSelected && oldDishes.Any(x => x.Id == dish.Id && x.OrderId == viewModel.Order.Id) && oldDishes.FirstOrDefault(x => x.Id == dish.Id && x.OrderId == viewModel.Order.Id).Dish.IsSelected)
                        {
                            if (dish.Amount == null || dish.Amount <= 0)
                            {
                                ModelState.AddModelError(string.Empty, "Podaj poprawne ilości wybranych dań");
                                return View(viewModel);
                            }
                            dishOrders.Remove(dishOrders.FirstOrDefault(x => x.Id == dish.Id && x.OrderId == viewModel.Order.Id));
                            newDishOrder.OrderId = viewModel.Order.Id;
                            if (dishOrders.Count == 0)
                                newDishOrder.Id = 1;
                            else
                                newDishOrder.Id = dishOrders.LastOrDefault().Id + 1;
                            newDishOrder.Dish = dish;
                            newDishOrder.DishId = dish.Id;
                            newDishOrder.Amount = dish.Amount.Value;
                            dishOrders.Add(newDishOrder);
                            order.Dishes.Add(newDishOrder);
                        }
                        else if (dish.IsSelected)
                        {
                            if (dish.Amount == null || dish.Amount <= 0)
                            {
                                ModelState.AddModelError(string.Empty, "Podaj poprawne ilości wybranych dań");
                                return View(viewModel);
                            }
                            newDishOrder.OrderId = viewModel.Order.Id;
                            newDishOrder.Id = dishOrders.LastOrDefault().Id + 1;
                            newDishOrder.DishId = dish.Id;
                            newDishOrder.Dish = dish;
                            newDishOrder.Amount = dish.Amount.Value;
                            dishOrders.Add(newDishOrder);
                            order.Dishes.Add(newDishOrder);
                        }
                    }
                    SessionService.SetJson(HttpContext.Session, SessionKeys.OrdersKey, orders);
                    TempData["SuccessMessage"] = "Pomyślnie edytowano zamówienie";
                    return RedirectToAction("Index");
                }
                else
                {
                    var order = new Order()
                    {
                        TableId = viewModel.TableId
                    };
                    if (orders.Any())
                        order.Id = orders.LastOrDefault().Id + 1;
                    else
                        order.Id = 1;
                    order.Dishes = new List<DishOrder>();
                    foreach (var dish in selectedDishes)
                    {
                        if (dishOrders.Any(x => x.Dish.Name == dish.Name && x.OrderId == order.Id))
                        {
                            if (!dish.IsSelected)
                                dishOrders.Remove(dishOrders.FirstOrDefault(x => x.Dish.Name == dish.Name && x.OrderId == order.Id));
                            else
                            {
                                var existingDishOrder = dishOrders.FirstOrDefault(x => x.Dish.Name == dish.Name && x.OrderId == order.Id);
                                existingDishOrder.Amount = dish.Amount.Value;
                            }
                        }
                        else
                        {
                            if (dish.Amount == null || dish.Amount <= 0)
                            {
                                ModelState.AddModelError(string.Empty, "Podaj poprawne ilości wybranych dań");
                                return View(viewModel);
                            }
                            var dishOrder = new DishOrder();
                            if (dishOrders.Any())
                                dishOrder.Id = dishOrders.LastOrDefault().Id + 1;
                            else
                                dishOrder.Id = 1;
                            dishOrder.Dish = dish;
                            dishOrder.DishId = dish.Id;
                            dishOrder.Amount = dish.Amount.Value;
                            dishOrder.OrderId = order.Id;
                            dishOrders.Add(dishOrder);
                            order.Dishes.Add(dishOrder);
                        }
                    }
                    orders.Add(order);
                    SessionService.SetJson(HttpContext.Session, SessionKeys.OrdersKey, orders);
                    TempData["SuccessMessage"] = "Pomyślnie utworzono zamówienie";
                    return RedirectToAction("Index");
                }
            }
            ModelState.AddModelError(string.Empty, "Nie wybrano dań do zapisania");
            return View(viewModel);
        }

        public IActionResult EditOrder(int orderId)
        {
            var order = orders.FirstOrDefault(x => x.Id == orderId);
            var viewModel = new NewOrderViewModel
            {
                Dishes = dishes,
                TableId = order.TableId,
                Table = tables.FirstOrDefault(x => x.Id == order.TableId),
                Order = order,
                IsEditing = true
            };
            foreach (var dish in order.Dishes)
            {
                var selectedDish = viewModel.Dishes.FirstOrDefault(x => x.Id == dish.DishId);
                selectedDish.IsSelected = true;
                selectedDish.Amount = dish.Amount;
            }
            return View("CreateNewOrder", viewModel);
        }

        public decimal GetOrderValue(int? orderId)
        {
            if (orderId != null && orderId != 0)
                return Math.Round(orders.FirstOrDefault(x => x.Id == orderId).DishesValue, 2);
            return 0M;
        }

        public IActionResult DeleteOrder(int orderId)
        {
            orders.Remove(orders.FirstOrDefault(x => x.Id == orderId));
            SessionService.SetJson(HttpContext.Session, SessionKeys.OrdersKey, orders);
            TempData["SuccessMessage"] = "Pomyślnie usunięto zamówienie";
            return RedirectToAction("Index");
        }

        public IActionResult Payout(int orderId)
        {
            var viewModel = new PayoutViewModel()
            {
                OrderId = orderId,
                Order = orders.FirstOrDefault(x => x.Id == orderId)
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Payout(PayoutViewModel viewModel)
        {
            var order = orders.FirstOrDefault(x => x.Id == viewModel.OrderId);
            if (viewModel.Tip)
                order.Tip = true;
            else
                order.Tip = false;
            order.IsPaid = true;
            SessionService.SetJson(HttpContext.Session, SessionKeys.OrdersKey, orders);
            TempData["SuccessMessage"] = "Pomyślnie opłacono zamówienie";
            return RedirectToAction("Index");
        }

        public IActionResult CreateOrderForManyTables()
        {
            var viewModel = new CreateOrderForManyTablesViewModel()
            {
                Tables = tables,
                Dishes = dishes
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult CreateOrderForManyTables(CreateOrderForManyTablesViewModel viewModel)
        {
            if (!viewModel.Tables.Any(x => x.IsSelected))
            {
                ModelState.AddModelError(string.Empty, "Wybierz conajmniej 1 stolik");
                return View(viewModel);
            }
            if (!viewModel.Dishes.Any(x => x.IsSelected))
            {
                ModelState.AddModelError(string.Empty, "Wybierz co najmniej jedno danie");
                return View(viewModel);
            }
            var selectedTables = viewModel.Tables.Where(x => x.IsSelected).ToList();
            var selectedDishes = viewModel.Dishes.Where(x => x.IsSelected).ToList();
            foreach (var table in selectedTables)
            {
                var order = new Order();
                if (orders.Any())
                    order.Id = orders.LastOrDefault().Id + 1;
                else
                    order.Id = 1;
                order.TableId = table.Id;
                order.Dishes = new List<DishOrder>();
                foreach (var dish in selectedDishes)
                {
                    var dishOrder = new DishOrder();
                    if (!dishOrders.Any(x => x.OrderId == order.Id && x.DishId == dish.Id))
                    {
                        if (dishOrders.Any())
                            dishOrder.Id = dishOrders.LastOrDefault().Id + 1;
                        else
                            dishOrder.Id = 1;
                    }
                    else
                    {
                        dishOrders.Remove(dishOrders.FirstOrDefault(x => x.OrderId == order.Id && x.DishId == dish.Id));
                        if (dishOrders.Any())
                            dishOrder.Id = dishOrders.LastOrDefault().Id + 1;
                        else
                            dishOrder.Id = 1;
                    }
                    if (dish.Amount == null || dish.Amount <= 0)
                    {
                        ModelState.AddModelError(string.Empty, "Podaj prawidłowe ilości dla wszystkich wybranych dań");
                        return View(viewModel);
                    }
                    dishOrder.DishId = dish.Id;
                    dishOrder.Amount = dish.Amount.Value;
                    dishOrder.OrderId = order.Id;
                    dishOrder.Dish = dish;
                    dishOrders.Add(dishOrder);
                    order.Dishes.Add(dishOrder);
                }
                orders.Add(order);
            }
            SessionService.SetJson(HttpContext.Session, SessionKeys.OrdersKey, orders);
            TempData["SuccessMessage"] = "Pomyślnie utworzono zamówienia";
            return RedirectToAction("Index");
        }

        public IActionResult CreateDish()
        {
            return View(new Dish());
        }

        [HttpPost]
        public IActionResult CreateDish(Dish newDish)
        {
            if (!ModelState.IsValid)
                return View(newDish);
            if (dishes.Any(x => x.Name.ToLower().Equals(newDish.Name.ToLower())) && newDish.Id == 0)
            {
                ModelState.AddModelError(string.Empty, "Istnieje danie o takiej nazwie");
                return View(newDish);
            }
            if (newDish.Id == 0)
            {
                newDish.Id = dishes.LastOrDefault().Id + 1;
                newDish.Amount = 0;
                dishes.Add(newDish);
                TempData["SuccessMessage"] = "Udało się dodać nowe danie";
            }
            else
            {
                for(int i = 0; i < dishes.Count; i++)
                {
                    if (dishes[i].Id == newDish.Id)
                        dishes[i] = newDish;
                }
                TempData["SuccessMessage"] = "Udało się zaktualizować danie";
            }
            SessionService.SetJson(HttpContext.Session, SessionKeys.DishesKey, dishes);
            return RedirectToAction("Index");
        }

        public IActionResult CreateTable()
        {
            return View(new Table());
        }

        [HttpPost]
        public IActionResult CreateTable(Table newTable)
        {
            if (!ModelState.IsValid)
                return View(newTable);
            if(tables.Any(x => x.Name.ToLower().Equals(newTable.Name.ToLower())) && newTable.Id == 0)
            {
                ModelState.AddModelError(string.Empty, "Istnieje stolik o takiej nazwie");
                return View(newTable);
            }
            if (newTable.Id == 0)
            {
                newTable.Id = tables.LastOrDefault().Id + 1;
                tables.Add(newTable);
                TempData["SuccessMessage"] = "Udało się utworzyć nowy stolik";
            }
            else
            {
                for(int i = 0; i < tables.Count; i++)
                {
                    if (tables[i].Id == newTable.Id)
                        tables[i] = newTable;
                }
                TempData["SuccessMessage"] = "Udało się zaktualizować stolik";
            }
            SessionService.SetJson(HttpContext.Session, SessionKeys.TablesKey, tables);
            return RedirectToAction("Index");
        }

        public IActionResult ListTables()
        {
            return View(tables);
        }

        public IActionResult ListDishes()
        {
            return View(dishes);
        }

        public IActionResult EditTable(int tableId)
        {
            return View("CreateTable", tables.FirstOrDefault(x => x.Id == tableId));
        }

        public IActionResult DeleteTable(int tableId)
        {
            tables.Remove(tables.FirstOrDefault(x => x.Id == tableId));
            SessionService.SetJson(HttpContext.Session, SessionKeys.TablesKey, tables);
            TempData["SuccessMessage"] = "Pomyślnie usunięto stolik";
            return RedirectToAction("Index");
        }

        public IActionResult EditDish(int dishId)
        {
            return View("CreateDish", dishes.FirstOrDefault(x => x.Id == dishId));
        }

        public IActionResult DeleteDish(int dishId)
        {
            dishes.Remove(dishes.FirstOrDefault(x => x.Id == dishId));
            SessionService.SetJson(HttpContext.Session, SessionKeys.DishesKey, dishes);
            TempData["SuccessMessage"] = "Pomyślnie usunięto danie";
            return RedirectToAction("Index");
        }
    }
}
