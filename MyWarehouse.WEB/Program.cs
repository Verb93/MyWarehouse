using Microsoft.EntityFrameworkCore;
using MyWarehouse.Common.ProfileData;
using MyWarehouse.Common.Security;
using MyWarehouse.Common.Security.SecurityInterface;
using MyWarehouse.Data;
using MyWarehouse.Data.Models;
using MyWarehouse.Interfaces.RepositoryInterfaces;
using MyWarehouse.Interfaces.ServiceInterfaces;
using MyWarehouse.Repositories;
using MyWarehouse.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ConnessioneDB")
    ?? throw new InvalidOperationException("Connection string"
        + "'ConnessioneDB' not found.");

builder.Services.AddDbContext<WarehouseContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAutoMapper(typeof(MappingProfile));

// Registra il repository specifico
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISupplierUserRepository, SupplierUserRepository>();

// Registra il servizio specifico, passando sia l'entità che il DTO
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IPasswordService<Users>, PasswordService<Users>>();

//JWT
var jwtSettingsSection = builder.Configuration.GetSection("JwtSetting");
builder.Services.Configure<JwtSetting>(jwtSettingsSection);

var jwtSetting = jwtSettingsSection.Get<JwtSetting>();
var key = Encoding.ASCII.GetBytes(jwtSetting.Key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSetting.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSetting.Audience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddControllers();

/*builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromHours(1);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});
*/
builder.Services.AddHttpContextAccessor();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
                        policy =>
                        {
                            policy.WithOrigins("http://localhost:4200")
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                        });    
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
//app.UseSession();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();


