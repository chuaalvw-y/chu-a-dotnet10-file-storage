using ChuA.FileStorage.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddChuAFileStorage(builder.Configuration);

var app = builder.Build();

app.MapControllers();
app.Run();
