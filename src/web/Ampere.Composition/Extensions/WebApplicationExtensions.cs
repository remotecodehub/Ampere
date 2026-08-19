using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Ampere.Composition.Extensions;

/// <summary>Configures the Ampere HTTP pipeline.</summary>
public static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        /// <summary>
        /// Configures the HTTP pipeline and runs the app.
        /// </summary>
        public async Task RunAmpereAsync<T>()
            where T : Microsoft.AspNetCore.Components.IComponent
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(
                    "/Error",
                    createScopeForErrors: true);
                app.UseHsts();
            }
            else
            {
                app.MapOpenApi(
                    "ampere/{v1}.json")
                    .AllowAnonymous();
                app.MapScalarApiReference(
                    "ampere/scalar",
                    options =>
                    {
                        options.WithOpenApiRoutePattern(
                            "/ampere/{documentName}.json");
                        options.WithTitle(
                            $"AMPERE: [{app.Environment.EnvironmentName}]");
                    });
            }

            app.UseStatusCodePagesWithReExecute(
                "/not-found",
                createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapControllers();
            app.MapHub<Hub>("/hubs/ampere");
            app.MapRazorComponents<T>()
                .AddInteractiveServerRenderMode();

            await app.RunAsync();
        }
    }
}
