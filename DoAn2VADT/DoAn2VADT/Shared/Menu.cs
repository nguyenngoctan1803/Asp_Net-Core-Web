using DoAn2VADT.Database;
using DoAn2VADT.Database.Entities;
using System;
using System.Collections.Generic;
namespace DoAn2VADT.Shared
{

    public class Menu
    {
        public List<Category> Categories { get; set; }
        public List<Brand> Brands { get; set; }
    }
}
