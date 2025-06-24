using Product_API.Common;
using Product_API.Common.DTO;
using System.Text.Json;

namespace Product_API.Application.Services
{
    public class CategoryHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoryHttpClientService> _logger;

        public CategoryHttpClientService(HttpClient httpClient, ILogger<CategoryHttpClientService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDTO>?> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Catalog");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<CategoryDTO>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return apiResponse?.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error getting categories from Catalog API");
                return null;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while getting categories.");
                return null;
            }
        }
    }
} 