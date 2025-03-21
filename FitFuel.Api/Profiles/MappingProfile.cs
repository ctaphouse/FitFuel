// Add to FitFuel.Api/Profiles/MappingProfile.cs

using AutoMapper;
using FitFuel.Api.Models;
using FitFuel.Shared.Dtos;

namespace FitFuel.Api.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Item mappings
            CreateMap<Item, ItemDto>()
                .ForMember(dest => dest.ItemTypeName, opt => opt.MapFrom(src => src.ItemType.Name));
            CreateMap<ItemCreateDto, Item>();
            CreateMap<ItemUpdateDto, Item>();
            
            // ItemType mappings
            CreateMap<ItemType, ItemTypeDto>().ReverseMap();
            
            // Recipe mappings
            CreateMap<Recipe, RecipeDto>();
            CreateMap<RecipeCreateDto, Recipe>();
            CreateMap<RecipeUpdateDto, Recipe>();
            
            // RecipeItem mappings
            CreateMap<RecipeItem, RecipeItemDto>()
                .ForMember(dest => dest.RecipeName, opt => opt.MapFrom(src => src.Recipe.Name))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item.Name))
                .ForMember(dest => dest.Calories, opt => opt.Ignore())
                .ForMember(dest => dest.Protein, opt => opt.Ignore())
                .ForMember(dest => dest.Carbohydrates, opt => opt.Ignore())
                .ForMember(dest => dest.Fiber, opt => opt.Ignore())
                .ForMember(dest => dest.Sugars, opt => opt.Ignore())
                .ForMember(dest => dest.Fat, opt => opt.Ignore());
            CreateMap<RecipeItemCreateDto, RecipeItem>();
            CreateMap<RecipeItemUpdateDto, RecipeItem>();
            
            // AdjustedItem mappings
            CreateMap<AdjustedItem, AdjustedItemDto>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item.Name));
            CreateMap<AdjustedItemCreateDto, AdjustedItem>();
            CreateMap<AdjustedItemUpdateDto, AdjustedItem>();
            
            // AdjustedRecipe mappings
            CreateMap<AdjustedRecipe, AdjustedRecipeDto>()
                .ForMember(dest => dest.RecipeName, opt => opt.MapFrom(src => src.Recipe.Name))
                .ForMember(dest => dest.TotalCalories, opt => opt.Ignore())
                .ForMember(dest => dest.TotalProtein, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCarbohydrates, opt => opt.Ignore())
                .ForMember(dest => dest.TotalFiber, opt => opt.Ignore())
                .ForMember(dest => dest.TotalSugars, opt => opt.Ignore())
                .ForMember(dest => dest.TotalFat, opt => opt.Ignore())
                .ForMember(dest => dest.CaloriesPerServing, opt => opt.Ignore())
                .ForMember(dest => dest.ProteinPerServing, opt => opt.Ignore())
                .ForMember(dest => dest.CarbohydratesPerServing, opt => opt.Ignore())
                .ForMember(dest => dest.FiberPerServing, opt => opt.Ignore())
                .ForMember(dest => dest.SugarsPerServing, opt => opt.Ignore())
                .ForMember(dest => dest.FatPerServing, opt => opt.Ignore());
            CreateMap<AdjustedRecipeCreateDto, AdjustedRecipe>();
            CreateMap<AdjustedRecipeUpdateDto, AdjustedRecipe>();
            
            // GroceryList mappings
            CreateMap<GroceryList, GroceryListDto>()
                .ForMember(dest => dest.Items, opt => opt.Ignore()); // Items need to be loaded separately
            
            // Other mappings for GroceryListItem if needed - we're handling these manually in the controller
        }
    }
}