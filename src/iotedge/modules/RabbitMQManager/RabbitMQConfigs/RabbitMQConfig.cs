namespace RabbitMQManager.RabbitMQConfigs
{
    public class RabbitMQConfig
    {
        public string Hostname { get; set; }

        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public FederationConfig FederationConfig { get; set; }
    }
}
