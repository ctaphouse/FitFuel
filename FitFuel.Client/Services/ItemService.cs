using FitFuel.Shared.Dtos;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FitFuel.Client.Services
{
    public class ItemService
    {
        private readonly HttpClient _httpClient;

        public ItemService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaginatedResponseDto<ItemDto>> GetItemsAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            string? searchTerm = null, 
            int? itemTypeId = null,
            string sortBy = "Name",
            bool ascending = true)
        {
            var queryString = $"api/Item?pageNumber={pageNumber}&pageSize={pageSize}";
            
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            }
            
            if (itemTypeId.HasValue && itemTypeId.Value > 0)
            {
                queryString += $"&itemTypeId={itemTypeId.Value}";
            }
            
            queryString += $"&sortBy={Uri.EscapeDataString(sortBy)}&ascending={ascending}";
            
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PaginatedResponseDto<ItemDto>>>(queryString);
            return response?.Data ?? new PaginatedResponseDto<ItemDto>();
        }

        public async Task<ItemDto?> GetItemAsync(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<ItemDto>>($"api/Item/{id}");
            return response?.Data;
        }

        public async Task<(bool success, string? message)> CreateItemAsync(ItemCreateDto item)
        {
            var content = new StringContent(JsonSerializer.Serialize(item), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Item", content);
            
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                // Try to extract the error message from the response
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ItemDto>>();
                    return (false, errorResponse?.Message ?? "Failed to create item");
                }
                catch
                {
                    return (false, "Failed to create item. An error occurred.");
                }
            }
        }

        public async Task<(bool success, string? message)> UpdateItemAsync(int id, ItemUpdateDto item)
        {
            var content = new StringContent(JsonSerializer.Serialize(item), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/Item/{id}", content);
            
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                // Try to extract the error message from the response
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ItemDto>>();
                    return (false, errorResponse?.Message ?? "Failed to update item");
                }
                catch
                {
                    return (false, "Failed to update item. An error occurred.");
                }
            }
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Item/{id}");
            
            return response.IsSuccessStatusCode;
        }
    }
}