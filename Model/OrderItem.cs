﻿
using ConsoleSalesApp.Model;

public class OrderItem
{
    
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int Stock {get; set;}
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal TotalPrice => UnitPrice * Quantity;
    

}