using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FitFuel.Api.Data;
using FitFuel.Api.Models;
using FitFuel.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitFuel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroceryListController : ControllerBase
    {
        private readonly FitFuelDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GroceryListController> _logger;

        public GroceryListController(FitFuelDbContext context, IMapper mapper, ILogger<GroceryListController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/GroceryList
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GroceryListDto>>>> GetGroceryLists()
        {
            try
            {
                var groceryLists = await _context.GroceryLists
                    .OrderByDescending(gl => gl.CreatedDate)
                    .ToListAsync();
                
                var groceryListDtos = new List<GroceryListDto>();
                
                foreach (var groceryList in groceryLists)
                {
                    var groceryListDto = new GroceryListDto
                    {
                        Id = groceryList.Id,
                        Name = groceryList.Name,
                        CreatedDate = groceryList.CreatedDate,
                        Items = await GetGroceryListItemsDto(groceryList.Id)
                    };
                    
                    groceryListDtos.Add(groceryListDto);
                }
                
                return Ok(ApiResponse<IEnumerable<GroceryListDto>>.SuccessResponse(groceryListDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grocery lists");
                return StatusCode(500, ApiResponse<IEnumerable<GroceryListDto>>.ErrorResponse("An error occurred while retrieving grocery lists."));
            }
        }

        // GET: api/GroceryList/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GroceryListDto>>> GetGroceryList(int id)
        {
            try
            {
                var groceryList = await _context.GroceryLists.FindAsync(id);

                if (groceryList == null)
                {
                    return NotFound(ApiResponse<GroceryListDto>.ErrorResponse($"Grocery list with ID {id} not found"));
                }

                var groceryListDto = new GroceryListDto
                {
                    Id = groceryList.Id,
                    Name = groceryList.Name,
                    CreatedDate = groceryList.CreatedDate,
                    Items = await GetGroceryListItemsDto(groceryList.Id)
                };
                
                return Ok(ApiResponse<GroceryListDto>.SuccessResponse(groceryListDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grocery list {GroceryListId}", id);
                return StatusCode(500, ApiResponse<GroceryListDto>.ErrorResponse("An error occurred while retrieving the grocery list."));
            }
        }

        // POST: api/GroceryList
        [HttpPost]
        public async Task<ActionResult<ApiResponse<GroceryListDto>>> CreateGroceryList(GroceryListCreateDto groceryListCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<GroceryListDto>.ErrorResponse("Invalid model state"));
                }

                // Create the grocery list
                var groceryList = new GroceryList
                {
                    Name = groceryListCreateDto.Name,
                    CreatedDate = DateTime.UtcNow
                };
                
                _context.GroceryLists.Add(groceryList);
                await _context.SaveChangesAsync();
                
                // Add items from selected recipes
                if (groceryListCreateDto.RecipeIds.Any())
                {
                    await AddRecipeItemsToGroceryList(groceryList.Id, groceryListCreateDto.RecipeIds);
                }
                
                // Add individual items
                if (groceryListCreateDto.IndividualItems.Any())
                {
                    await AddIndividualItemsToGroceryList(groceryList.Id, groceryListCreateDto.IndividualItems);
                }
                
                // Fetch the complete grocery list with items
                var groceryListDto = new GroceryListDto
                {
                    Id = groceryList.Id,
                    Name = groceryList.Name,
                    CreatedDate = groceryList.CreatedDate,
                    Items = await GetGroceryListItemsDto(groceryList.Id)
                };

                return CreatedAtAction(nameof(GetGroceryList), new { id = groceryList.Id }, 
                    ApiResponse<GroceryListDto>.SuccessResponse(groceryListDto, "Grocery list created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating grocery list");
                return StatusCode(500, ApiResponse<GroceryListDto>.ErrorResponse("An error occurred while creating the grocery list."));
            }
        }

        // DELETE: api/GroceryList/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteGroceryList(int id)
        {
            try
            {
                var groceryList = await _context.GroceryLists.FindAsync(id);
                if (groceryList == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse($"Grocery list with ID {id} not found"));
                }

                // Delete associated items first (should cascade but being explicit)
                var groceryListItems = await _context.GroceryListItems.Where(gli => gli.GroceryListId == id).ToListAsync();
                _context.GroceryListItems.RemoveRange(groceryListItems);
                
                // Then delete the list
                _context.GroceryLists.Remove(groceryList);
                
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Grocery list deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting grocery list {GroceryListId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting the grocery list."));
            }
        }
        
        // PATCH: api/GroceryList/5/items/6/check
        [HttpPatch("{listId}/items/{itemId}/check")]
        public async Task<ActionResult<ApiResponse<bool>>> ToggleGroceryListItem(int listId, int itemId)
        {
            try
            {
                var groceryListItem = await _context.GroceryListItems
                    .FirstOrDefaultAsync(gli => gli.GroceryListId == listId && gli.Id == itemId);
                    
                if (groceryListItem == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse($"Grocery list item with ID {itemId} not found in list {listId}"));
                }
                
                // Toggle the checked state
                groceryListItem.IsChecked = !groceryListItem.IsChecked;
                
                await _context.SaveChangesAsync();
                
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Grocery list item updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating grocery list item {ItemId} in list {ListId}", itemId, listId);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while updating the grocery list item."));
            }
        }
        
        // Helper methods
        private async Task<List<GroceryListItemDto>> GetGroceryListItemsDto(int groceryListId)
        {
            var groceryListItems = await _context.GroceryListItems
                .Include(gli => gli.Item)
                    .ThenInclude(i => i.ItemType)
                .Where(gli => gli.GroceryListId == groceryListId)
                .ToListAsync();
                
            var groceryListItemDtos = new List<GroceryListItemDto>();
            
            foreach (var item in groceryListItems)
            {
                var groceryListItemDto = new GroceryListItemDto
                {
                    ItemId = item.ItemId,
                    ItemName = item.Item.Name,
                    ItemTypeId = item.Item.ItemTypeId,
                    ItemTypeName = item.Item.ItemType.Name,
                    Quantity = item.Quantity,
                    Measurement = item.Measurement,
                    IsChecked = item.IsChecked,
                    Source = item.Source,
                    SourceId = item.SourceId
                };
                
                // Get source name if it's a recipe
                if (item.Source == "Recipe" && item.SourceId.HasValue)
                {
                    var recipe = await _context.Recipes.FindAsync(item.SourceId.Value);
                    if (recipe != null)
                    {
                        groceryListItemDto.SourceName = recipe.Name;
                    }
                }
                
                groceryListItemDtos.Add(groceryListItemDto);
            }
            
            // Sort by item type, then by item name
            return groceryListItemDtos
                .OrderBy(i => i.ItemTypeName)
                .ThenBy(i => i.ItemName)
                .ToList();
        }
        
        private async Task AddRecipeItemsToGroceryList(int groceryListId, List<int> recipeIds)
        {
            // Get all recipe items for the selected recipes
            var recipeItems = await _context.RecipeItems
                .Include(ri => ri.Item)
                .Where(ri => recipeIds.Contains(ri.RecipeId))
                .ToListAsync();
                
            // Group by item to combine quantities
            var groupedItems = recipeItems
                .GroupBy(ri => ri.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    TotalGrams = g.Sum(ri => ri.GramEquivalent),
                    RecipeIds = g.Select(ri => ri.RecipeId).Distinct().ToList()
                })
                .ToList();
                
            // Add each item to the grocery list
            foreach (var groupedItem in groupedItems)
            {
                // For simplicity, we'll use grams as the measurement
                var groceryListItem = new GroceryListItem
                {
                    GroceryListId = groceryListId,
                    ItemId = groupedItem.ItemId,
                    Quantity = groupedItem.TotalGrams,
                    Measurement = "grams",
                    IsChecked = false,
                    Source = "Recipe",
                    SourceId = groupedItem.RecipeIds.First() // Just use the first recipe as the source
                };
                
                _context.GroceryListItems.Add(groceryListItem);
            }
            
            await _context.SaveChangesAsync();
        }
        
        private async Task AddIndividualItemsToGroceryList(int groceryListId, List<IndividualItemDto> individualItems)
        {
            foreach (var individualItem in individualItems)
            {
                var groceryListItem = new GroceryListItem
                {
                    GroceryListId = groceryListId,
                    ItemId = individualItem.ItemId,
                    Quantity = individualItem.Quantity,
                    Measurement = individualItem.Measurement,
                    IsChecked = false,
                    Source = "Individual"
                };
                
                _context.GroceryListItems.Add(groceryListItem);
            }
            
            await _context.SaveChangesAsync();
        }
    }
}