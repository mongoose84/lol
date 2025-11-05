using System.Text.Json;

namespace RiotProxy.Domain.Entities
{

    public class EntityBase
    {
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}