using FitFuel.Shared.Dtos;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FitFuel.Client.Services
{
    public class ItemTypeService
    {
        private readonly HttpClient _httpClient;

        public ItemTypeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ItemTypeDto>> GetItemTypesAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<ItemTypeDto>>>("api/ItemType");
            return response?.Data?.ToList() ?? new List<ItemTypeDto>();
        }

        public async Task<ItemTypeDto?> GetItemTypeAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<ItemTypeDto>>($"api/ItemType/{id}");
            return response?.Data;
        }

        public async Task<(bool success, string? message)> CreateItemTypeAsync(ItemTypeDto itemType)
        {
            var content = new StringContent(JsonSerializer.Serialize(itemType), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/ItemType", content);
            
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                // Try to extract the error message from the response
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ItemTypeDto>>();
                    return (false, errorResponse?.Message ?? "Failed to create item type");
                }
                catch
                {
                    return (false, "Failed to create item type. An error occurred.");
                }
            }
        }

        public async Task<(bool success, string? message)> UpdateItemTypeAsync(ItemTypeDto itemType)
        {
            var content = new StringContent(JsonSerializer.Serialize(itemType), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/ItemType/{itemType.Id}", content);
            
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                // Try to extract the error message from the response
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ItemTypeDto>>();
                    return (false, errorResponse?.Message ?? "Failed to update item type");
                }
                catch
                {
                    return (false, "Failed to update item type. An error occurred.");
                }
            }
        }

        public async Task<bool> DeleteItemTypeAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/ItemType/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            return false;
        }
    }
}