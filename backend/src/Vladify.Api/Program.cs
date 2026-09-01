using Vladify.Api.Playlists;
using Vladify.Application.Playlists;
using Vladify.Infrastructure.Spotify;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSpotifyIntegration(builder.Configuration);
builder.Services.AddScoped<ImportPlaylistUseCase>();
builder.Services.AddScoped<RefreshPlaylistUseCase>();

// Self-hosted, single-user app with a separate frontend origin during development.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapPlaylistEndpoints();

app.Run();
