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
    public static class CustomerFunction
    {
        [FunctionName(nameof(AddCustomer))]
        public static async Task<IActionResult> AddCustomer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "customers")] CustomerEntity customer,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            try
            {   
                await client.SignalEntityAsync(new EntityId(nameof(CustomerEntity), customer.Email), EntityOperation.Add.ToString(), customer);
                return new OkResult();
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        [FunctionName(nameof(GetCustomer))]
        public static async Task<IActionResult> GetCustomer(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers/{customerId}")] HttpRequest req,
         string customerId,
         [DurableClient] IDurableEntityClient client,
         ILogger log)
        {
            try
            {
                var res = await client.ReadEntityStateAsync<CustomerEntity>(new EntityId(nameof(CustomerEntity), customerId));
                return new OkObjectResult(res.EntityState);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        [FunctionName(nameof(ListCustomer))]
        public static async Task<IActionResult> ListCustomer(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "customers")] HttpRequest req,
         [DurableClient] IDurableEntityClient client,
         ILogger log,
         CancellationToken token)
        {
            try
            {
                var entities = await client.ListEntitiesAsync(new EntityQuery() { EntityName = nameof(CustomerEntity) }, token);
                return new OkObjectResult(entities);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e);
            }
        }

        [FunctionName(nameof(DeleteCustomer))]
        public static async Task<IActionResult> DeleteCustomer(
         [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "customers/{customerId}")] HttpRequest req,
         string customerId,
         [DurableClient] IDurableEntityClient client,
         ILogger log,
         CancellationToken token)
        {
            try
            {   
                await client.SignalEntityAsync(new EntityId(nameof(CustomerEntity), customerId), EntityOperation.Delete.ToString());
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
