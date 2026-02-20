using GAXT.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var построитель = WebAssemblyHostBuilder.CreateDefault(args);
построитель.RootComponents.Add<App>("#app");
построитель.RootComponents.Add<HeadOutlet>("head::after");

построитель.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(построитель.HostEnvironment.BaseAddress) });

await построитель.Build().RunAsync();
