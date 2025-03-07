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
    public class RecipeItemController : ControllerBase
    {
        private readonly FitFuelDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RecipeItemController> _logger;

        public RecipeItemController(FitFuelDbContext context, IMapper mapper, ILogger<RecipeItemController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/RecipeItem/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RecipeItemDto>>> GetRecipeItem(int id)
        {
            try
            {
                var recipeItem = await _context.RecipeItems
                    .Include(ri => ri.Item)
                    .Include(ri => ri.Recipe)
                    .FirstOrDefaultAsync(ri => ri.Id == id);

                if (recipeItem == null)
                {
                    return NotFound(ApiResponse<RecipeItemDto>.ErrorResponse($"Recipe item with ID {id} not found"));
                }

                var recipeItemDto = _mapper.Map<RecipeItemDto>(recipeItem);
                
                // Calculate nutritional information
                CalculateRecipeItemNutrition(recipeItemDto, recipeItem.Item);

                return Ok(ApiResponse<RecipeItemDto>.SuccessResponse(recipeItemDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recipe item {RecipeItemId}", id);
                return StatusCode(500, ApiResponse<RecipeItemDto>.ErrorResponse("An error occurred while retrieving the recipe item."));
            }
        }

        // POST: api/RecipeItem
        [HttpPost]
        public async Task<ActionResult<ApiResponse<RecipeItemDto>>> CreateRecipeItem(RecipeItemCreateDto recipeItemCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<RecipeItemDto>.ErrorResponse("Invalid model state"));
                }

                // Verify that the recipe exists
                var recipe = await _context.Recipes.FindAsync(recipeItemCreateDto.RecipeId);
                if (recipe == null)
                {
                    return BadRequest(ApiResponse<RecipeItemDto>.ErrorResponse($"Recipe with ID {recipeItemCreateDto.RecipeId} not found"));
                }

                // Verify that the item exists
                var item = await _context.Items.FindAsync(recipeItemCreateDto.ItemId);
                if (item == null)
                {
                    return BadRequest(ApiResponse<RecipeItemDto>.ErrorResponse($"Item with ID {recipeItemCreateDto.ItemId} not found"));
                }

                var recipeItem = _mapper.Map<RecipeItem>(recipeItemCreateDto);
                
                _context.RecipeItems.Add(recipeItem);
                await _context.SaveChangesAsync();

                // Load the related entities for the response
                await _context.Entry(recipeItem)
                    .Reference(ri => ri.Recipe)
                    .LoadAsync();
                await _context.Entry(recipeItem)
                    .Reference(ri => ri.Item)
                    .LoadAsync();

                var recipeItemDto = _mapper.Map<RecipeItemDto>(recipeItem);
                
                // Calculate nutritional information
                CalculateRecipeItemNutrition(recipeItemDto, item);

                return CreatedAtAction(nameof(GetRecipeItem), new { id = recipeItem.Id }, 
                    ApiResponse<RecipeItemDto>.SuccessResponse(recipeItemDto, "Recipe item created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recipe item");
                return StatusCode(500, ApiResponse<RecipeItemDto>.ErrorResponse("An error occurred while creating the recipe item."));
            }
        }

        // PUT: api/RecipeItem/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<RecipeItemDto>>> UpdateRecipeItem(int id, RecipeItemUpdateDto recipeItemUpdateDto)
        {
            try
            {
                var recipeItem = await _context.RecipeItems
                    .Include(ri => ri.Item)
                    .Include(ri => ri.Recipe)
                    .FirstOrDefaultAsync(ri => ri.Id == id);
                
                if (recipeItem == null)
                {
                    return NotFound(ApiResponse<RecipeItemDto>.ErrorResponse($"Recipe item with ID {id} not found"));
                }

                _mapper.Map(recipeItemUpdateDto, recipeItem);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeItemExists(id))
                    {
                        return NotFound(ApiResponse<RecipeItemDto>.ErrorResponse($"Recipe item with ID {id} not found"));
                    }
                    else
                    {
                        throw;
                    }
                }

                var recipeItemDto = _mapper.Map<RecipeItemDto>(recipeItem);
                
                // Calculate nutritional information
                CalculateRecipeItemNutrition(recipeItemDto, recipeItem.Item);

                return Ok(ApiResponse<RecipeItemDto>.SuccessResponse(recipeItemDto, "Recipe item updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recipe item {RecipeItemId}", id);
                return StatusCode(500, ApiResponse<RecipeItemDto>.ErrorResponse("An error occurred while updating the recipe item."));
            }
        }

        // DELETE: api/RecipeItem/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRecipeItem(int id)
        {
            try
            {
                var recipeItem = await _context.RecipeItems.FindAsync(id);
                if (recipeItem == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse($"Recipe item with ID {id} not found"));
                }

                _context.RecipeItems.Remove(recipeItem);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Recipe item deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recipe item {RecipeItemId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting the recipe item."));
            }
        }

        // Helper method to calculate nutritional information for a recipe item
        private void CalculateRecipeItemNutrition(RecipeItemDto recipeItemDto, Item item)
        {
            var ratio = recipeItemDto.GramEquivalent / 100; // Nutritional values are per 100g

            recipeItemDto.Calories = (int)(item.Calories * ratio);
            recipeItemDto.Protein = item.Protein * ratio;
            recipeItemDto.Carbohydrates = item.Carbohydrates * ratio;
            recipeItemDto.Fiber = item.Fiber * ratio;
            recipeItemDto.Sugars = item.Sugars * ratio;
            recipeItemDto.Fat = item.Fat * ratio;
        }

        private bool RecipeItemExists(int id)
        {
            return _context.RecipeItems.Any(e => e.Id == id);
        }
    }
}