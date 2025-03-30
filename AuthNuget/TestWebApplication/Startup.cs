using AuthNuget.Registration;

namespace TestWebApplication;

public sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.RegisterPfeAuthorization();

        services.AddEndpointsApiExplorer();

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}