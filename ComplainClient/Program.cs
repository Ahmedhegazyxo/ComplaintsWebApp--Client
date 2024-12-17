using ComplainClient;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (builder.HostEnvironment.IsDevelopment())
{
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5288") });
}
else
{
    builder.Services.AddScoped(sp =>
    {
        string apiUri;
        var navigationManager = sp.GetRequiredService<NavigationManager>();
        if (navigationManager.BaseUri.Contains("192.168.200.36"))
        {
            apiUri = "http://192.168.200.36:81";
        }
        else
        {
            apiUri = "http://1.3.29.236:81";
        }
        return new HttpClient { BaseAddress = new Uri(apiUri) };
    });
}

await builder.Build().RunAsync();