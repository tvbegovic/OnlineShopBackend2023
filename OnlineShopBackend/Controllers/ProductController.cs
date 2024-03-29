﻿using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace OnlineShopBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public ProductController(IConfiguration configuration) 
        {
            this.configuration = configuration;
        }

        [HttpGet("")]
        [Authorize]
        public List<Product> GetProducts()
        {
            using(var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                return db.Query<Product>("SELECT * FROM Product").ToList();
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public Product? GetProduct(int id)
        {
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                return db.QueryFirstOrDefault<Product>("SELECT * FROM Product WHERE id = @id", new { id });
            }
        }

        [HttpGet("search")]
        public List<Product> Search(string? name = null, double? priceFrom = null, double? priceTo = null)
        {
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                return db.Query<Product>(
                    @"SELECT * FROM Product
                    WHERE ([Name] LIKE @name OR @name IS NULL)
                    AND (Price >= @pricefrom OR @pricefrom IS NULL)
                    AND (Price <= @priceto OR @priceto IS NULL)",
                    new { name = $"%{name}%", priceFrom, priceTo }
                    ).ToList();
            }
        }

        [HttpPost("")]
        [Authorize]
        public IActionResult Create(Product product)
        {
            if (string.IsNullOrEmpty(product.Name))
                return BadRequest("Naziv proizvoda je obavezan");
            if(product.Price == null)
            {
                return BadRequest("Cijena je obavezna");
            }
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                var sql = @"INSERT INTO Product(
                        Name,Code,Price,IdManufacturer,IdCategory
                        ) OUTPUT inserted.id VALUES(
                        @Name,@Code,@Price,@IdManufacturer,@IdCategory
                        )";
                product.ID = db.ExecuteScalar<int>(sql,product);
                return Ok(product);
            }
        }

        [HttpPut("")]
        [Authorize]
        public IActionResult Update(Product product)
        {
            if (string.IsNullOrEmpty(product.Name))
                return BadRequest("Naziv proizvoda je obavezan");
            if (product.Price == null)
            {
                return BadRequest("Cijena je obavezna");
            }
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                var sql = @"UPDATE Product SET 
                Name=@Name,Code=@Code,Price=@Price,IdManufacturer=@IdManufacturer,IdCategory=@IdCategory
                WHERE id = @id";
                db.Execute(sql, product);
                return Ok();
            }
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                var sql = "DELETE FROM Product WHERE id = @id";
                db.Execute(sql, new { id });
            }
        }
    }
}
