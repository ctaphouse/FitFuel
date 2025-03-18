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

        public async Task<bool> CreateRecipeAsync(RecipeCreateDto recipe)
        {
            var content = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Recipe", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateRecipeAsync(int id, RecipeUpdateDto recipe)
        {
            var content = new StringContent(JsonSerializer.Serialize(recipe), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/Recipe/{id}", content);
            
            return response.IsSuccessStatusCode;
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