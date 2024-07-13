

using Hangfire;
using HangfireShowcase;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ExampleJob>();
builder.Services.AddHangfire(x => {
    x.UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    //.UseInMemoryStorage();
    .UseSqlServerStorage("Server=localhost;Database=HangfireDB;Trusted_Connection=True;TrustServerCertificate=True;"); 
    // this is using windows authentication connectionstring and not suitable for prod
});
builder.Services.AddHangfireServer(x => {

    x.SchedulePollingInterval = TimeSpan.FromSeconds(1);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/job", (IBackgroundJobClient jobClient, IRecurringJobManager manager) => {

    manager.AddOrUpdate("every5second", () => Console.WriteLine("Hello manager"), "*/5 * * * *");

    jobClient.Enqueue(() => Console.WriteLine("Hello from BG"));
    jobClient.Schedule(() => Console.WriteLine("Hello Scheduler"), TimeSpan.FromSeconds(5));
    jobClient.Enqueue<ExampleJob>((x) => x.Run());
    jobClient.Schedule<ExampleJob>((x) => x.Run(), TimeSpan.FromSeconds(2));


    return Results.Ok("Hello job");
});
app.UseHangfireDashboard();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
