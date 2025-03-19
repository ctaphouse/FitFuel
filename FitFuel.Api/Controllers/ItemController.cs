using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FitFuel.Api.Data;
using FitFuel.Api.Models;
using FitFuel.Shared.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitFuel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly FitFuelDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ItemController> _logger;

        public ItemController(FitFuelDbContext context, IMapper mapper, ILogger<ItemController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Item
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponseDto<ItemDto>>>> GetItems(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? itemTypeId = null,
            [FromQuery] string sortBy = "Name",
            [FromQuery] bool ascending = true)
        {
            try
            {
                var query = _context.Items
                    .Include(i => i.ItemType)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(i => i.Name.Contains(searchTerm));
                }

                if (itemTypeId.HasValue)
                {
                    query = query.Where(i => i.ItemTypeId == itemTypeId.Value);
                }

                // Apply sorting
                query = ApplySorting(query, sortBy, ascending);

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var itemDtos = _mapper.Map<List<ItemDto>>(items);

                // Create paginated response
                var paginatedResponse = new PaginatedResponseDto<ItemDto>
                {
                    Items = itemDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(ApiResponse<PaginatedResponseDto<ItemDto>>.SuccessResponse(paginatedResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items");
                return StatusCode(500, ApiResponse<PaginatedResponseDto<ItemDto>>.ErrorResponse("An error occurred while retrieving items."));
            }
        }

        // Helper method to apply sorting to query
        private IQueryable<Item> ApplySorting(IQueryable<Item> query, string sortBy, bool ascending)
        {
            // Default to sorting by Name if invalid property is provided
            return sortBy?.ToLower() switch
            {
                "name" => ascending 
                    ? query.OrderBy(i => i.Name) 
                    : query.OrderByDescending(i => i.Name),
                "calories" => ascending 
                    ? query.OrderBy(i => i.Calories) 
                    : query.OrderByDescending(i => i.Calories),
                "protein" => ascending 
                    ? query.OrderBy(i => i.Protein) 
                    : query.OrderByDescending(i => i.Protein),
                "carbohydrates" => ascending 
                    ? query.OrderBy(i => i.Carbohydrates) 
                    : query.OrderByDescending(i => i.Carbohydrates),
                "fat" => ascending 
                    ? query.OrderBy(i => i.Fat) 
                    : query.OrderByDescending(i => i.Fat),
                "fiber" => ascending 
                    ? query.OrderBy(i => i.Fiber) 
                    : query.OrderByDescending(i => i.Fiber),
                "sugars" => ascending 
                    ? query.OrderBy(i => i.Sugars) 
                    : query.OrderByDescending(i => i.Sugars),
                "itemtype" => ascending 
                    ? query.OrderBy(i => i.ItemType.Name) 
                    : query.OrderByDescending(i => i.ItemType.Name),
                _ => query.OrderBy(i => i.Name)
            };
        }

        // GET: api/Item/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ItemDto>>> GetItem(int id)
        {
            try
            {
                var item = await _context.Items
                    .Include(i => i.ItemType)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (item == null)
                {
                    return NotFound(ApiResponse<ItemDto>.ErrorResponse($"Item with ID {id} not found"));
                }

                var itemDto = _mapper.Map<ItemDto>(item);
                return Ok(ApiResponse<ItemDto>.SuccessResponse(itemDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item {ItemId}", id);
                return StatusCode(500, ApiResponse<ItemDto>.ErrorResponse("An error occurred while retrieving the item."));
            }
        }

        // POST: api/Item
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ItemDto>>> CreateItem(ItemCreateDto itemCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<ItemDto>.ErrorResponse("Invalid model state"));
                }

                // Verify that the item type exists
                if (!await _context.ItemTypes.AnyAsync(it => it.Id == itemCreateDto.ItemTypeId))
                {
                    return BadRequest(ApiResponse<ItemDto>.ErrorResponse($"ItemType with ID {itemCreateDto.ItemTypeId} not found"));
                }
                
                // Check if item with same name already exists
                var nameExists = await _context.Items.AnyAsync(i => i.Name.ToLower() == itemCreateDto.Name.ToLower());
                if (nameExists)
                {
                    return BadRequest(ApiResponse<ItemDto>.ErrorResponse($"An item with the name '{itemCreateDto.Name}' already exists"));
                }

                var item = _mapper.Map<Item>(itemCreateDto);
                
                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                // Load the item type for the response
                await _context.Entry(item)
                    .Reference(i => i.ItemType)
                    .LoadAsync();

                var itemDto = _mapper.Map<ItemDto>(item);

                return CreatedAtAction(nameof(GetItem), new { id = item.Id }, 
                    ApiResponse<ItemDto>.SuccessResponse(itemDto, "Item created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item");
                return StatusCode(500, ApiResponse<ItemDto>.ErrorResponse("An error occurred while creating the item."));
            }
        }

        // PUT: api/Item/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ItemDto>>> UpdateItem(int id, ItemUpdateDto itemUpdateDto)
        {
            try
            {
                // Verify that the item type exists
                if (!await _context.ItemTypes.AnyAsync(it => it.Id == itemUpdateDto.ItemTypeId))
                {
                    return BadRequest(ApiResponse<ItemDto>.ErrorResponse($"ItemType with ID {itemUpdateDto.ItemTypeId} not found"));
                }

                var item = await _context.Items.FindAsync(id);
                if (item == null)
                {
                    return NotFound(ApiResponse<ItemDto>.ErrorResponse($"Item with ID {id} not found"));
                }
                
                // Check if another item with the same name already exists
                var nameExists = await _context.Items.AnyAsync(i => 
                    i.Id != id && 
                    i.Name.ToLower() == itemUpdateDto.Name.ToLower());
                    
                if (nameExists)
                {
                    return BadRequest(ApiResponse<ItemDto>.ErrorResponse($"An item with the name '{itemUpdateDto.Name}' already exists"));
                }

                _mapper.Map(itemUpdateDto, item);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(id))
                    {
                        return NotFound(ApiResponse<ItemDto>.ErrorResponse($"Item with ID {id} not found"));
                    }
                    else
                    {
                        throw;
                    }
                }

                // Load the item type for the response
                await _context.Entry(item)
                    .Reference(i => i.ItemType)
                    .LoadAsync();

                var itemDto = _mapper.Map<ItemDto>(item);
                return Ok(ApiResponse<ItemDto>.SuccessResponse(itemDto, "Item updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {ItemId}", id);
                return StatusCode(500, ApiResponse<ItemDto>.ErrorResponse("An error occurred while updating the item."));
            }
        }

        // DELETE: api/Item/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteItem(int id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse($"Item with ID {id} not found"));
                }

                // Check if the item is used in any recipes
                var hasRecipeItems = await _context.RecipeItems.AnyAsync(ri => ri.ItemId == id);
                if (hasRecipeItems)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete an item that is used in recipes"));
                }

                // Check if the item has adjusted items
                var hasAdjustedItems = await _context.AdjustedItems.AnyAsync(ai => ai.ItemId == id);
                if (hasAdjustedItems)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete an item that has adjusted measurements"));
                }

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Item deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item {ItemId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting the item."));
            }
        }

        // GET: api/Item/types
        [HttpGet("types")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ItemTypeDto>>>> GetItemTypes()
        {
            try
            {
                var itemTypes = await _context.ItemTypes.ToListAsync();
                var itemTypeDtos = _mapper.Map<IEnumerable<ItemTypeDto>>(itemTypes);
                return Ok(ApiResponse<IEnumerable<ItemTypeDto>>.SuccessResponse(itemTypeDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item types");
                return StatusCode(500, ApiResponse<IEnumerable<ItemTypeDto>>.ErrorResponse("An error occurred while retrieving item types."));
            }
        }

        private bool ItemExists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}