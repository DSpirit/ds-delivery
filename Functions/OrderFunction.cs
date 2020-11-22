using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using DeliveryService.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading;
using DeliveryService.Formats;

namespace DeliveryService.Functions
{
    public static class OrderFunction
    {
        [FunctionName(nameof(AddOrder))]
        public static async Task<IActionResult> AddOrder(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")] OrderEntity order,
             [DurableClient] IDurableEntityClient client,
             ILogger log)
        {
            if (order is null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            if (client is null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (log is null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            try
            {
                order.Id = Guid.NewGuid();
                
                foreach (var p in order.Products)
                {
                    var product = await client.ReadEntityStateAsync<ProductEntity>(new EntityId(nameof(ProductEntity), p));
                    order.Total += product.EntityState.Price;
                }

                await client.SignalEntityAsync(new EntityId(nameof(OrderEntity), order.Id.ToString()), EntityOperation.Add.ToString(), order);
                return new OkResult();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        [FunctionName(nameof(GetOrder))]
        public static async Task<IActionResult> GetOrder(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{orderId}")] HttpRequest req,
         string orderId,
         [DurableClient] IDurableEntityClient client,
         ILogger log)
        {
            try
            {
                var res = await client.ReadEntityStateAsync<OrderEntity>(new EntityId(nameof(OrderEntity), orderId));
                return new OkObjectResult(res.EntityState);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        [FunctionName(nameof(ListOrder))]
        public static async Task<IActionResult> ListOrder(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders")] HttpRequest req,
         [DurableClient] IDurableEntityClient client,
         ILogger log,
         CancellationToken token)
        {
            try
            {
                var entities = await client.ListEntitiesAsync(new EntityQuery() { EntityName = nameof(OrderEntity), FetchState = true }, token);
                return new OkObjectResult(entities);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        [FunctionName(nameof(DeleteOrder))]
        public static async Task<IActionResult> DeleteOrder(
         [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "orders/{orderId}")] HttpRequest req,
         string orderId,
         [DurableClient] IDurableEntityClient client,
         ILogger log,
         CancellationToken token)
        {
            try
            {
                await client.SignalEntityAsync(new EntityId(nameof(OrderEntity), orderId), EntityOperation.Delete.ToString());
                var res = await client.CleanEntityStorageAsync(true, true, token);
                return new OkObjectResult(res);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }
    }
}
