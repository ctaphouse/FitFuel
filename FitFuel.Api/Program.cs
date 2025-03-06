using Microsoft.EntityFrameworkCore;
using FitFuel.Api.Data;
using FitFuel.Api.Profiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<FitFuelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("FitFuelConnection")));

builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS (if needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Dependency injection example
// Replace YourService and IYourService with actual services
// builder.Services.AddScoped<IYourService, YourServiceImplementation>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
