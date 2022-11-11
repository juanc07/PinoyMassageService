using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PinoyMassageService.Repositories;
using PinoyMassageService.Settings;

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
builder.Services.AddSingleton<IAccountsRepository, MongoDbAccountRepository>();

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.UseEndpoints(endpoints =>
{
    //endpoints.MapControllers();    
    endpoints.MapGet("/hello", () => "Hello!");
    endpoints.MapGet("/hi", () => "Hi!");
});


app.Run();


