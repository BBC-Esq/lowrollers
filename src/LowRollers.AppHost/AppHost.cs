var builder = DistributedApplication.CreateBuilder(args);

// Add Redis for session state and real-time caching
var redis = builder.AddRedis("redis");

// Add SQL Server for game state and hand history
// Note: Uses SA for local dev. For production, use dedicated app user with least privileges.
var sqlServer = builder.AddSqlServer("sqlserver")
    .AddDatabase("lowrollers");

// Add the API project
var api = builder.AddProject<Projects.LowRollers_Api>("api")
    .WithReference(redis)
    .WithReference(sqlServer)
    .WithExternalHttpEndpoints();

// Add the Angular frontend
var web = builder.AddJavaScriptApp("web", "../LowRollers.Web")
    .WithNpm(install: true)
    .WithRunScript("start")
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(targetPort: 4200)
    .WithExternalHttpEndpoints();

builder.Build().Run();
