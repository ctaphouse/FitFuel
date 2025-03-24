using FitFuel.Shared.Dtos;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FitFuel.Client.Services
{
    public class RecipeService
    {
        private readonly HttpClient _httpClient;

        public RecipeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedResponseDto<RecipeDto>> GetRecipesAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null)
        {
            var queryString = $"api/Recipe?pageNumber={pageNumber}&pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            }

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PaginatedResponseDto<RecipeDto>>>(queryString);
            return response?.Data ?? new PaginatedResponseDto<RecipeDto>();
        }

        public async Task<RecipeDto?> GetRecipeAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<RecipeDto>>($"api/Recipe/{id}");
            return response?.Data;
        }

        // In FitFuel.Client/Services/RecipeService.cs
        // Replace the CreateRecipeAsync method with this improved version:

        public async Task<(bool success, int? id, string? message)> CreateRecipeAsync(RecipeCreateDto recipe)
        {
            var content = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Recipe", content);

            if (response.IsSuccessStatusCode)
            {
                // Parse the response to get the created recipe ID
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RecipeDto>>();
                if (apiResponse?.Data != null)
                {
                    return (true, apiResponse.Data.Id, null);
                }
                return (true, null, null);
            }
            else
            {
                // Try to extract the error message from the response
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RecipeDto>>();
                    return (false, null, errorResponse?.Message ?? "Failed to create recipe");
                }
                catch
                {
                    return (false, null, "Failed to create recipe. An error occurred.");
                }
            }
        }

        public async Task<(bool success, string? message)> UpdateRecipeAsync(int id, RecipeUpdateDto recipe)
        {
            var content = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/Recipe/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                // Try to extract the error message from the response
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RecipeDto>>();
                    return (false, errorResponse?.Message ?? "Failed to update recipe");
                }
                catch
                {
                    return (false, "Failed to update recipe. An error occurred.");
                }
            }
        }

        public async Task<bool> DeleteRecipeAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Recipe/{id}");

            return response.IsSuccessStatusCode;
        }

        public async Task<List<RecipeItemDto>> GetRecipeItemsAsync(int recipeId)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<RecipeItemDto>>>($"api/Recipe/{recipeId}/items");
            return response?.Data?.ToList() ?? new List<RecipeItemDto>();
        }
    }
}