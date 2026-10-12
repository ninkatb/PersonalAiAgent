using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using PersonalAI.Application;
using PersonalAI.Domain;

namespace PersonalAI.Api;

public static class ApiHost
{
    public static WebApplication Create(SessionConfiguration session, TextWriter logs,
        Action<WebApplication>? configureTestRoutes = null)
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions
        {
            Args = [], EnvironmentName = "Production"
        });
        // No environment/argv/appsettings source may add a second listener.
        builder.Configuration.Sources.Clear();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new SafeLogProvider(logs));
        builder.WebHost.ConfigureKestrel(server =>
        {
            server.AddServerHeader = false;
            server.Limits.MaxRequestBodySize = 4096;
            server.Limits.MaxRequestHeadersTotalSize = 8192;
            server.Listen(IPAddress.Loopback, 0);
        });
        builder.Services.AddFoundation();
        builder.Services.AddSingleton<IRequestAuthenticator>(session);
        var app = builder.Build();
        app.Lifetime.ApplicationStopping.Register(session.Revoke);
        app.Use(async (context, next) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            try { await next(context); }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                context.Abort();
            }
            catch (Exception error) when (!context.Response.HasStarted)
            {
                var invalid = error is BadHttpRequestException or JsonException or InvalidDataException;
                app.Logger.LogWarning(new EventId(invalid ? 400 : 500), "Request failed");
                context.Response.Clear();
                await WriteProblem(context, invalid ? 400 : 500,
                    invalid ? "VALIDATION_FAILED" : "INTERNAL_ERROR");
            }
        });
        app.Use(async (context, next) =>
        {
            if (!session.MatchesBoundary(context.Request))
            {
                await WriteProblem(context, 403, "SESSION_BOUNDARY_INVALID");
                return;
            }
            if (HttpMethods.IsOptions(context.Request.Method))
            {
                await Preflight(context, session);
                return;
            }
            if (!session.Authorizes(context.Request))
            {
                await WriteProblem(context, 401, "SESSION_UNAUTHORIZED");
                return;
            }
            context.Response.Headers.AccessControlAllowOrigin = session.Origin;
            context.Response.Headers.Vary = "Origin";
            using var operation = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted, session.Revoked);
            context.RequestAborted = operation.Token;
            using var abortOnRevocation = session.Revoked.Register(context.Abort);
            operation.Token.ThrowIfCancellationRequested();
            await next(context);
            if (context.Response.StatusCode >= 400 && !context.Response.HasStarted)
                await WriteProblem(context, context.Response.StatusCode, "REQUEST_REJECTED");
        });
        app.MapGet("/health", () => new { status = "ok" });
        configureTestRoutes?.Invoke(app);
        return app;
    }

    private static async Task Preflight(HttpContext context, SessionConfiguration session)
    {
        var request = context.Request;
        if (request.Path != "/health" || request.QueryString.HasValue ||
            request.Headers.AccessControlRequestMethod.Count != 1 ||
            request.Headers.AccessControlRequestMethod[0] != "GET" ||
            request.Headers.AccessControlRequestHeaders.Count != 1 ||
            !string.Equals(request.Headers.AccessControlRequestHeaders[0], "authorization", StringComparison.OrdinalIgnoreCase))
        {
            await WriteProblem(context, 403, "PREFLIGHT_REJECTED");
            return;
        }
        context.Response.Headers.AccessControlAllowOrigin = session.Origin;
        context.Response.Headers.AccessControlAllowMethods = "GET";
        context.Response.Headers.AccessControlAllowHeaders = "Authorization";
        context.Response.Headers.Vary = "Origin";
        context.Response.StatusCode = 204;
    }

    public static Task WriteProblem(HttpContext context, int status, string code)
    {
        context.Response.StatusCode = status;
        context.Response.Headers.CacheControl = "no-store";
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status, Title = "Request failed", Type = "about:blank",
            Extensions =
            {
                ["errorCode"] = code, ["parameters"] = new { },
                ["correlationId"] = Guid.NewGuid().ToString("N"), ["retryable"] = status >= 500
            }
        }, options: (JsonSerializerOptions?)null, contentType: "application/problem+json");
    }

    public static int GetPort(WebApplication app)
    {
        var addresses = app.Services.GetRequiredService<IServer>().Features
            .Get<IServerAddressesFeature>()!.Addresses;
        var address = new Uri(addresses.Single());
        if (address.Host != "127.0.0.1" || address.Port <= 0)
            throw new InvalidDataException("LISTENER_INVALID");
        return address.Port;
    }
}
