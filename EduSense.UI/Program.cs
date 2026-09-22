using EduSense.UI;
using EduSense.UI.Http;
using EduSense.UI.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrlSetting = builder.Configuration["ApiBaseUrl"] ?? throw new InvalidOperationException("API base URL is not configured.");

// Byt ut host-delen mot den host webbläsaren faktiskt öppnade UI:t via
// (localhost, din LAN-IP, ...) - "localhost" i appsettings.json betyder
// annars alltid webbläsarens EGEN maskin, inte servern som levererade sidan.
var uiHost = new Uri(builder.HostEnvironment.BaseAddress).Host;
var apiBaseUrl = new UriBuilder(new Uri(apiBaseUrlSetting)) { Host = uiHost }.Uri.ToString();

builder.Services.AddScoped(sp => new HttpClient(new CookieHandler { InnerHandler = new HttpClientHandler() })
{
    BaseAddress = new Uri(apiBaseUrl)
});
builder.Services.AddScoped<ApiService>();

// AddAuthorizationCore (inte AddAuthorization) - WASM-varianten utan serverberoenden.
// Policies måste registreras separat på klienten - de delas inte automatiskt
// med API-projektets Program.cs eftersom det är två olika processer.
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AnalystOnly", policy => policy.RequireRole("Analyst"));
    options.AddPolicy("AdminOrAnalyst", policy => policy.RequireRole("Admin", "Analyst"));
}); 

builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CookieAuthenticationStateProvider>());

await builder.Build().RunAsync();
