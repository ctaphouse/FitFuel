// FitFuel.Client/Services/GroceryListService.cs
using FitFuel.Shared.Dtos;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FitFuel.Client.Services
{
    public class GroceryListService
    {
        private readonly HttpClient _httpClient;

        public GroceryListService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<GroceryListDto>> GetGroceryListsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<GroceryListDto>>>("api/GroceryList");
            return response?.Data?.ToList() ?? new List<GroceryListDto>();
        }

        public async Task<GroceryListDto?> GetGroceryListAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<GroceryListDto>>($"api/GroceryList/{id}");
            return response?.Data;
        }

        public async Task<(bool success, int? id, string? message)> CreateGroceryListAsync(GroceryListCreateDto groceryList)
        {
            var content = new StringContent(JsonSerializer.Serialize(groceryList), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/GroceryList", content);

            if (response.IsSuccessStatusCode)
            {
                var createdResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GroceryListDto>>();
                if (createdResponse?.Data != null)
                {
                    return (true, createdResponse.Data.Id, null);
                }
                return (true, null, null);
            }
            else
            {
                // Try to extract the error message from the response
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GroceryListDto>>();
                    return (false, null, errorResponse?.Message ?? "Failed to create grocery list");
                }
                catch
                {
                    return (false, null, "Failed to create grocery list. An error occurred.");
                }
            }
        }

        public async Task<bool> DeleteGroceryListAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/GroceryList/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ToggleGroceryListItemAsync(int listId, int itemId)
        {
            var response = await _httpClient.PatchAsync($"api/GroceryList/{listId}/items/{itemId}/check", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool success, string? message)> UpdateGroceryListAsync(int id, GroceryListCreateDto groceryList)
        {
            var content = new StringContent(JsonSerializer.Serialize(groceryList), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/GroceryList/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                // Try to extract the error message from the response
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GroceryListDto>>();
                    return (false, errorResponse?.Message ?? "Failed to update grocery list");
                }
                catch
                {
                    return (false, "Failed to update grocery list. An error occurred.");
                }
            }
        }
    }
}