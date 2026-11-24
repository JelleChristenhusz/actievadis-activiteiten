using UI.Components;
using UI.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// The UI gets its data from the API. The API address ("ApiSettings:BaseUrl") differs per environment:
// - local development: appsettings.Development.json (localhost)
// - server: set it in appsettings.Production.json or the environment variable ApiSettings__BaseUrl
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException(
        "ApiSettings:BaseUrl is missing. Set it in appsettings.Development.json (local) or appsettings.Production.json (server).");

// Keeps the JWT of the visitor who logged in.
builder.Services.AddScoped<JwtTokenStore>();

builder.Services.AddHttpClient<IActivityApiService, ActivityApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<IAuthenticationApiService, AuthenticationApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient("UserManagementApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
