using Amazon.EC2.Model;
using Amazon.EC2;
using Apistart_stop;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.AutoScaling.Model;
 
namespace Api_start_stop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Scaling : ControllerBase
    {
        private IAmazonEC2 _ec2Client;

        [HttpPost("ScaleInstances")]
        public async Task<IActionResult> ScaleInstances([FromQuery]string AWSAccessKey, [FromQuery] string AWSSecretKey, [FromQuery] string InstanceType, [FromQuery] int MinCount, [FromQuery] int MaxCount, [FromQuery] string InstanceId)

        {
            try
            {
                // Replace with your AWS credentials and region
                var awsCredentials = new Amazon.Runtime.BasicAWSCredentials(AWSAccessKey, AWSSecretKey);
                _ec2Client = new AmazonEC2Client(awsCredentials, RegionEndpoint.USEast1);

                if (string.IsNullOrEmpty(InstanceType) || MinCount <= 0 || MaxCount <= 0)
                {
                    return BadRequest("Invalid scaling parameters.");
                }

                // Pass a specific instance ID to modify
                await ScaleEC2Instance(InstanceId, InstanceType, MinCount, MaxCount);

                return Ok($"EC2 instance {InstanceId} scaled to {InstanceType} successfully.");
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, $"Error scaling EC2 instances: {ex.Message}");
            }
        }

        private async Task ScaleEC2Instance(string instanceId, string instanceType, int minCount, int maxCount)
        {
            // Modify the instance type
            var modifyRequest = new ModifyInstanceAttributeRequest
            {
                InstanceId = instanceId,
                InstanceType = instanceType
            };
            await _ec2Client.ModifyInstanceAttributeAsync(modifyRequest);

            // Set desired instance count
            var modifyCountRequest = new ModifyInstanceCapacityReservationAttributesRequest
            {
                InstanceId = instanceId,
                CapacityReservationSpecification = new CapacityReservationSpecification
                {
                    CapacityReservationPreference = CapacityReservationPreference.Open,
                }
            };
            await _ec2Client.ModifyInstanceCapacityReservationAttributesAsync(modifyCountRequest);
        }
    }
}
