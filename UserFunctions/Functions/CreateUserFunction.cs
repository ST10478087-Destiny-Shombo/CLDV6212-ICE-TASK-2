using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using UserFunctions.Models;

namespace UserFunctions.Functions
{
    public class CreateUserFunction
    {
        private readonly IConfiguration _configuration;

        public CreateUserFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Function("CreateUser")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")] HttpRequestData req)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                User? user = JsonSerializer.Deserialize<User>(
                    requestBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (user == null)
                {
                    HttpResponseData badResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badResponse.WriteStringAsync("Invalid user information.");

                    return badResponse;
                }

                if (string.IsNullOrWhiteSpace(user.Name) ||
                    string.IsNullOrWhiteSpace(user.Email))
                {
                    HttpResponseData badResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badResponse.WriteStringAsync(
                        "Name and Email are required.");

                    return badResponse;
                }

                user.PartitionKey = "Users";

                if (string.IsNullOrWhiteSpace(user.RowKey))
                {
                    user.RowKey = Guid.NewGuid().ToString();
                }

                string connectionString =
                    _configuration["AzureWebJobsStorage"]!;

                TableServiceClient tableServiceClient =
                    new TableServiceClient(connectionString);

                TableClient tableClient =
                    tableServiceClient.GetTableClient("Users");

                await tableClient.CreateIfNotExistsAsync();

                await tableClient.AddEntityAsync(user);

                HttpResponseData response =
                    req.CreateResponse(HttpStatusCode.Created);

                await response.WriteAsJsonAsync(user);

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