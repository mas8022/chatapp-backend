dotnet ef dbcontext scaffold "Name=ConnectionStrings:DB_URL" Microsoft.EntityFrameworkCore.SqlServer -o model --no-onconfiguring -c AppDbContext --force
