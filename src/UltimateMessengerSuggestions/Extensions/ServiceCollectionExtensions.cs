using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Globalization;
using System.Reflection;
using UltimateMessengerSuggestions.Common.Behaviours;
using UltimateMessengerSuggestions.Common.Conventions;
using UltimateMessengerSuggestions.Common.Options;
using UltimateMessengerSuggestions.Common.Options.Configurators.Swagger;
using UltimateMessengerSuggestions.Common.Options.Loggers;
using UltimateMessengerSuggestions.Common.Options.Validators;
using UltimateMessengerSuggestions.Common.Options.Validators.Loggers;
using UltimateMessengerSuggestions.DbContexts;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;


namespace UltimateMessengerSuggestions.Extensions;

internal static class ServiceCollectionExtensions
{
	internal static IServiceCollection AddAppOptions(this IServiceCollection services)
	{
		services
			.AddOptionsWithValidateOnStart<ApplicationOptions>()
			.BindConfiguration(ApplicationOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<ConnectionStringsOptions>()
			.BindConfiguration(ConnectionStringsOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<SeqOptions>()
			.BindConfiguration(SeqOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<ConsoleLogOptions>()
			.BindConfiguration(ConsoleLogOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<DebugLogOptions>()
			.BindConfiguration(DebugLogOptions.ConfigurationSectionName);
		services
			.AddOptionsWithValidateOnStart<FileLogOptions>()
			.BindConfiguration(FileLogOptions.ConfigurationSectionName);

		return services;
	}

	internal static IServiceCollection AddAppOptionsValidators(this IServiceCollection services)
	{
		services.AddSingleton<IValidateOptions<ApplicationOptions>, ApplicationOptionsValidator>();
		services.AddSingleton<IValidateOptions<ConnectionStringsOptions>, ConnectionStringsOptionsValidator>();
		services.AddSingleton<IValidateOptions<SeqOptions>, SeqOptionsValidator>();
		services.AddSingleton<IValidateOptions<ConsoleLogOptions>, ConsoleLogOptionsValidator>();
		services.AddSingleton<IValidateOptions<DebugLogOptions>, DebugLogOptionsValidator>();
		services.AddSingleton<IValidateOptions<FileLogOptions>, FileLogOptionsValidator>();

		return services;
	}

	internal static IServiceCollection AddDatabaseConnection(this IServiceCollection services)
	{
		var connectionOptions = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<ConnectionStringsOptions>>()
			.Value;
		var applicationOptions = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<ApplicationOptions>>()
			.Value;

		return services.AddDbContext<IAppDbContext, AppDbContext>
		(
			options => options.UseNpgsql
			(
				connectionOptions.Default,
				options => options.EnableRetryOnFailure(
					maxRetryCount: applicationOptions.CheckDbRetryCount,
					maxRetryDelay: TimeSpan.FromSeconds(applicationOptions.CheckDbRetryDelay),
					errorCodesToAdd: null
				)
			)
		);
	}

	internal static IServiceCollection AddControllersWithJsonNamingPolicy(this IServiceCollection services)
	{
		services.Configure<ApiBehaviorOptions>(options =>
		{
			options.SuppressModelStateInvalidFilter = true;
		});
		services
			.AddControllers(options =>
			{
				options.Conventions.Add(new CamelCaseControllerNameConvention());
				options.Conventions.Add(new CamelCaseQueryParameterConvention());
			})
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonResponseExtensions.SerializerOptions.PropertyNamingPolicy;
				options.JsonSerializerOptions.DictionaryKeyPolicy = JsonResponseExtensions.SerializerOptions.DictionaryKeyPolicy;
				options.JsonSerializerOptions.PropertyNameCaseInsensitive = JsonResponseExtensions.SerializerOptions.PropertyNameCaseInsensitive;
			});
		services.Configure<JsonOptions>(options =>
		{
			options.SerializerOptions.PropertyNamingPolicy = JsonResponseExtensions.SerializerOptions.PropertyNamingPolicy;
			options.SerializerOptions.DictionaryKeyPolicy = JsonResponseExtensions.SerializerOptions.DictionaryKeyPolicy;
		});
		return services;
	}

	internal static IServiceCollection AddMediatRFromAssembly(this IServiceCollection services)
	{
		return services.AddMediatR(c =>
		{
			c.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
			c.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
			c.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
		});
	}

	internal static IServiceCollection AddAutoMapperFromAssembly(this IServiceCollection services)
	{
		return services.AddAutoMapper(Assembly.GetExecutingAssembly());
	}

	internal static IServiceCollection AddValidatorsFromAssembly(this IServiceCollection services)
	{
		ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");
		services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
		return services;
	}

	internal static IServiceCollection AddSwaggerSupport(this IServiceCollection services)
	{
		services.ConfigureOptions<ConfigureSwaggerOptions>();
		services.AddEndpointsApiExplorer();
		services.AddApiVersioning(options =>
		{
			options.AssumeDefaultVersionWhenUnspecified = true;
			options.DefaultApiVersion = new ApiVersion(1);
			options.ReportApiVersions = true;
			options.ApiVersionReader = new UrlSegmentApiVersionReader();
		}).AddApiExplorer(options =>
			{
				options.GroupNameFormat = "'v'VVV";
				options.SubstituteApiVersionInUrl = true;
			}
		);
		services.AddSwaggerGen(options =>
		{
			options.IncludeXmlComments(Assembly.GetExecutingAssembly());
			options.EnableAnnotations();
			options.DocInclusionPredicate(((docName, apiDesc) =>
			{
				var groupName = apiDesc.GroupName;
				return groupName == docName;
			}));
		});

		return services;
	}

	internal static IServiceCollection AddAppHealthChecks(this IServiceCollection services)
	{
		var connectionOptions = services
			.BuildServiceProvider()
			.GetRequiredService<IOptions<ConnectionStringsOptions>>()
			.Value;

		services.AddHealthChecks()
			.AddNpgSql(connectionOptions.Default, name: "database");

		return services;
	}

	internal static IServiceCollection AddOpenTelemetryMetrics(this IServiceCollection services)
	{
		services.AddOpenTelemetry()
			.WithMetrics(metrics =>
			{
				metrics
					.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Assembly.GetExecutingAssembly().GetName().Name ?? "UltimateMessengerSuggestions"))
					.AddAspNetCoreInstrumentation()
					.AddRuntimeInstrumentation()
					.AddPrometheusExporter(); // <-- creates /metrics
			});

		return services;
	}

	internal static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
	{
		services.AddCors(options =>
		{
			options.AddPolicy("AllowAllOrigins",
				builder => builder
					.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader());
		});
		return services;
	}	
}
