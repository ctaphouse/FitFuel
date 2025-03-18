using FitFuel.Shared.Dtos;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FitFuel.Client.Services
{
    public class RecipeItemService
    {
        private readonly HttpClient _httpClient;

        public RecipeItemService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RecipeItemDto?> GetRecipeItemAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<RecipeItemDto>>($"api/RecipeItem/{id}");
            return response?.Data;
        }

        public async Task<bool> CreateRecipeItemAsync(RecipeItemCreateDto recipeItem)
        {
            var content = new StringContent(JsonSerializer.Serialize(recipeItem), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/RecipeItem", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateRecipeItemAsync(int id, RecipeItemUpdateDto recipeItem)
        {
            var content = new StringContent(JsonSerializer.Serialize(recipeItem), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/RecipeItem/{id}", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRecipeItemAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/RecipeItem/{id}");
            
            return response.IsSuccessStatusCode;
        }
    }
}