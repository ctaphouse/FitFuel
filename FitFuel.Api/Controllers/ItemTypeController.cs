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
    public class ItemTypeController : ControllerBase
    {
        private readonly FitFuelDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ItemTypeController> _logger;

        public ItemTypeController(FitFuelDbContext context, IMapper mapper, ILogger<ItemTypeController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/ItemType
        [HttpGet]
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

        // GET: api/ItemType/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ItemTypeDto>>> GetItemType(int id)
        {
            try
            {
                var itemType = await _context.ItemTypes.FindAsync(id);

                if (itemType == null)
                {
                    return NotFound(ApiResponse<ItemTypeDto>.ErrorResponse($"ItemType with ID {id} not found"));
                }

                var itemTypeDto = _mapper.Map<ItemTypeDto>(itemType);
                return Ok(ApiResponse<ItemTypeDto>.SuccessResponse(itemTypeDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item type {ItemTypeId}", id);
                return StatusCode(500, ApiResponse<ItemTypeDto>.ErrorResponse("An error occurred while retrieving the item type."));
            }
        }

        // POST: api/ItemType
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ItemTypeDto>>> CreateItemType(ItemTypeDto itemTypeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<ItemTypeDto>.ErrorResponse("Invalid model state"));
                }

                // Check if an item type with the same name already exists
                var nameExists = await _context.ItemTypes.AnyAsync(it => it.Name.ToLower() == itemTypeDto.Name.ToLower());
                if (nameExists)
                {
                    return BadRequest(ApiResponse<ItemTypeDto>.ErrorResponse($"An item type with the name '{itemTypeDto.Name}' already exists"));
                }

                var itemType = _mapper.Map<ItemType>(itemTypeDto);
                
                _context.ItemTypes.Add(itemType);
                await _context.SaveChangesAsync();

                itemTypeDto = _mapper.Map<ItemTypeDto>(itemType);

                return CreatedAtAction(nameof(GetItemType), new { id = itemType.Id }, 
                    ApiResponse<ItemTypeDto>.SuccessResponse(itemTypeDto, "Item type created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item type");
                return StatusCode(500, ApiResponse<ItemTypeDto>.ErrorResponse("An error occurred while creating the item type."));
            }
        }

        // PUT: api/ItemType/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ItemTypeDto>>> UpdateItemType(int id, ItemTypeDto itemTypeDto)
        {
            try
            {
                if (id != itemTypeDto.Id)
                {
                    return BadRequest(ApiResponse<ItemTypeDto>.ErrorResponse("ID mismatch"));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<ItemTypeDto>.ErrorResponse("Invalid model state"));
                }

                var itemType = await _context.ItemTypes.FindAsync(id);
                if (itemType == null)
                {
                    return NotFound(ApiResponse<ItemTypeDto>.ErrorResponse($"ItemType with ID {id} not found"));
                }

                // Check if another item type with the same name already exists
                var nameExists = await _context.ItemTypes.AnyAsync(it => 
                    it.Id != id && 
                    it.Name.ToLower() == itemTypeDto.Name.ToLower());
                    
                if (nameExists)
                {
                    return BadRequest(ApiResponse<ItemTypeDto>.ErrorResponse($"An item type with the name '{itemTypeDto.Name}' already exists"));
                }

                _mapper.Map(itemTypeDto, itemType);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemTypeExists(id))
                    {
                        return NotFound(ApiResponse<ItemTypeDto>.ErrorResponse($"ItemType with ID {id} not found"));
                    }
                    else
                    {
                        throw;
                    }
                }

                var updatedItemTypeDto = _mapper.Map<ItemTypeDto>(itemType);
                return Ok(ApiResponse<ItemTypeDto>.SuccessResponse(updatedItemTypeDto, "Item type updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item type {ItemTypeId}", id);
                return StatusCode(500, ApiResponse<ItemTypeDto>.ErrorResponse("An error occurred while updating the item type."));
            }
        }

        // DELETE: api/ItemType/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteItemType(int id)
        {
            try
            {
                var itemType = await _context.ItemTypes.FindAsync(id);
                if (itemType == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse($"ItemType with ID {id} not found"));
                }

                // Check if there are any items using this item type
                var hasRelatedItems = await _context.Items.AnyAsync(i => i.ItemTypeId == id);
                if (hasRelatedItems)
                {
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Cannot delete item type that is in use"));
                }

                _context.ItemTypes.Remove(itemType);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Item type deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item type {ItemTypeId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting the item type."));
            }
        }

        private bool ItemTypeExists(int id)
        {
            return _context.ItemTypes.Any(e => e.Id == id);
        }
    }
}