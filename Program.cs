using backend.common.data;
using backend.Extensions;
using backend.hubs.chat;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi(builder.Configuration);

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await DatabaseSeeder.SeedAsync(app.Services);

app.MapHub<ChatHub>("/hub/chat");

app.Run();


