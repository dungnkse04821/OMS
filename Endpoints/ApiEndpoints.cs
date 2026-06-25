using OMS.Models;
using OMS.Repositories;

namespace OMS.Endpoints
{
    public static class ApiEndpoints
    {
        public static void MapApiEndpoints(this IEndpointRouteBuilder app)
        {
            var apiGroup = app.MapGroup("/api");

            // Orders API
            var ordersGroup = apiGroup.MapGroup("/orders");
            
            ordersGroup.MapGet("/", async (IOrderRepository repo) => 
            {
                var orders = await repo.GetAllAsync();
                return Results.Ok(orders);
            });

            ordersGroup.MapGet("/{id}", async (string id, IOrderRepository repo) => 
            {
                var order = await repo.GetByIdAsync(id);
                return order != null ? Results.Ok(order) : Results.NotFound();
            });

            ordersGroup.MapPost("/", async (Order order, IOrderRepository repo) => 
            {
                await repo.AddAsync(order);
                return Results.Created($"/api/orders/{order.Id}", order);
            });

            ordersGroup.MapPut("/{id}", async (string id, Order order, IOrderRepository repo) => 
            {
                if (id != order.Id) return Results.BadRequest("Id mismatch");
                var existing = await repo.GetByIdAsync(id);
                if (existing == null) return Results.NotFound();
                
                await repo.UpdateAsync(order);
                return Results.NoContent();
            });

            ordersGroup.MapDelete("/{id}", async (string id, IOrderRepository repo) => 
            {
                var existing = await repo.GetByIdAsync(id);
                if (existing == null) return Results.NotFound();
                
                await repo.DeleteAsync(id);
                return Results.NoContent();
            });

            // Products API
            var productsGroup = apiGroup.MapGroup("/products");

            productsGroup.MapGet("/", async (IProductRepository repo) => 
            {
                var products = await repo.GetAllAsync();
                return Results.Ok(products);
            });

            productsGroup.MapGet("/{sku}", async (string sku, IProductRepository repo) => 
            {
                var product = await repo.GetByIdAsync(sku);
                return product != null ? Results.Ok(product) : Results.NotFound();
            });

            productsGroup.MapPost("/", async (Product product, IProductRepository repo) => 
            {
                await repo.AddAsync(product);
                return Results.Created($"/api/products/{product.Sku}", product);
            });

            productsGroup.MapPut("/{sku}", async (string sku, Product product, IProductRepository repo) => 
            {
                if (sku != product.Sku) return Results.BadRequest("Sku mismatch");
                var existing = await repo.GetByIdAsync(sku);
                if (existing == null) return Results.NotFound();
                
                await repo.UpdateAsync(product);
                return Results.NoContent();
            });

            productsGroup.MapDelete("/{sku}", async (string sku, IProductRepository repo) => 
            {
                var existing = await repo.GetByIdAsync(sku);
                if (existing == null) return Results.NotFound();
                
                await repo.DeleteAsync(sku);
                return Results.NoContent();
            });
        }
    }
}
