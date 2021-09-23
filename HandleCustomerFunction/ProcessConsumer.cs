using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using HandleCustomerFunction.Models;
using CustomerCore.Data;
using System.Collections.Generic;
using System.Linq;

namespace HandleCustomerFunction
{
    public class ProcessConsumer
    {
        private readonly ILogger<ProcessConsumer> _log;
        private readonly IBaseRepository _baseRepository;

        public ProcessConsumer(ILogger<ProcessConsumer> log, IBaseRepository baseRepository)
        {
            _log = log;
            _baseRepository = baseRepository;
        }

        [FunctionName("ProcessConsumer")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            _log.LogInformation("Function was triggered");

            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var connectionString = config["CustomerDemoConnection"];

            try
            {
                if (HttpMethods.IsGet(req.Method))
                {
                    return await ProcessGet(req, connectionString);   
                }
                else if (HttpMethods.IsPost(req.Method))
                {
                    return await ProcessPost(req, connectionString);
                }
                else
                {
                    return new UnprocessableEntityObjectResult(new { message = "This function doesn't support this request."});
                }
            }
            catch(Exception ex)
            {
                _log.LogError($"Exception at function run: {ex.Message} - StackTrace is: { ex }", ex.Message, ex);
                return new BadRequestObjectResult(new { message = "Could not retrieve customers" });
            }
        }

        private async Task<IActionResult> ProcessGet(HttpRequest req, string connectionString)
        {
            string id = req.Query["id"];

            dynamic result;

            if(String.IsNullOrEmpty(id))
            {
                result = await GetCustomers(connectionString);
            }
            else
            {
                result = await GetCustomer(connectionString, id);
            }

            if (result == null)
            {
                return new NotFoundObjectResult(new { message = "Could not retrieve customers" });
            }

            return new OkObjectResult(result);
        }

        private async Task<IActionResult> ProcessPost(HttpRequest req, string connectionString)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Customer customer = JsonConvert.DeserializeObject<Customer>(requestBody);

            bool success = await InsertCustomer(connectionString, customer);

            if (success)
            {
                return new OkObjectResult(customer);
            }
            else
            {
                return new BadRequestObjectResult(new { message = $"Could not insert customer {customer.Name}" });
            }
        }

        private async Task<bool> InsertCustomer(string connectionString, Customer customer)
        {
            try
            {
                return await _baseRepository.Execute(connectionString,
                    "INSERT INTO Customer(Name, Address, Phone, Country) VALUES (@Name, @Address, @Phone, @Country)", customer);
            }
            catch (Exception ex)
            {
                _log.LogError($"Exception at InsertCustomer: {ex.Message} - StackTrace is: { ex }", ex.Message, ex);
                return false;
            }
        }

        private async Task<IEnumerable<Customer>> GetCustomers(string connectionString)
        {
            try
            {
                return await _baseRepository.QueryAll<Customer>(connectionString, 
                    "SELECT Id, Name, Address, Phone, Country FROM Customer");
            }
            catch(Exception ex)
            {
                _log.LogError($"Exception at GetCustomers: {ex.Message} - StackTrace is: { ex }", ex.Message, ex);
                return null;
            }
        }

        private async Task<Customer> GetCustomer(string connectionString, string id)
        {
            try
            {
                return await _baseRepository.QuerySingleOrDefault<Customer>(connectionString,
                    "SELECT Id, Name, Address, Phone, Country FROM Customer WHERE Id = @Id;", new { Id = id });
            }
            catch (Exception ex)
            {
                _log.LogError($"Exception at GetCustomer: {ex.Message} - StackTrace is: { ex }", ex.Message, ex);
                return null;
            }
        }
    }
}
