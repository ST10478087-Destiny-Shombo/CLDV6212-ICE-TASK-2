using System.Net;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using UserFunctions.Models;

namespace UserFunctions.Functions
{
    public class GetAllUsersFunction
    {
        private readonly IConfiguration _configuration;

        public GetAllUsersFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("GetAllUsers")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "users")]
            HttpRequestData req)
        {
            try
            {
                string connectionString =
                    _configuration["AzureWebJobsStorage"]!;

                TableServiceClient tableServiceClient =
                    new TableServiceClient(connectionString);

                TableClient tableClient =
                    tableServiceClient.GetTableClient("Users");

                List<User> users = new List<User>();

                await foreach (User user in tableClient.QueryAsync<User>())
                {
                    users.Add(user);
                }

                HttpResponseData response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(users);

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