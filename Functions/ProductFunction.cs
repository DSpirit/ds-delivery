using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using DeliveryService.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using DeliveryService.Formats;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace DeliveryService.Functions
{
    public static class ProductFunction
    {
        [FunctionName(nameof(AddProduct))]
        public static async Task<IActionResult> AddProduct(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Products")] ProductEntity product,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            try
            {   
                await client.SignalEntityAsync(new EntityId(nameof(ProductEntity), product.Name), EntityOperation.Add.ToString(), product);
                return new OkResult();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        [FunctionName(nameof(GetProduct))]
        public static async Task<IActionResult> GetProduct(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products/{productId}")] HttpRequest req,
         string productId,
         [DurableClient] IDurableEntityClient client,
         ILogger log)
        {
            try
            {
                var res = await client.ReadEntityStateAsync<ProductEntity>(new EntityId(nameof(ProductEntity), productId));
                return new OkObjectResult(res.EntityState);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        [FunctionName(nameof(ListProduct))]
        public static async Task<IActionResult> ListProduct(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "products")] HttpRequest req,
         [DurableClient] IDurableEntityClient client,
         ILogger log,
         CancellationToken token)
        {
            try
            {
                var entities = await client.ListEntitiesAsync(new EntityQuery() { EntityName = nameof(ProductEntity), FetchState = true }, token);
                return new OkObjectResult(entities);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        [FunctionName(nameof(DeleteProduct))]
        public static async Task<IActionResult> DeleteProduct(
         [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "products/{productId}")] HttpRequest req,
         string productId,
         [DurableClient] IDurableEntityClient client,
         ILogger log,
         CancellationToken token)
        {
            try
            {   
                await client.SignalEntityAsync(new EntityId(nameof(ProductEntity), productId), EntityOperation.Delete.ToString());
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
