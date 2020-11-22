using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DeliveryService.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ProductEntity
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Price")]
        public double Price{ get; set; }

        public ProductEntity Get() => this;

        public void Add(ProductEntity entity)
        {
            this.Name = entity.Name;
            this.Price = entity.Price;
        }

        [FunctionName(nameof(ProductEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<ProductEntity>();
    }
}