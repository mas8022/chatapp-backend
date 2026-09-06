using backend.common.data;
using backend.Extensions;
using backend.hubs.chat;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

////////////////////////////////////////////////////////////
var connectionString =
    builder.Configuration.GetConnectionString("DB_URL")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DB_URL is missing.");

var sqlInfo = new SqlConnectionStringBuilder(connectionString);

Console.WriteLine(
    "SQL configuration: Server={Server}, Database={Database}, User={User}",
    sqlInfo.DataSource,
    sqlInfo.InitialCatalog,
    sqlInfo.UserID);
////////////////////////////////////////////////////////////



















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


