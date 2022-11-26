using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PinoyMassageService.Repositories;
using PinoyMassageService.Settings;
using System.Diagnostics;

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


