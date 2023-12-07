using Amazon.EC2.Model;
using Amazon.EC2;
using Apistart_stop.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Apistart_stop.Controllers;
using Amazon;

namespace Api_start_stop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulingInstance : ControllerBase
    {
        private readonly ILogger<SchedulingInstance> _logger;
        private readonly Dictionary<string, Timer> instanceTimers = new Dictionary<string, Timer>();
        private readonly object lockObj = new object();

        public SchedulingInstance(ILogger<SchedulingInstance> logger)
        {
            _logger = logger;

        }

        private async Task StartInstancesAsync(AmazonEC2Client ec2Client, List<string> instanceIds)
        {
            if (instanceIds.Any())
            {
                var startRequest = new StartInstancesRequest
                {
                    InstanceIds = instanceIds
                };

                var response = await ec2Client.StartInstancesAsync(startRequest);

                _logger.LogInformation($"Started instances: {string.Join(", ", response.StartingInstances.Select(i => i.InstanceId))}");
            }
        }

        private async Task<List<string>> GetFilteredInstanceIdsAsync(AmazonEC2Client ec2Client, string tagKey, string tagValue)
        {
            var describeInstancesRequest = new DescribeInstancesRequest();
            var describeInstancesResponse = await ec2Client.DescribeInstancesAsync(describeInstancesRequest);

            var filteredInstanceIds = describeInstancesResponse.Reservations
                .SelectMany(reservation => reservation.Instances)
                .Where(instance => instance.Tags.Any(tag => tag.Key == tagKey && tag.Value == tagValue))
                .Select(instance => instance.InstanceId)
                .ToList();

            return filteredInstanceIds;
        }

        [HttpPost("schedule-start-vm-instance")]
        public IActionResult ScheduleStartVMInstance([FromBody] RequestModel requestModel)
        {
            try
            {
                var timer = new Timer(_ => StartInstance(requestModel.InstanceId), null, requestModel.DelayMilliseconds, Timeout.Infinite);

                lock (lockObj)
                {
                    instanceTimers[requestModel.InstanceId] = timer;
                }

                return Ok($"Instance {requestModel.InstanceId} scheduled to start after {requestModel.DelayMilliseconds} milliseconds at VM level.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("schedule-stop-vm-instance")]
        public IActionResult ScheduleStopVMInstance([FromBody] RequestModel requestModel)
        {
            try
            {
                var timer = new Timer(_ => StopInstance(requestModel.InstanceId), null, requestModel.DelayMilliseconds, Timeout.Infinite);

                lock (lockObj)
                {
                    instanceTimers[requestModel.InstanceId] = timer;
                }

                return Ok($"Instance {requestModel.InstanceId} scheduled to stop after {requestModel.DelayMilliseconds} milliseconds at VM level.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("schedule-start-rg-instance")]
        public IActionResult ScheduleStartRGInstance([FromBody] RequestModel requestModel)
        {
            try
            {


                return Ok($"Instances in the resource group scheduled to start at RG level.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("schedule-stop-rg-instance")]
        public IActionResult ScheduleStopRGInstance([FromBody] RequestModel requestModel)
        {
            try
            {

                return Ok($"Instances in the resource group scheduled to stop at RG level.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("schedule-start-subscription-instance")]
        public IActionResult ScheduleStartSubscriptionInstance([FromBody] RequestModel requestModel)
        {
            try
            {
                var timer = new Timer(_ => StartInstance(requestModel.InstanceId), null, requestModel.DelayMilliseconds, Timeout.Infinite);

                lock (lockObj)
                {
                    instanceTimers[requestModel.InstanceId] = timer;
                }

                return Ok($"Instance {requestModel.InstanceId} scheduled to start after {requestModel.DelayMilliseconds} milliseconds at subscription level.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("schedule-stop-subscription-instance")]
        public IActionResult ScheduleStopSubscriptionInstance([FromBody] RequestModel requestModel)
        {
            try
            {
                var timer = new Timer(_ => StopInstance(requestModel.InstanceId), null, requestModel.DelayMilliseconds, Timeout.Infinite);

                lock (lockObj)
                {
                    instanceTimers[requestModel.InstanceId] = timer;
                }

                return Ok($"Instance {requestModel.InstanceId} scheduled to stop after {requestModel.DelayMilliseconds} milliseconds at subscription level.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("schedule-start-labeled-instance")]
        public IActionResult ScheduleStartLabeledInstance([FromBody] RequestModel requestModel)
        {
            try
            {

                return Ok($"Labeled instances scheduled to start.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("schedule-stop-labeled-instance")]
        public IActionResult ScheduleStopLabeledInstance([FromBody] RequestModel requestModel)
        {
            try
            {

                return Ok($"Labeled instances scheduled to stop.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        private void StartInstance(string instanceId)
        {
            lock (lockObj)
            {
                instanceTimers.Remove(instanceId);
            }
        }

        private void StopInstance(string instanceId)
        {
            lock (lockObj)
            {
                instanceTimers.Remove(instanceId);
            }
        }

        [HttpPost("schedule-vm-instances")]
        public async Task<IActionResult> ScheduleVMInstances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

                var vmInstanceIds = await GetFilteredInstanceIdsAsync(ec2Client, "YourTagName", "YourTagValue");

                var scheduledInstanceIds = FineTuneScheduling(vmInstanceIds);

                await StartInstancesAsync(ec2Client, scheduledInstanceIds);

                return Ok("VM instances scheduled successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        private List<string> FineTuneScheduling(List<string> instanceIds)
        {

            var scheduledInstanceIds = instanceIds.Take(2).ToList();
            return scheduledInstanceIds;
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
