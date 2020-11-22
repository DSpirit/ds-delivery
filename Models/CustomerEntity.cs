using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DeliveryService.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CustomerEntity
    {
        [JsonProperty(nameof(Email))]
        public string Email { get; set; }
        [JsonProperty(nameof(FirstName))]
        public string FirstName { get; set; }
        [JsonProperty(nameof(LastName))]
        public string LastName { get; set; }

        public CustomerEntity Get() => this;

        public void Add(CustomerEntity entity)
        {
            this.Email = entity.Email;
            this.FirstName = entity.FirstName;
            this.LastName = entity.LastName;
        }

        public void Delete()
        {
            this.Email = null;
            this.FirstName = null;
            this.LastName = null;
        }


        [FunctionName(nameof(CustomerEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<CustomerEntity>();
    }
}