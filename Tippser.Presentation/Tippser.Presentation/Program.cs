using Hangfire;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tippser.Application.Services;
using Tippser.Core.Entities;
using Tippser.Core.Interfaces;
using Tippser.Infrastructure.Repositories;
using Tippser.Infrastructure.Services;
using Tippser.Presentation.Client.Pages;
using Tippser.Presentation.Components;
using Tippser.Infrastructure.Data;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Tippser.Presentation.Helpers;
using Tippser.Core;
using Tippser.Presentation.Client.Services;
using Tippser.Presentation.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddControllers();

builder.Services.AddLogging();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingRevalidatingAuthenticationStateProvider>();

builder.Services.AddScoped<IBaseRepository, BaseRepository>();
builder.Services.AddScoped<IDbService, DbService>();
builder.Services.AddScoped<IBackroundJobsService, BackroundJobsService>();
builder.Services.AddScoped<IScraperService, ScraperService>();
builder.Services.AddScoped<IMatchDataService, MatchDataService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher<Person>, PasswordHasher<Person>>();
builder.Services.AddScoped<ISignInService, SignInService>();

builder.Services.AddSingleton<SharedStateService>();

var baseAddress = builder.Configuration[Constants.ApiBaseUrl]?.ToString() ?? 
        throw new InvalidOperationException($"API address '{Constants.ApiBaseUrl}' not found.");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

var connectionString = builder.Configuration.GetConnectionString(Constants.TippserConnectionString) ?? 
        throw new InvalidOperationException($"Connection string '{Constants.TippserConnectionString}' not found.");
builder.Services.AddDbContext<TippserDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<Person, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddEntityFrameworkStores<TippserDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/signin";
    options.LogoutPath = "/account/signout";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // In production
});


// add CORS policy for Wasm client
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins(builder.Configuration[Constants.ApiBaseUrl]!)
            .AllowAnyMethod()
            .SetIsOriginAllowed(pol => true)
            .AllowAnyHeader()
            .AllowCredentials()));

builder.Services.AddSingleton<IEmailSender<Person>, EmailService>();

builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

var supportedCultures = new List<CultureInfo>
{
    new("sv-SE"),
    new("en-US"),
    new("en-GB")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
};

localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()    
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Counter).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalEndpoints();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var backgroundJobsService = scope.ServiceProvider.GetRequiredService<IBackroundJobsService>();
    backgroundJobsService.CreateSchedule();
}

app.Run();
