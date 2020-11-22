using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DeliveryService.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OrderEntity
    {
        [JsonProperty("Id")]
        public Guid Id { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("Products")]
        public string[] Products { get; set; }

        [JsonProperty("Completed")]
        public bool Completed { get; set; }

        [JsonProperty("Total")]
        public double Total { get; set; }

        [JsonProperty("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("CompletedAt")]
        public DateTime CompletedAt { get; set; }

        public void Add(OrderEntity order)
        {
            this.Id = order.Id;
            this.CustomerId = order.CustomerId;
            this.Products = order.Products;
            this.Completed = order.Completed;
            this.Total = order.Total;

            if (!order.Completed)
            {
                this.CreatedAt = DateTime.UtcNow;
            }
            else if (order.Completed)
            {
                this.CompletedAt = DateTime.UtcNow;
            }
        }

        public OrderEntity Get() => this;

        public void Delete()
        {   
            this.CustomerId = null;
            this.Products = null;
        }

        [FunctionName(nameof(OrderEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<OrderEntity>();
    }
}