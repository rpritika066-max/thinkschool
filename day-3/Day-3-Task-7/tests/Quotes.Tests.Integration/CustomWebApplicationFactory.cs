using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using QuotesApi.Data;
using QuotesApi.Services;
using System;
using System.Linq;

namespace Quotes.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection _connection;
    public IClock MockClock { get; } = Substitute.For<IClock>();

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        
        // Ensure standard time for tests, but keep it close to actual time so JWTs don't expire immediately.
        MockClock.UtcNow.Returns(DateTimeOffset.UtcNow);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the app's DbContext registration.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<QuoteDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Remove the app's IClock registration
            var clockDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IClock));
            if (clockDescriptor != null)
            {
                services.Remove(clockDescriptor);
            }

            // Add IClock fake
            services.AddSingleton(MockClock);

            // Add DbContext using an in-memory database for testing.
            services.AddDbContext<QuoteDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
            
            // Build the service provider.
            var sp = services.BuildServiceProvider();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}
