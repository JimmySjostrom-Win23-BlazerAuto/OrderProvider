using Data.Contexts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace OrderProvider.Functions
{
    public class Order
    {
        private readonly ILogger<Order> _logger;
        private readonly DataContext _context;

        public Order(ILogger<Order> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Function("Order")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("Processing request to place order.");

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                var orderEntity = JsonConvert.DeserializeObject<OrderEntity>(body);
                if (orderEntity != null)
                {
                    var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.CourseId == orderEntity.CourseId && o.UserId == orderEntity.UserId);
                    if (existingOrder != null)
                    {
                        _context.Entry(existingOrder).CurrentValues.SetValues(orderEntity);
                        await _context.SaveChangesAsync();

                        return new OkObjectResult(new
                        {
                            Status = 200,
                            Message = "Order was updated."
                        });
                    }

                    _context.Orders.Add(orderEntity);
                    await _context.SaveChangesAsync();

                    return new OkObjectResult(new
                    {
                        Status = 200,
                        Message = "Order was added."
                    });
                }
            }

            return new BadRequestObjectResult(new
            {
                Status = 400,
                Message = "Unable to place order right now."
            });
        }
    }
}
