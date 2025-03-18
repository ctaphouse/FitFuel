using FitFuel.Shared.Dtos;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FitFuel.Client.Services
{
    public class AdjustedRecipeService
    {
        private readonly HttpClient _httpClient;

        public AdjustedRecipeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<AdjustedRecipeDto>> GetAdjustedRecipesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<AdjustedRecipeDto>>>("api/AdjustedRecipe");
            return response?.Data?.ToList() ?? new List<AdjustedRecipeDto>();
        }

        public async Task<AdjustedRecipeDto?> GetAdjustedRecipeAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<AdjustedRecipeDto>>($"api/AdjustedRecipe/{id}");
            return response?.Data;
        }

        public async Task<List<AdjustedRecipeDto>> GetAdjustedRecipesByRecipeIdAsync(int recipeId)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<AdjustedRecipeDto>>>($"api/AdjustedRecipe/recipe/{recipeId}");
            return response?.Data?.ToList() ?? new List<AdjustedRecipeDto>();
        }

        public async Task<bool> CreateAdjustedRecipeAsync(AdjustedRecipeCreateDto adjustedRecipe)
        {
            var content = new StringContent(JsonSerializer.Serialize(adjustedRecipe), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/AdjustedRecipe", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAdjustedRecipeAsync(int id, AdjustedRecipeUpdateDto adjustedRecipe)
        {
            var content = new StringContent(JsonSerializer.Serialize(adjustedRecipe), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/AdjustedRecipe/{id}", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAdjustedRecipeAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/AdjustedRecipe/{id}");
            
            return response.IsSuccessStatusCode;
        }
    }
}