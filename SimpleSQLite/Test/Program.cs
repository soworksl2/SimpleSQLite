using SimpleSQLite;
using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Proyectos C#\Projects\SimpleSQLite\SimpleSQLite\Test\bin\Debug\DB.db3";

            Product Refrescos = new Product()
            {
                name = "refrescos",
                Price = 20m,
            };
            Product Jugos = new Product()
            {
                name = "jugos",
                Price = 25m
            };
            Product Cervezas = new Product()
            {
                name = "Cervezas",
                Price = 30m
            };

            Brand Cocacola = new Brand()
            {
                Name = "Cocacola",
                Products = new List<Product>()
                {
                    Refrescos, Jugos, Cervezas
                }
            };

            //SQLiteOperations.Insert<Product>(new List<Product>() { Refrescos, Jugos, Cervezas}, path);

            Product xxx = SQLiteOperations.Read<Product>(p=>p.Price==30m, path).First();

            SQLiteOperations.FillWithChildren<Product>(xxx, path, true);

            //Expression<Func<Product, bool>> expression = p => p.Price >= 25;

            //List<Product> xxx = SQLiteOperations.Read<Product>(expression,path).ToList();
        }
    }

    public class Product
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string name { get; set; }
        [ForeignKey(typeof(Brand))]
        public int IDBrand { get; set; }
        public decimal Price { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public Brand Brand { get; set; }
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Tags> Tags { get; set; }
    }

    public class Brand
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Name { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Product> Products { get; set; }
    }
    
    public class Tags
    {
        [PrimaryKey,AutoIncrement]
        public int ID { get; set; }
        public string tag { get; set; }
        [ForeignKey(typeof(Product))]
        public int IdProduct { get; set; }
    }
}
