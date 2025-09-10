using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using UltimateMessengerSuggestions.Common.Handlers.Exceptions;
using UltimateMessengerSuggestions.Common.HealthChecks;
using UltimateMessengerSuggestions.Common.Options;

namespace UltimateMessengerSuggestions.Extensions;

internal static class ApplicationBuilderExtensions
{
	/// <summary>
	/// Adds a login requirement before accessing Swagger.
	/// </summary>
	/// <param name="builder"></param>
	/// <returns></returns>
	public static IApplicationBuilder RequireSwaggerAuthorization(this IApplicationBuilder builder)
	{
		var options = builder.ApplicationServices
			.GetRequiredService<IOptions<SwaggerAuthOptions>>()
			.Value;
		return options.RequireLogin
			? builder
			.UseWhen(context => context.Request.Path.StartsWithSegments("/swagger"), appBuilder =>
			{
				appBuilder.Use(async (context, next) =>
				{
					string authHeader = context.Request.Headers["Authorization"];

					if (authHeader != null && authHeader.StartsWith("Basic "))
					{
						var encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
						var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

						var parts = decodedUsernamePassword.Split(':');
						var username = parts[0];
						var password = parts[1];

						if (username == options.Username && password == options.Password)
						{
							await next.Invoke();
							return;
						}
					}

					context.Response.Headers["WWW-Authenticate"] = "Basic";
					context.Response.StatusCode = 401;
				});
			})
			: builder;
	}

	/// <summary>
	/// Adds and configures Swagger documentation in the application
	/// </summary>
	/// <param name="builder">The application configuration builder</param>
	/// <returns>The configured application configuration builder</returns>
	public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder builder)
	{
		var appOptions = builder.ApplicationServices.GetRequiredService<IOptions<ApplicationOptions>>().Value;
		var forwardedHeadersOptions = new ForwardedHeadersOptions
		{
			ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
		};
		appOptions.KnownProxies
			.Select(IPAddress.Parse)
			.ToList()
			.ForEach(forwardedHeadersOptions.KnownProxies.Add);

		// Enable processing of HTTP headers if the service is behind a reverse proxy.
		builder.UseForwardedHeaders(forwardedHeadersOptions);

		// Configure Swagger URLs for correct operation behind a reverse proxy,
		// using information from the HTTP request (scheme, host, base path)
		builder.UseSwagger(options =>
		{
			options.PreSerializeFilters.Add((swagger, httpReq) =>
			{
				swagger.Servers = new List<OpenApiServer>
				{
					new() { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{httpReq.PathBase.Value?.TrimEnd('/')}" }
				};
			});
		});

		// Configure the Swagger UI interface, adding endpoints for each API version.
		// Use the IApiVersionDescriptionProvider to get information about all API versions
		// and register them in SwaggerUI with the corresponding URLs and names.
		builder.UseSwaggerUI(options =>
		{
			var provider = builder.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
			foreach (var desc in provider.ApiVersionDescriptions)
			{
				options.SwaggerEndpoint($"{desc.GroupName}/swagger.json", $"{desc.GroupName}");
			}

			options.RoutePrefix = "swagger";
		});
		return builder;
	}

	internal static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
	{
		var exceptionMiddleware = new CustomExceptionHandler();
		return app.Use(async (context, next) =>
		{
			try
			{
				await next(context);
			}
			catch (Exception ex)
			{
				await exceptionMiddleware.TryHandleAsync(context, ex, new CancellationToken());
			}
		});
	}

	internal static IEndpointRouteBuilder MapHealthCheckEndpoint(this IEndpointRouteBuilder endpoints, string path = "/health")
	{
		endpoints.MapHealthChecks(path, new HealthCheckOptions
		{
			ResponseWriter = async (context, report) =>
			{
				context.Response.ContentType = "application/json";

				var response = new HealthCheckResponse
				{
					Status = report.Status.ToString(),
					Duration = report.TotalDuration.TotalMilliseconds,
					Checks = report.Entries.Select(entry => new HealthCheckComponent
					{
						Entry = entry.Key,
						Status = entry.Value.Status.ToString(),
						Description = entry.Value.Description,
						ErrorMessage = entry.Value.Exception?.Message
					}).ToList()
				};

				var json = JsonSerializer.Serialize(response, JsonResponseExtensions.SerializerOptions);
				await context.Response.WriteAsync(json);
			}
		});

		return endpoints;
	}

	internal static WebApplication MapOpenTelemetryMetricsEndpoint(this WebApplication app, string path = "/metrics")
	{
		app.MapPrometheusScrapingEndpoint(path);

		return app;
	}
}
