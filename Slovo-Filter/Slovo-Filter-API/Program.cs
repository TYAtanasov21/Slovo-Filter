using Slovo_Filter_DAL;
using Slovo_Filter_DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Ensure controllers are added
builder.Services.AddScoped<MessageRepository>(); // Your MessageRepository
builder.Services.AddScoped<dbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.MapControllers(); // Map your controller routes

app.Run();