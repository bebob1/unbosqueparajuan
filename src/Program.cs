using Microsoft.AspNetCore.HttpLogging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add HTTP logging to see all requests
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestPath | 
                           HttpLoggingFields.RequestMethod | 
                           HttpLoggingFields.ResponseStatusCode;
});

// RECAPTCHA DISABLED - Register HttpClient factory for Recaptcha service
// RECAPTCHA DISABLED - builder.Services.AddHttpClient();

// RECAPTCHA DISABLED - Register Recaptcha service
// RECAPTCHA DISABLED - builder.Services.AddScoped<UnBosqueParaJuan.Controllers.Recaptcha>();

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== APPLICATION STARTING ===");

await app.BootUmbracoAsync();

logger.LogInformation("=== UMBRACO BOOTED ===");

// Add HTTP logging middleware
app.UseHttpLogging();

// Add custom logging middleware to track all requests
app.Use(async (context, next) =>
{
    logger.LogInformation(">>> INCOMING REQUEST: {Method} {Path}", 
        context.Request.Method, 
        context.Request.Path);
    
    // Log POST request details
    if (context.Request.Method == "POST")
    {
        logger.LogInformation("POST Request Details:");
        logger.LogInformation("  ContentType: {ContentType}", context.Request.ContentType);
        logger.LogInformation("  HasFormContentType: {HasForm}", context.Request.HasFormContentType);
        
        // REMOVED: Reading form here was consuming the stream and causing 400 errors
        // The form will be read by the controller instead
    }
    
    await next();
    
    logger.LogInformation("<<< RESPONSE: {StatusCode} for {Method} {Path}", 
        context.Response.StatusCode,
        context.Request.Method, 
        context.Request.Path);
});

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        logger.LogInformation("=== CONFIGURING UMBRACO MIDDLEWARE ===");
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        logger.LogInformation("=== CONFIGURING UMBRACO ENDPOINTS ===");
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

logger.LogInformation("=== APPLICATION CONFIGURED, STARTING ===");

await app.RunAsync();
