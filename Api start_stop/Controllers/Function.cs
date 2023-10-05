using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Apistart_stop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EC2Controller : ControllerBase
    {
        private readonly ILogger<EC2Controller> _logger;

        public EC2Controller(ILogger<EC2Controller> logger)
        {
            _logger = logger;
        }

        [HttpPost("get-instances")]
        public async Task<IActionResult> GetEC2Instances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

                var request = new DescribeInstancesRequest();
                var response = await ec2Client.DescribeInstancesAsync(request);

                var instances = new List<Instance>();

                foreach (var reservation in response.Reservations)
                {
                    instances.AddRange(reservation.Instances);
                }

                return Ok(instances);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("start-all-instances")]
        public async Task<IActionResult> StartAllEC2Instances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

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
        public async Task<IActionResult> StopAllEC2Instances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

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
