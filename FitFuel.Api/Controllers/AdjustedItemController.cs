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
    public class AdjustedItemController : ControllerBase
    {
        private readonly FitFuelDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AdjustedItemController> _logger;

        public AdjustedItemController(FitFuelDbContext context, IMapper mapper, ILogger<AdjustedItemController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/AdjustedItem
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AdjustedItemDto>>>> GetAdjustedItems()
        {
            try
            {
                var adjustedItems = await _context.AdjustedItems
                    .Include(ai => ai.Item)
                    .ToListAsync();
                
                var adjustedItemDtos = _mapper.Map<IEnumerable<AdjustedItemDto>>(adjustedItems);
                return Ok(ApiResponse<IEnumerable<AdjustedItemDto>>.SuccessResponse(adjustedItemDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting adjusted items");
                return StatusCode(500, ApiResponse<IEnumerable<AdjustedItemDto>>.ErrorResponse("An error occurred while retrieving adjusted items."));
            }
        }

        // GET: api/AdjustedItem/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AdjustedItemDto>>> GetAdjustedItem(int id)
        {
            try
            {
                var adjustedItem = await _context.AdjustedItems
                    .Include(ai => ai.Item)
                    .FirstOrDefaultAsync(ai => ai.Id == id);

                if (adjustedItem == null)
                {
                    return NotFound(ApiResponse<AdjustedItemDto>.ErrorResponse($"Adjusted item with ID {id} not found"));
                }

                var adjustedItemDto = _mapper.Map<AdjustedItemDto>(adjustedItem);
                return Ok(ApiResponse<AdjustedItemDto>.SuccessResponse(adjustedItemDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting adjusted item {AdjustedItemId}", id);
                return StatusCode(500, ApiResponse<AdjustedItemDto>.ErrorResponse("An error occurred while retrieving the adjusted item."));
            }
        }

        // GET: api/AdjustedItem/item/5
        [HttpGet("item/{itemId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AdjustedItemDto>>>> GetAdjustedItemsByItemId(int itemId)
        {
            try
            {
                // Check if the item exists
                var itemExists = await _context.Items.AnyAsync(i => i.Id == itemId);
                if (!itemExists)
                {
                    return NotFound(ApiResponse<IEnumerable<AdjustedItemDto>>.ErrorResponse($"Item with ID {itemId} not found"));
                }

                var adjustedItems = await _context.AdjustedItems
                    .Include(ai => ai.Item)
                    .Where(ai => ai.ItemId == itemId)
                    .ToListAsync();

                var adjustedItemDtos = _mapper.Map<IEnumerable<AdjustedItemDto>>(adjustedItems);
                return Ok(ApiResponse<IEnumerable<AdjustedItemDto>>.SuccessResponse(adjustedItemDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting adjusted items for item {ItemId}", itemId);
                return StatusCode(500, ApiResponse<IEnumerable<AdjustedItemDto>>.ErrorResponse("An error occurred while retrieving adjusted items."));
            }
        }

        // POST: api/AdjustedItem
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AdjustedItemDto>>> CreateAdjustedItem(AdjustedItemCreateDto adjustedItemCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<AdjustedItemDto>.ErrorResponse("Invalid model state"));
                }

                // Check if the item exists
                var itemExists = await _context.Items.AnyAsync(i => i.Id == adjustedItemCreateDto.ItemId);
                if (!itemExists)
                {
                    return BadRequest(ApiResponse<AdjustedItemDto>.ErrorResponse($"Item with ID {adjustedItemCreateDto.ItemId} not found"));
                }

                // Check if the measurement already exists for this item
                var measurementExists = await _context.AdjustedItems
                    .AnyAsync(ai => ai.ItemId == adjustedItemCreateDto.ItemId && 
                                   ai.Measurement.ToLower() == adjustedItemCreateDto.Measurement.ToLower());
                if (measurementExists)
                {
                    return BadRequest(ApiResponse<AdjustedItemDto>.ErrorResponse($"Measurement '{adjustedItemCreateDto.Measurement}' already exists for this item"));
                }

                var adjustedItem = _mapper.Map<AdjustedItem>(adjustedItemCreateDto);
                
                _context.AdjustedItems.Add(adjustedItem);
                await _context.SaveChangesAsync();

                // Load the item for the response
                await _context.Entry(adjustedItem)
                    .Reference(ai => ai.Item)
                    .LoadAsync();

                var adjustedItemDto = _mapper.Map<AdjustedItemDto>(adjustedItem);

                return CreatedAtAction(nameof(GetAdjustedItem), new { id = adjustedItem.Id }, 
                    ApiResponse<AdjustedItemDto>.SuccessResponse(adjustedItemDto, "Adjusted item created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating adjusted item");
                return StatusCode(500, ApiResponse<AdjustedItemDto>.ErrorResponse("An error occurred while creating the adjusted item."));
            }
        }

        // PUT: api/AdjustedItem/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AdjustedItemDto>>> UpdateAdjustedItem(int id, AdjustedItemUpdateDto adjustedItemUpdateDto)
        {
            try
            {
                var adjustedItem = await _context.AdjustedItems.FindAsync(id);
                if (adjustedItem == null)
                {
                    return NotFound(ApiResponse<AdjustedItemDto>.ErrorResponse($"Adjusted item with ID {id} not found"));
                }

                // Check if the measurement already exists for this item (excluding the current one)
                var measurementExists = await _context.AdjustedItems
                    .AnyAsync(ai => ai.Id != id && 
                                   ai.ItemId == adjustedItem.ItemId && 
                                   ai.Measurement.ToLower() == adjustedItemUpdateDto.Measurement.ToLower());
                if (measurementExists)
                {
                    return BadRequest(ApiResponse<AdjustedItemDto>.ErrorResponse($"Measurement '{adjustedItemUpdateDto.Measurement}' already exists for this item"));
                }

                _mapper.Map(adjustedItemUpdateDto, adjustedItem);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdjustedItemExists(id))
                    {
                        return NotFound(ApiResponse<AdjustedItemDto>.ErrorResponse($"Adjusted item with ID {id} not found"));
                    }
                    else
                    {
                        throw;
                    }
                }

                // Load the item for the response
                await _context.Entry(adjustedItem)
                    .Reference(ai => ai.Item)
                    .LoadAsync();

                var adjustedItemDto = _mapper.Map<AdjustedItemDto>(adjustedItem);
                return Ok(ApiResponse<AdjustedItemDto>.SuccessResponse(adjustedItemDto, "Adjusted item updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating adjusted item {AdjustedItemId}", id);
                return StatusCode(500, ApiResponse<AdjustedItemDto>.ErrorResponse("An error occurred while updating the adjusted item."));
            }
        }

        // DELETE: api/AdjustedItem/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAdjustedItem(int id)
        {
            try
            {
                var adjustedItem = await _context.AdjustedItems.FindAsync(id);
                if (adjustedItem == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse($"Adjusted item with ID {id} not found"));
                }

                _context.AdjustedItems.Remove(adjustedItem);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Adjusted item deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting adjusted item {AdjustedItemId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting the adjusted item."));
            }
        }

        private bool AdjustedItemExists(int id)
        {
            return _context.AdjustedItems.Any(e => e.Id == id);
        }
    }
}