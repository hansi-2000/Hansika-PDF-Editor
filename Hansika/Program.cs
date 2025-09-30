using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;
using Syncfusion.Licensing;
using iText.License;

var builder = WebApplication.CreateBuilder(args);

// Register Syncfusion license
var syncfusionLicenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY") 
    ?? builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrEmpty(syncfusionLicenseKey) && syncfusionLicenseKey != "LICENSE_KEY")
{
    try
    {
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
        Console.WriteLine("Syncfusion license registered successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error registering Syncfusion license: {ex.Message}");
    }
}
else
{
    Console.WriteLine("Warning: No valid Syncfusion license key found. Using trial version.");
}

// Register iText7 license
var itextLicensePath = Environment.GetEnvironmentVariable("ITEXT_LICENSE_PATH") 
    ?? builder.Configuration["iText:LicensePath"];
if (!string.IsNullOrEmpty(itextLicensePath) && File.Exists(itextLicensePath))
{
    try
    {
        LicenseKey.LoadLicenseFile(itextLicensePath);
        Console.WriteLine("iText7 license registered successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Failed to load iText7 license: {ex.Message}");
    }
}
else
{
    // Try to find license file in common locations
    var commonPaths = new[]
    {
        "itextkey.json",
        "itextkey.xml",
        Path.Combine("Hansika", "itextkey.json"),
        Path.Combine("Hansika", "itextkey.xml")
    };
    
    string? foundLicensePath = null;
    foreach (var path in commonPaths)
    {
        if (File.Exists(path))
        {
            foundLicensePath = path;
            break;
        }
    }
    
    if (foundLicensePath != null)
    {
        try
        {
            LicenseKey.LoadLicenseFile(foundLicensePath);
            Console.WriteLine($"iText7 license registered successfully from {foundLicensePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to load iText7 license from {foundLicensePath}: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("Warning: No iText7 license file found. Using AGPL version with limitations.");
        Console.WriteLine("To use iText7 without trial limitations, add itextkey.json or itextkey.xml to the project root.");
    }
}

// Add controllers
builder.Services.AddControllers();


// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDirectoryBrowser(); // optional

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowReactApp");

// Serve /Uploads folder
var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
if (!Directory.Exists(uploadPath))
    Directory.CreateDirectory(uploadPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();


app.Run();
