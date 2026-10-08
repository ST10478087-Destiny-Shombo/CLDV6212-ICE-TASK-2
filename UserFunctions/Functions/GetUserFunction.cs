using System.Net;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using UserFunctions.Models;

namespace UserFunctions.Functions
{
    public class GetUserFunction
    {
        private readonly IConfiguration _configuration;

        public GetUserFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("GetUser")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "users/{id}")]
            HttpRequestData req,
            string id)
        {
            try
            {
                string connectionString =
                    _configuration["AzureWebJobsStorage"]!;

                TableServiceClient tableServiceClient =
                    new TableServiceClient(connectionString);

                TableClient tableClient =
                    tableServiceClient.GetTableClient("Users");

                User user =
                    await tableClient.GetEntityAsync<User>("Users", id);

                HttpResponseData response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(user);

                return response;
            }
            catch (Azure.RequestFailedException ex)
                when (ex.Status == 404)
            {
                HttpResponseData response =
                    req.CreateResponse(HttpStatusCode.NotFound);

                await response.WriteStringAsync(
                    "User with ID " + id + " was not found.");

                return response;
            }
            catch (Exception ex)
            {
                HttpResponseData response =
                    req.CreateResponse(HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(ex.Message);

                return response;
            }
        }
    }
}