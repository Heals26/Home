Run
dotnet tool install --global dotnet-ef --version {with the appropriate efcore version}

To Run the Migration
dotnet ef migrations add [Explanation] --project Home.Persistence --context PersistenceContext --startup-project Home.WebApi