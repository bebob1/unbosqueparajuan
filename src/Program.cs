WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register HttpClient factory for Recaptcha service
builder.Services.AddHttpClient();

// Register Recaptcha service
builder.Services.AddScoped<UnBosqueParaJuan.Controllers.Recaptcha>();

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
