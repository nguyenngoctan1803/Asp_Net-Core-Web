using DoAn2VADT.Database.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace DoAn2VADT.ViewModel
{
    public class OrderItem
    {
        public int Quantity { set; get; }
        public string ProductId { set; get; }
    }
}