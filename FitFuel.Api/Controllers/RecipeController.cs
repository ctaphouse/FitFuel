using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FitFuel.Api.Data;
using FitFuel.Api.Models;
using FitFuel.Shared.Dtos;

namespace FitFuel.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : ControllerBase
    {
        private readonly FitFuelDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RecipeController> _logger;

        public RecipeController(FitFuelDbContext context, IMapper mapper, ILogger<RecipeController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Recipe
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponseDto<RecipeDto>>>> GetRecipes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var query = _context.Recipes.AsQueryable();

                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(r => r.Name.Contains(searchTerm));
                }

                // Always order by name
                query = query.OrderBy(r => r.Name);

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var recipes = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Map to DTOs
                var recipeDtos = _mapper.Map<List<RecipeDto>>(recipes);

                // Calculate nutritional information for each recipe
                foreach (var recipeDto in recipeDtos)
                {
                    await CalculateRecipeNutrition(recipeDto);
                }

                // Create paginated response
                var paginatedResponse = new PaginatedResponseDto<RecipeDto>
                {
                    Items = recipeDtos,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                return Ok(ApiResponse<PaginatedResponseDto<RecipeDto>>.SuccessResponse(paginatedResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recipes");
                return StatusCode(500, ApiResponse<PaginatedResponseDto<RecipeDto>>.ErrorResponse("An error occurred while retrieving recipes."));
            }
        }

        // GET: api/Recipe/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RecipeDto>>> GetRecipe(int id)
        {
            try
            {
                var recipe = await _context.Recipes
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (recipe == null)
                {
                    return NotFound(ApiResponse<RecipeDto>.ErrorResponse($"Recipe with ID {id} not found"));
                }

                var recipeDto = _mapper.Map<RecipeDto>(recipe);
                
                // Calculate nutritional information
                await CalculateRecipeNutrition(recipeDto);

                return Ok(ApiResponse<RecipeDto>.SuccessResponse(recipeDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recipe {RecipeId}", id);
                return StatusCode(500, ApiResponse<RecipeDto>.ErrorResponse("An error occurred while retrieving the recipe."));
            }
        }

        // POST: api/Recipe
        [HttpPost]
        public async Task<ActionResult<ApiResponse<RecipeDto>>> CreateRecipe(RecipeCreateDto recipeCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<RecipeDto>.ErrorResponse("Invalid model state"));
                }
                
                // Check if recipe with same name already exists
                var nameExists = await _context.Recipes.AnyAsync(r => r.Name.ToLower() == recipeCreateDto.Name.ToLower());
                if (nameExists)
                {
                    return BadRequest(ApiResponse<RecipeDto>.ErrorResponse($"A recipe with the name '{recipeCreateDto.Name}' already exists"));
                }

                var recipe = _mapper.Map<Recipe>(recipeCreateDto);
                
                _context.Recipes.Add(recipe);
                await _context.SaveChangesAsync();

                var recipeDto = _mapper.Map<RecipeDto>(recipe);
                
                // Initialize nutritional information to zero since there are no items yet
                recipeDto.TotalCalories = 0;
                recipeDto.TotalProtein = 0;
                recipeDto.TotalCarbohydrates = 0;
                recipeDto.TotalFiber = 0;
                recipeDto.TotalSugars = 0;
                recipeDto.TotalFat = 0;
                recipeDto.CaloriesPerServing = 0;
                recipeDto.ProteinPerServing = 0;
                recipeDto.CarbohydratesPerServing = 0;
                recipeDto.FiberPerServing = 0;
                recipeDto.SugarsPerServing = 0;
                recipeDto.FatPerServing = 0;

                return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, 
                    ApiResponse<RecipeDto>.SuccessResponse(recipeDto, "Recipe created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recipe");
                return StatusCode(500, ApiResponse<RecipeDto>.ErrorResponse("An error occurred while creating the recipe."));
            }
        }

        // PUT: api/Recipe/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<RecipeDto>>> UpdateRecipe(int id, RecipeUpdateDto recipeUpdateDto)
        {
            try
            {
                var recipe = await _context.Recipes.FindAsync(id);
                if (recipe == null)
                {
                    return NotFound(ApiResponse<RecipeDto>.ErrorResponse($"Recipe with ID {id} not found"));
                }
                
                // Check if another recipe with the same name already exists
                var nameExists = await _context.Recipes.AnyAsync(r => 
                    r.Id != id && 
                    r.Name.ToLower() == recipeUpdateDto.Name.ToLower());
                    
                if (nameExists)
                {
                    return BadRequest(ApiResponse<RecipeDto>.ErrorResponse($"A recipe with the name '{recipeUpdateDto.Name}' already exists"));
                }

                _mapper.Map(recipeUpdateDto, recipe);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeExists(id))
                    {
                        return NotFound(ApiResponse<RecipeDto>.ErrorResponse($"Recipe with ID {id} not found"));
                    }
                    else
                    {
                        throw;
                    }
                }

                var recipeDto = _mapper.Map<RecipeDto>(recipe);
                
                // Calculate updated nutritional information
                await CalculateRecipeNutrition(recipeDto);

                return Ok(ApiResponse<RecipeDto>.SuccessResponse(recipeDto, "Recipe updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recipe {RecipeId}", id);
                return StatusCode(500, ApiResponse<RecipeDto>.ErrorResponse("An error occurred while updating the recipe."));
            }
        }

        // DELETE: api/Recipe/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRecipe(int id)
        {
            try
            {
                var recipe = await _context.Recipes.FindAsync(id);
                if (recipe == null)
                {
                    return NotFound(ApiResponse<bool>.ErrorResponse($"Recipe with ID {id} not found"));
                }

                // Check if the recipe has recipe items
                var hasRecipeItems = await _context.RecipeItems.AnyAsync(ri => ri.RecipeId == id);
                if (hasRecipeItems)
                {
                    // Delete all associated recipe items first
                    var recipeItems = await _context.RecipeItems.Where(ri => ri.RecipeId == id).ToListAsync();
                    _context.RecipeItems.RemoveRange(recipeItems);
                }

                // Check if there are adjusted recipes
                var hasAdjustedRecipes = await _context.AdjustedRecipes.AnyAsync(ar => ar.RecipeId == id);
                if (hasAdjustedRecipes)
                {
                    // Delete all associated adjusted recipes
                    var adjustedRecipes = await _context.AdjustedRecipes.Where(ar => ar.RecipeId == id).ToListAsync();
                    _context.AdjustedRecipes.RemoveRange(adjustedRecipes);
                }

                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Recipe deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recipe {RecipeId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("An error occurred while deleting the recipe."));
            }
        }

        // GET: api/Recipe/5/items
        [HttpGet("{id}/items")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RecipeItemDto>>>> GetRecipeItems(int id)
        {
            try
            {
                var recipeExists = await _context.Recipes.AnyAsync(r => r.Id == id);
                if (!recipeExists)
                {
                    return NotFound(ApiResponse<IEnumerable<RecipeItemDto>>.ErrorResponse($"Recipe with ID {id} not found"));
                }

                var recipeItems = await _context.RecipeItems
                    .Include(ri => ri.Item)
                    .Include(ri => ri.Recipe)
                    .Where(ri => ri.RecipeId == id)
                    .ToListAsync();

                var recipeItemDtos = _mapper.Map<IEnumerable<RecipeItemDto>>(recipeItems);
                
                // Calculate nutritional information for each recipe item
                foreach (var recipeItemDto in recipeItemDtos)
                {
                    CalculateRecipeItemNutrition(recipeItemDto, recipeItems.First(ri => ri.Id == recipeItemDto.Id).Item);
                }

                return Ok(ApiResponse<IEnumerable<RecipeItemDto>>.SuccessResponse(recipeItemDtos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recipe items for recipe {RecipeId}", id);
                return StatusCode(500, ApiResponse<IEnumerable<RecipeItemDto>>.ErrorResponse("An error occurred while retrieving recipe items."));
            }
        }

        // Helper method to calculate nutritional information for a recipe
        private async Task CalculateRecipeNutrition(RecipeDto recipeDto)
        {
            var recipeItems = await _context.RecipeItems
                .Include(ri => ri.Item)
                .Where(ri => ri.RecipeId == recipeDto.Id)
                .ToListAsync();

            // Initialize totals
            recipeDto.TotalCalories = 0;
            recipeDto.TotalProtein = 0;
            recipeDto.TotalCarbohydrates = 0;
            recipeDto.TotalFiber = 0;
            recipeDto.TotalSugars = 0;
            recipeDto.TotalFat = 0;

            // Calculate totals
            foreach (var recipeItem in recipeItems)
            {
                var item = recipeItem.Item;
                var ratio = recipeItem.GramEquivalent / 100; // Nutritional values are per 100g

                recipeDto.TotalCalories += (int)(item.Calories * ratio);
                recipeDto.TotalProtein += item.Protein * ratio;
                recipeDto.TotalCarbohydrates += item.Carbohydrates * ratio;
                recipeDto.TotalFiber += item.Fiber * ratio;
                recipeDto.TotalSugars += item.Sugars * ratio;
                recipeDto.TotalFat += item.Fat * ratio;
            }

            // Calculate per serving
            if (recipeDto.Servings > 0)
            {
                recipeDto.CaloriesPerServing = recipeDto.TotalCalories / recipeDto.Servings;
                recipeDto.ProteinPerServing = recipeDto.TotalProtein / recipeDto.Servings;
                recipeDto.CarbohydratesPerServing = recipeDto.TotalCarbohydrates / recipeDto.Servings;
                recipeDto.FiberPerServing = recipeDto.TotalFiber / recipeDto.Servings;
                recipeDto.SugarsPerServing = recipeDto.TotalSugars / recipeDto.Servings;
                recipeDto.FatPerServing = recipeDto.TotalFat / recipeDto.Servings;
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

        private bool RecipeExists(int id)
        {
            return _context.Recipes.Any(e => e.Id == id);
        }
    }
}