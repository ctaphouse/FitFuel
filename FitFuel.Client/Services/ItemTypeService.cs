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

        public async Task<bool> CreateItemTypeAsync(ItemTypeDto itemType)
        {
            var content = new StringContent(JsonSerializer.Serialize(itemType), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/ItemType", content);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            return false;
        }

        public async Task<bool> UpdateItemTypeAsync(ItemTypeDto itemType)
        {
            var content = new StringContent(JsonSerializer.Serialize(itemType), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/ItemType/{itemType.Id}", content);
            
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            
            return false;
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