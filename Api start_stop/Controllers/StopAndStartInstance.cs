using Amazon.EC2.Model;
using Amazon.EC2;
using Microsoft.AspNetCore.Mvc;
using Amazon;
 
namespace Api_start_stop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StopAndStartInstance : Controller
    {
        private readonly ILogger<StopAndStartInstance> _logger;

        public StopAndStartInstance(ILogger<StopAndStartInstance> logger)
        {
            _logger = logger;
        }

        [HttpPost("start-all-instances")]
        public async Task<IActionResult> StartAllEC2Instances([FromQuery] string AWSAccessKey, [FromQuery] string AWSSecretKey, [FromQuery] string Region)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(AWSAccessKey, AWSSecretKey, RegionEndpoint.GetBySystemName(Region));

                var instanceIds = await GetInstanceIdsAsync(ec2Client);

                var request = new StartInstancesRequest
                {
                    InstanceIds = instanceIds
                };

                var response = await ec2Client.StartInstancesAsync(request);

                return Ok(response.StartingInstances);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("stop-all-instances")]
        public async Task<IActionResult> StopAllEC2Instances([FromQuery] string AWSAccessKey, [FromQuery] string AWSSecretKey, [FromQuery] string Region)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(AWSAccessKey, AWSSecretKey, RegionEndpoint.GetBySystemName(Region));

                var instanceIds = await GetInstanceIdsAsync(ec2Client);

                var request = new StopInstancesRequest
                {
                    InstanceIds = instanceIds
                };

                var response = await ec2Client.StopInstancesAsync(request);

                return Ok(response.StoppingInstances);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("start-instance")]
        public async Task<IActionResult> StartEC2Instance([FromQuery] string AWSAccessKey, [FromQuery] string AWSSecretKey, [FromQuery] string Region, [FromQuery] string InstanceId)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(AWSAccessKey, AWSSecretKey, RegionEndpoint.GetBySystemName(Region));

                var request = new StartInstancesRequest
                {
                    InstanceIds = new List<string> {InstanceId }
                };

                var response = await ec2Client.StartInstancesAsync(request);

                return Ok(response.StartingInstances);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("stop-instance")]
        public async Task<IActionResult> StopEC2Instance([FromQuery] string AWSAccessKey, [FromQuery] string AWSSecretKey, [FromQuery] string Region, [FromQuery] string InstanceId)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(AWSAccessKey, AWSSecretKey, RegionEndpoint.GetBySystemName(Region));

                var request = new StopInstancesRequest
                {
                    InstanceIds = new List<string> { InstanceId }
                };

                var response = await ec2Client.StopInstancesAsync(request);

                return Ok(response.StoppingInstances);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        private static async Task<List<string>> GetInstanceIdsAsync(IAmazonEC2 ec2Client)
        {
            var request = new DescribeInstancesRequest();
            var response = await ec2Client.DescribeInstancesAsync(request);

            var instanceIds = new List<string>();

            foreach (var reservation in response.Reservations)
            {
                foreach (var instance in reservation.Instances)
                {
                    instanceIds.Add(instance.InstanceId);
                }
            }

            return instanceIds;
        }


    }
}

