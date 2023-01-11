using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PinoyMassageService.Controllers.Services;
using PinoyMassageService.Extensions;
using PinoyMassageService.Repositories;
using PinoyMassageService.Settings;
using Swashbuckle.AspNetCore.Filters;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));


var mongoDbSettings = builder.Configuration.GetSection(nameof(MongoDBSettings)).Get<MongoDBSettings>();


builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{

    return new MongoClient(mongoDbSettings.ConnectionString);
    //return new MongoClient("mongodb://myroot:my-super-secret-password@localhost:31000/");            
});
builder.Services.AddSingleton<IAccountRepository, MongoDbAccountRepository>();
builder.Services.AddSingleton<IAddressRepository, MongoDbAddressRepository>();
builder.Services.AddSingleton<IProfileImageRepository, MongoDbProfileImageRepository>();
builder.Services.AddSingleton<IServiceRepository, MongoDbServiceRepository>();
builder.Services.AddSingleton<IUserRepository, MongoDbUserRepository>();
builder.Services.AddSingleton<IRefreshTokenRepository, MongoDbRefreshTokenRepository>();

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// we need this to start usingt the bearer authorization header
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.
                GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseEndpoints(endpoints =>
{
    //endpoints.MapControllers();    
    endpoints.MapGet("/hello", () => "Hello!");
    endpoints.MapGet("/hi", () => "Hi!");
    endpoints.MapGet("/TestTimeStamp", () => 
        {             
            DateTimeOffset now = DateTimeOffset.Now;

            long unixTimeStamp = now.ToUnixTimeSeconds();
            Debug.WriteLine($"TimeStampToData: {unixTimeStamp}");

            DateTimeOffset converted = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);            
            Debug.WriteLine($"TimeStampToDateTimeOffset: {converted}");

            Debug.WriteLine( $"ToLocalTime: {now.ToLocalTime()}");
            Debug.WriteLine($"ToUniversalTime: {now.ToUniversalTime()}");
            Debug.WriteLine($"ToFormatDataTime: {now.ToString("yyyy-MM-ddTHH:mm:ss")}");



            //double dateNowTimeStampOffset = DateTimeOffset.FromUnixTimeSeconds()
            return converted;
        }
    );
    endpoints.MapGet("/image", () =>
    {
        byte[] binaryContent = File.ReadAllBytes("D:\\2022\\gigs\\pinoy-massage-app\\images\\smiley.png");
        Debug.WriteLine($"binaryContent: {binaryContent}");
        return binaryContent;
    });
    endpoints.MapGet("/image2", () =>
    {
        byte[] binaryContent = File.ReadAllBytes("D:\\2022\\gigs\\pinoy-massage-app\\images\\smiley2.jpg");
        Debug.WriteLine($"binaryContent: {binaryContent}");
        return binaryContent;
    });

    endpoints.MapGet("/image3", () =>
    {
        byte[] binaryContent = File.ReadAllBytes("D:\\2022\\gigs\\pinoy-massage-app\\images\\smiley3.jpg");
        Debug.WriteLine($"binaryContent: {binaryContent}");
        return binaryContent;
    });

    endpoints.MapGet("/image4", () =>
    {
        byte[] binaryContent = File.ReadAllBytes("D:\\2022\\gigs\\pinoy-massage-app\\images\\smiley4.jpg");
        Debug.WriteLine($"binaryContent: {binaryContent}");
        return binaryContent;
    });
});


app.Run();


