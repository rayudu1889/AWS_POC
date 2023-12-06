namespace Apistart_stop.Entity
{
    public class RequestModel
    {
        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get; set; }
        public string Region { get; set; }
        public string AccountId { get; set; }
        public string ResourceGroup { get; set; }
        public string InstanceType { get; set; }
        public string InstanceId { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
    }
}
