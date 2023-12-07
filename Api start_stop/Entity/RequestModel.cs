namespace Apistart_stop.Entity
{
    public class RequestModel
    {
        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get; set; }
        public string Region { get; set; }
        public string RgName { get; set; }
        public string AccountId { get; set; }
        public string InstanceId { get; set; }
        public int DelayMilliseconds { get; set; }
    }
}
