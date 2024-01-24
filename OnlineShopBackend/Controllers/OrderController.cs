using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper;

namespace OnlineShopBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public OrderController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet("forUser/{idUser}")]
        [Authorize]
        public List<Order> GetOrdersForUser(int idUser)
        {
            using (var db = new SqlConnection(configuration.GetConnectionString("connString")))
            {
                var orders = db.Query<Order>(
                    "SELECT * FROM [Order] WHERE idClient = @idUser", new { idUser }).ToList();
                var orderIds = orders.Select(o => o.Id).ToList();
                var details = db.Query<OrderDetail>(
                    "SELECT * FROM OrderDetail WHERE idOrder IN @orderIds", new { orderIds }
                    );
                foreach (var order in orders)
                {
                    order.Details = details.Where(d => d.IdOrder == order.Id).ToList();
                }
                return orders;
            }
        }
    }
}
