using System;
using System.Collections.Generic;
using System.Linq;
using OMS.Models;

namespace OMS.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Customers.Any())
            {
                var customers = new List<Customer>
                {
                    new Customer { Id = "KH0001", FullName = "Nguyễn Văn Anh", PhoneNumber = "0987654321", Address = "123 Đường Lê Lợi, Quận 1, TP. HCM", Reference = "Facebook", Note = "Khách quen" },
                    new Customer { Id = "KH0002", FullName = "Trần Thị Bình", PhoneNumber = "0912345678", Address = "456 Đường Nguyễn Trãi, Quận 5, TP. HCM", Reference = "Tiktok", Note = "Hay mua váy hoa" },
                    new Customer { Id = "KH0003", FullName = "Lê Hoàng Cường", PhoneNumber = "0909998887", Address = "789 Đường Điện Biên Phủ, Bình Thạnh, TP. HCM", Reference = "Người quen giới thiệu", Note = "Giao giờ hành chính" }
                };
                context.Customers.AddRange(customers);
            }

            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product { Sku = "AO-THUN-VIBE", Name = "Áo thun cotton VibeCode Premium", Category = "Áo thun", ImportPrice = 120000, SellingPrice = 250000, Source = "Xưởng may HN", Warehouse = "Kho HCM 1" },
                    new Product { Sku = "VAY-HOA-VINTAGE", Name = "Váy hoa nhí Vintage Pháp", Category = "Váy đầm", ImportPrice = 280000, SellingPrice = 490000, Source = "Quảng Châu", Warehouse = "Kho HCM 2" },
                    new Product { Sku = "JEAN-SLIM-01", Name = "Quần Jeans Slimfit Co giãn Nam", Category = "Quần", ImportPrice = 180000, SellingPrice = 350000, Source = "Xưởng may HCM", Warehouse = "Kho HCM 1" }
                };
                context.Products.AddRange(products);
            }

            if (!context.Orders.Any())
            {
                var orders = new List<Order>
                {
                    new Order
                    {
                        Id = "DH0001",
                        OrderDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-5), DateTimeKind.Utc),
                        Source = "Quảng Châu",
                        Warehouse = "Kho HCM 2",
                        Code = "VAY-HOA-VINTAGE",
                        Category = "Váy đầm",
                        ProductName = "Váy hoa nhí Vintage Pháp",
                        Color = "Vàng nhạt",
                        Size = "M",
                        SellingPrice = 490000,
                        Quantity = 1,
                        TotalAmount = 490000,
                        CustomerName = "Trần Thị Bình",
                        Deposit = 100000,
                        Discount = 0,
                        RemainingAmount = 390000,
                        ArrivalDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-1), DateTimeKind.Utc),
                        PaymentDate = null,
                        ImportPrice = 280000,
                        TotalImportCost = 280000,
                        Profit = 210000,
                        Status = "Đã về",
                        PhoneNumber = "0912345678",
                        ShippingAddress = "456 Đường Nguyễn Trãi, Quận 5, TP. HCM"
                    },
                    new Order
                    {
                        Id = "DH0002",
                        OrderDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-3), DateTimeKind.Utc),
                        Source = "Xưởng may HN",
                        Warehouse = "Kho HCM 1",
                        Code = "AO-THUN-VIBE",
                        Category = "Áo thun",
                        ProductName = "Áo thun cotton VibeCode Premium",
                        Color = "Đen",
                        Size = "L",
                        SellingPrice = 250000,
                        Quantity = 2,
                        TotalAmount = 500000,
                        CustomerName = "Nguyễn Văn Anh",
                        Deposit = 500000,
                        Discount = 20000,
                        RemainingAmount = -20000,
                        ArrivalDate = DateTime.SpecifyKind(DateTime.Today.AddDays(2), DateTimeKind.Utc),
                        PaymentDate = DateTime.SpecifyKind(DateTime.Today.AddDays(-3), DateTimeKind.Utc),
                        ImportPrice = 120000,
                        TotalImportCost = 240000,
                        Profit = 260000,
                        Status = "Đã đặt",
                        PhoneNumber = "0987654321",
                        ShippingAddress = "123 Đường Lê Lợi, Quận 1, TP. HCM"
                    }
                };
                context.Orders.AddRange(orders);
            }

            context.SaveChanges();
        }
    }
}
