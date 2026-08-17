var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.QuotesApi>("quotesapi");
builder.Build().Run();
