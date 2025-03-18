using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FitFuel.Client;
using FitFuel.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure the API client
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:5001/") });

// Register services
builder.Services.AddScoped<ItemTypeService>();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<AdjustedItemService>();
builder.Services.AddScoped<RecipeService>();
builder.Services.AddScoped<RecipeItemService>();
builder.Services.AddScoped<AdjustedRecipeService>();
builder.Services.AddScoped<IDialogService, DialogService>();

builder.Services.AddSingleton<AppState>();

await builder.Build().RunAsync();
