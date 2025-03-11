using FitFuel.Shared.Dtos;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FitFuel.Client.Services
{
    public class AdjustedItemService
    {
        private readonly HttpClient _httpClient;

        public AdjustedItemService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<AdjustedItemDto>> GetAdjustedItemsAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<AdjustedItemDto>>>("api/AdjustedItem");
            return response?.Data?.ToList() ?? new List<AdjustedItemDto>();
        }

        public async Task<AdjustedItemDto?> GetAdjustedItemAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<AdjustedItemDto>>($"api/AdjustedItem/{id}");
            return response?.Data;
        }

        public async Task<List<AdjustedItemDto>> GetAdjustedItemsByItemIdAsync(int itemId)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<AdjustedItemDto>>>($"api/AdjustedItem/item/{itemId}");
            return response?.Data?.ToList() ?? new List<AdjustedItemDto>();
        }

        public async Task<bool> CreateAdjustedItemAsync(AdjustedItemCreateDto adjustedItem)
        {
            var content = new StringContent(JsonSerializer.Serialize(adjustedItem), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/AdjustedItem", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAdjustedItemAsync(int id, AdjustedItemUpdateDto adjustedItem)
        {
            var content = new StringContent(JsonSerializer.Serialize(adjustedItem), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/AdjustedItem/{id}", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAdjustedItemAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/AdjustedItem/{id}");
            
            return response.IsSuccessStatusCode;
        }
    }
}