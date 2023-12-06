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
using Amazon.ComputeOptimizer;
using Amazon.ComputeOptimizer.Model;

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
        [HttpPost("refresh-vm-account")]
        public async Task<IActionResult> RefreshVMAccountLevel([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

                var request = new DescribeInstancesRequest();
                var response = await ec2Client.DescribeInstancesAsync(request);

                // Assuming GetInstanceIdsAsync returns a List<string> of instance IDs
                var instanceIds = await GetInstanceIdsAsync(ec2Client);

                foreach (var reservation in response.Reservations)
                {
                    foreach (var instance in reservation.Instances)
                    {
                        // Add the InstanceId of each instance to the list
                        instanceIds.Add(instance.InstanceId);
                    }
                }

                return Ok(instanceIds);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        //[HttpPost("refresh-vm-resource-group")]
        //public async Task<IActionResult> RefreshVMResourceGroupLevel([FromBody] RequestModel requestModel)
        //{
        //    try
        //    {
        //        var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));


        //       // var resourceGroupFilter = new Filter
        //      
        //        {
        //            Name = "tag:ResourceGroup",
        //            Values = new List<string> { requestModel.ResourceGroup }
        //        };

        //        var request = new DescribeInstancesRequest
        //        {
        //            // Filters = new List<Filter> { resourceGroupFilter }
        //          
        //        };

        //        var response = await ec2Client.DescribeInstancesAsync(request);

        //        var instanceIds = new List<string>();

        //        foreach (var reservation in response.Reservations)
        //        {
        //            foreach (var instance in reservation.Instances)
        //            {

        //                instanceIds.Add(instance.InstanceId);
        //            }
        //        }

        //        return Ok(instanceIds);


        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error: {ex.Message}");
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpPost("refresh-vm-resource-group")]
        public async Task<IActionResult> RefreshVMResourceGroupLevel([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

                // Use the correct Filter class from the Amazon.EC2.Model namespace
                var resourceGroupFilter = new Amazon.EC2.Model.Filter
                {
                    Name = "tag:ResourceGroup",
                    Values = new List<string> { requestModel.ResourceGroup }
                };

                var request = new DescribeInstancesRequest
                {
                    Filters = new List<Amazon.EC2.Model.Filter> { resourceGroupFilter }
                };

                var response = await ec2Client.DescribeInstancesAsync(request);

                var instanceIds = new List<string>();

                foreach (var reservation in response.Reservations)
                {
                    foreach (var instance in reservation.Instances)
                    {
                        instanceIds.Add(instance.InstanceId);
                    }
                }

                return Ok(instanceIds);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("get-recommendations")]
        public async Task<IActionResult> GetEC2Recommendations([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

                var instanceId = await GetInstanceIdsAsync(ec2Client);


                var computeOptimizerClient = new AmazonComputeOptimizerClient(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

                var getRecommendationsRequest = new GetEC2InstanceRecommendationsRequest
                {
                    InstanceArns = new List<string> { $"arn:aws:ec2:{requestModel.Region}:{requestModel.AccountId}:instance/{instanceId}" }
                };

                var recommendationsResponse = await computeOptimizerClient.GetEC2InstanceRecommendationsAsync(getRecommendationsRequest);

                return Ok(recommendationsResponse);
            }
            catch (AmazonComputeOptimizerException ex)
            {
                _logger.LogError($"AWS Compute Optimizer Error: {ex.Message}");
                return BadRequest($"AWS Compute Optimizer Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
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

        private async Task StopInstancesAsync(AmazonEC2Client ec2Client, List<string> instanceIds)
        {
            if (instanceIds.Any())
            {
                var stopRequest = new StopInstancesRequest
                {
                    InstanceIds = instanceIds
                };

                var response = await ec2Client.StopInstancesAsync(stopRequest);

                _logger.LogInformation($"Stopped instances: {string.Join(", ", response.StoppingInstances.Select(i => i.InstanceId))}");
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

        private async Task<List<string>> GetLabeledInstanceIdsAsync(AmazonEC2Client ec2Client, string labelKey, string labelValue)
        {
            var describeInstancesRequest = new DescribeInstancesRequest();
            var describeInstancesResponse = await ec2Client.DescribeInstancesAsync(describeInstancesRequest);

            var labeledInstanceIds = describeInstancesResponse.Reservations
                .SelectMany(reservation => reservation.Instances)
                .Where(instance => instance.Tags.Any(tag => tag.Key == labelKey && tag.Value == labelValue))
                .Select(instance => instance.InstanceId)
                .ToList();

            return labeledInstanceIds;
        }

        //[HttpPost("start-rg-instances")]
        //public async Task<IActionResult> StartRGEC2Instances([FromBody] RequestModel requestModel)
        //{
        //    try
        //    {
        //        var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

        //        var rgInstanceIds = await GetInstanceIdsInResourceGroupAsync(ec2Client, "YourResourceGroup");
        //        await StartInstancesAsync(ec2Client, rgInstanceIds);

        //        return Ok("Instances in Resource Group started successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error: {ex.Message}");
        //        return BadRequest(ex.Message);
        //    }
        //}

        //private Task<List<string>> GetInstanceIdsInResourceGroupAsync(AmazonEC2Client ec2Client, string v)
        //{
        //    throw new NotImplementedException();
        //}

        //[HttpPost("stop-rg-instances")]
        //public async Task<IActionResult> StopRGEC2Instances([FromBody] RequestModel requestModel)
        //{
        //    try
        //    {
        //        var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

        //        // RG Level: Implement RG-level scheduling logic
        //        var rgInstanceIds = await GetInstanceIdsInResourceGroupAsync(ec2Client, "YourResourceGroup");
        //        await StopInstancesAsync(ec2Client, rgInstanceIds);

        //        return Ok("Instances in Resource Group stopped successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error: {ex.Message}");
        //        return BadRequest(ex.Message);
        //    }
        //}

        [HttpPost("start-subscription-instances")]
        public async Task<IActionResult> StartSubscriptionEC2Instances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

                var allInstanceIds = await GetAllInstanceIdsAsync(ec2Client);
                await StartInstancesAsync(ec2Client, allInstanceIds);

                return Ok("All instances in the subscription started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        private async Task<List<string>> GetAllInstanceIdsAsync(AmazonEC2Client ec2Client)
        {
            var request = new DescribeInstancesRequest();
            var response = await ec2Client.DescribeInstancesAsync(request);

            var allInstanceIds = new List<string>();
            foreach (var reservation in response.Reservations)
            {
                foreach (var instance in reservation.Instances)
                {
                    allInstanceIds.Add(instance.InstanceId);
                }
            }

            return allInstanceIds;
        }

        [HttpPost("stop-subscription-instances")]
        public async Task<IActionResult> StopSubscriptionEC2Instances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

                var allInstanceIds = await GetAllInstanceIdsAsync(ec2Client);
                await StopInstancesAsync(ec2Client, allInstanceIds);

                return Ok("All instances in the subscription stopped successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("start-labeled-instances")]
        public async Task<IActionResult> StartLabeledEC2Instances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

                var labeledInstanceIds = await GetLabeledInstanceIdsAsync(ec2Client, "YourLabelKey", "YourLabelValue");
                await StartInstancesAsync(ec2Client, labeledInstanceIds);

                return Ok("Labeled instances started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("stop-labeled-instances")]
        public async Task<IActionResult> StopLabeledEC2Instances([FromBody] RequestModel requestModel)
        {
            try
            {
                var ec2Client = new AmazonEC2Client(requestModel.AWSAccessKey, requestModel.AWSSecretKey, RegionEndpoint.GetBySystemName(requestModel.Region));

                var labeledInstanceIds = await GetLabeledInstanceIdsAsync(ec2Client, "YourLabelKey", "YourLabelValue");
                await StopInstancesAsync(ec2Client, labeledInstanceIds);

                return Ok("Labeled instances stopped successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                return BadRequest(ex.Message);
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
