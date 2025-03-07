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
    public class AdjustedRecipeController : ControllerBase
    {
        private readonly FitFuelDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AdjustedRecipeController> _logger;

        public AdjustedRecipeController(FitFuelDbContext context, IMapper mapper, ILogger<AdjustedRecipeController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/AdjustedRecipe
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AdjustedRecipeDto>>>> GetAdjustedRecipes()
        {
            try
            {
                var adjustedRecipes = await _context.AdjustedRecipes
                    .Include(ar => ar.Recipe)
                    .ToListAsync();
                
                var adjustedRecipeDtos = _mapper.Map<IEnumerable<AdjustedRecipeDto>>(adjustedRecipes);
                
                // Calculate nutritional information for each adjusted recipe
                foreach (var adjustedRecipeDto in adjustedRecipeDtos)
                {
                    await CalculateAdjustedRecipeNutrition(adjustedRecipeDto);
                }

                return Ok(ApiResponse<IEnumerable<AdjustedRecipeDto>>.SuccessResponse(adjustedRecipeDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting adjusted recipes");
                return StatusCode(500, ApiResponse<IEnumerable<AdjustedRecipeDto>>.ErrorResponse("An error occurred while retrieving adjusted recipes."));
            }
        }

        // GET: api/AdjustedRecipe/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AdjustedRecipeDto>>> GetAdjustedRecipe(int id)
        {
            try
            {
                var adjustedRecipe = await _context.AdjustedRecipes
                    .Include(ar => ar.Recipe)
                    .FirstOrDefaultAsync(ar => ar.Id == id);

                if (adjustedRecipe == null)
                {
                    return NotFound(ApiResponse<AdjustedRecipeDto>.ErrorResponse($"Adjusted recipe with ID {id} not found"));
                }

                var adjustedRecipeDto = _mapper.Map<AdjustedRecipeDto>(adjustedRecipe);
                
                // Calculate nutritional information
                await CalculateAdjustedRecipeNutrition(adjustedRecipeDto);

                return Ok(ApiResponse<AdjustedRecipeDto>.SuccessResponse(adjustedRecipeDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting adjusted recipe {AdjustedRecipeId}", id);
                return StatusCode(500, ApiResponse<AdjustedRecipeDto>.ErrorResponse("An error occurred while retrieving the adjusted recipe."));
            }
        }

        // GET: api/AdjustedRecipe/recipe/5
        [HttpGet("recipe/{recipeId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AdjustedRecipeDto>>>> GetAdjustedRecipesByRecipeId(int recipeId)
        {
            try
            {
                // Check if the recipe exists
                var recipeExists = await _context.Recipes.AnyAsync(r => r.Id == recipeId);
                if (!recipeExists)
                {
                    return NotFound(ApiResponse<IEnumerable<AdjustedRecipeDto>>.ErrorResponse($"Recipe with ID {recipeId} not found"));
                }

                var adjustedRecipes = await _context.AdjustedRecipes
                    .Include(ar => ar.Recipe)
                    .Where(ar => ar.RecipeId == recipeId)
                    .ToListAsync();

                var adjustedRecipeDtos = _mapper.Map<IEnumerable<AdjustedRecipeDto>>(adjustedRecipes);
                
                // Calculate nutritional information for each adjusted recipe
                foreach (var adjustedRecipeDto in adjustedRecipeDtos)
                {
                    await CalculateAdjustedRecipeNutrition(adjustedRecipeDto);
                }

                return Ok(ApiResponse<IEnumerable<AdjustedRecipeDto>>.SuccessResponse(adjustedRecipeDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting adjusted recipes for recipe {RecipeId}", recipeId);
                return StatusCode(500, ApiResponse<IEnumerable<AdjustedRecipeDto>>.ErrorResponse("An error occurred while retrieving adjusted recipes."));
            }
        }

        // POST: api/AdjustedRecipe
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AdjustedRecipeDto>>> CreateAdjustedRecipe(AdjustedRecipeCreateDto adjustedRecipeCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<AdjustedRecipeDto>.ErrorResponse("Invalid model state"));
                }

                // Check if the recipe exists
                var recipe = await _context.Recipes.FindAsync(adjustedRecipeCreateDto.RecipeId);
                if (recipe == null)
                {
                    return BadRequest(ApiResponse<AdjustedRecipeDto>.ErrorResponse($"Recipe with ID {adjustedRecipeCreateDto.RecipeId} not found"));
                }

                // Check if the measurement already exists for this recipe
                var measurementExists = await _context.AdjustedRecipes
                    .AnyAsync(ar => ar.RecipeId == adjustedRecipeCreateDto.RecipeId && 
                                   ar.Measurement.ToLower() == adjustedRecipeCreateDto.Measurement.ToLower());
                if (measurementExists)
                {
                    return BadRequest(ApiResponse<AdjustedRecipeDto>.ErrorResponse($"Measurement '{adjustedRecipeCreateDto.Measurement}' already exists for this recipe"));
                }

                var adjustedRecipe = _mapper.Map<AdjustedRecipe>(adjustedRecipeCreateDto);
                
                _context.AdjustedRecipes.Add(adjustedRecipe);
                await _context.SaveChangesAsync();

                // Load the recipe for the response
                await _context.Entry(adjustedRecipe)
                    .Reference(ar => ar.Recipe)
                    .LoadAsync();

                var adjustedRecipeDto = _mapper.Map<AdjustedRecipeDto>(adjustedRecipe);
                
                // Calculate nutritional information
                await CalculateAdjustedRecipeNutrition(adjustedRecipeDto);

                return CreatedAtAction(nameof(GetAdjustedRecipe), new { id = adjustedRecipe.Id }, 
                    ApiResponse<AdjustedRecipeDto>.SuccessResponse(adjustedRecipeDto, "Adjusted recipe created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating adjusted recipe");
                return StatusCode(500, ApiResponse<AdjustedRecipeDto>.ErrorResponse("An error occurred while creating the adjusted recipe."));
            }
        }

        // PUT: api/AdjustedRecipe/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AdjustedRecipeDto>>> UpdateAdjustedRecipe(int id, AdjustedRecipeUpdateDto adjustedRecipeUpdateDto)
        {
            try
            {
                var adjustedRecipe = await _context.AdjustedRecipes
                    .Include(ar => ar.Recipe)
                    .FirstOrDefaultAsync(ar => ar.Id == id);
                
                if (adjustedRecipe == null)
                {
                    return NotFound(ApiResponse<AdjustedRecipeDto>.ErrorResponse($"Adjusted recipe with ID {id} not found"));
                }

                // Check if the measurement already exists for this recipe (excluding the current one)
                var measurementExists = await _context.AdjustedRecipes
                    .AnyAsync(ar => ar.Id != id && 
                                   ar.RecipeId == adjustedRecipe.RecipeId && 
                                   ar.Measurement.ToLower() == adjustedRecipeUpdateDto.Measurement.ToLower());
                if (measurementExists)
                {
                    return BadRequest(ApiResponse<AdjustedRecipeDto>.ErrorResponse($"Measurement '{adjustedRecipeUpdateDto.Measurement}' already exists for this recipe"));
                }

                _mapper.Map(adjustedRecipeUpdateDto, adjustedRecipe);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdjustedRecipeExists(id))
                    {
                        return NotFound(ApiResponse<AdjustedRecipeDto>.ErrorResponse($"Adjusted recipe with ID {id} not found"));
                    }
                    else
                    {
                        throw;
                    }
                }

                var adjustedRecipeDto = _mapper.Map<AdjustedRecipeDto>(adjustedRecipe);
                
                // Calculate nutritional information
                await CalculateAdjustedRecipeNutrition(adjustedRecipeDto);

                return Ok(ApiResponse<AdjustedRecipeDto>.SuccessResponse(adjustedRecipeDto, "Adjusted recipe updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating adjusted recipe {AdjustedRecipeId}", id);
                return StatusCode(500, ApiResponse<AdjustedRecipeDto>.ErrorResponse("An error occurred while updating the adjusted recipe."));
            }
        }

        // DELETE: api/AdjustedRecipe/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAdjustedRecipe(int id)
        {
            try
            {
                var adjustedRecipe = await _context.AdjustedRecipes.FindAsync(id);
                if (adjustedRecipe == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse($"Adjusted recipe with ID {id} not found"));
                }

                _context.AdjustedRecipes.Remove(adjustedRecipe);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Adjusted recipe deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting adjusted recipe {AdjustedRecipeId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting the adjusted recipe."));
            }
        }

        // Helper method to calculate nutritional information for an adjusted recipe
        private async Task CalculateAdjustedRecipeNutrition(AdjustedRecipeDto adjustedRecipeDto)
        {
            // Get the recipe items to calculate nutrition
            var recipeItems = await _context.RecipeItems
                .Include(ri => ri.Item)
                .Where(ri => ri.RecipeId == adjustedRecipeDto.RecipeId)
                .ToListAsync();
            
            // Get the original recipe
            var recipe = await _context.Recipes.FindAsync(adjustedRecipeDto.RecipeId);
            
            if (recipe == null || !recipeItems.Any())
            {
                // If no recipe or no items, set nutritional values to zero
                adjustedRecipeDto.TotalCalories = 0;
                adjustedRecipeDto.TotalProtein = 0;
                adjustedRecipeDto.TotalCarbohydrates = 0;
                adjustedRecipeDto.TotalFiber = 0;
                adjustedRecipeDto.TotalSugars = 0;
                adjustedRecipeDto.TotalFat = 0;
                adjustedRecipeDto.CaloriesPerServing = 0;
                adjustedRecipeDto.ProteinPerServing = 0;
                adjustedRecipeDto.CarbohydratesPerServing = 0;
                adjustedRecipeDto.FiberPerServing = 0;
                adjustedRecipeDto.SugarsPerServing = 0;
                adjustedRecipeDto.FatPerServing = 0;
                return;
            }
            
            // Calculate original recipe totals
            int originalCalories = 0;
            decimal originalProtein = 0;
            decimal originalCarbohydrates = 0;
            decimal originalFiber = 0;
            decimal originalSugars = 0;
            decimal originalFat = 0;
            
            foreach (var recipeItem in recipeItems)
            {
                var item = recipeItem.Item;
                var ratio = recipeItem.GramEquivalent / 100; // Nutritional values are per 100g
                
                originalCalories += (int)(item.Calories * ratio);
                originalProtein += item.Protein * ratio;
                originalCarbohydrates += item.Carbohydrates * ratio;
                originalFiber += item.Fiber * ratio;
                originalSugars += item.Sugars * ratio;
                originalFat += item.Fat * ratio;
            }
            
            // Calculate the scaling factor based on servings
            // Example: If original recipe is 4 servings and adjusted is 6 servings,
            // the scaling factor is 6/4 = 1.5x the nutrition
            var servingRatio = (decimal)adjustedRecipeDto.Servings / recipe.Servings;
            
            // Apply the scaling factor to get adjusted totals
            adjustedRecipeDto.TotalCalories = (int)(originalCalories * servingRatio);
            adjustedRecipeDto.TotalProtein = originalProtein * servingRatio;
            adjustedRecipeDto.TotalCarbohydrates = originalCarbohydrates * servingRatio;
            adjustedRecipeDto.TotalFiber = originalFiber * servingRatio;
            adjustedRecipeDto.TotalSugars = originalSugars * servingRatio;
            adjustedRecipeDto.TotalFat = originalFat * servingRatio;
            
            // Calculate per serving values
            if (adjustedRecipeDto.Servings > 0)
            {
                adjustedRecipeDto.CaloriesPerServing = adjustedRecipeDto.TotalCalories / adjustedRecipeDto.Servings;
                adjustedRecipeDto.ProteinPerServing = adjustedRecipeDto.TotalProtein / adjustedRecipeDto.Servings;
                adjustedRecipeDto.CarbohydratesPerServing = adjustedRecipeDto.TotalCarbohydrates / adjustedRecipeDto.Servings;
                adjustedRecipeDto.FiberPerServing = adjustedRecipeDto.TotalFiber / adjustedRecipeDto.Servings;
                adjustedRecipeDto.SugarsPerServing = adjustedRecipeDto.TotalSugars / adjustedRecipeDto.Servings;
                adjustedRecipeDto.FatPerServing = adjustedRecipeDto.TotalFat / adjustedRecipeDto.Servings;
            }
        }

        private bool AdjustedRecipeExists(int id)
        {
            return _context.AdjustedRecipes.Any(e => e.Id == id);
        }
    }
}