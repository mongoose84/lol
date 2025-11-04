namespace RiotProxy.Domain
{
    public class User : EntityBase
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;      
    }
}