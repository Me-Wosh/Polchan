using Polchan.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServices(builder);

var app = builder.Build();
app.ConfigureApp();
app.ConfigureStartup();
app.MapEndpoints();
app.Run();
