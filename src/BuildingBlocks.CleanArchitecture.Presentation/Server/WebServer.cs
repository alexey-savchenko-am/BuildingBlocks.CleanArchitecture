using Asp.Versioning;
using BuildingBlocks.CleanArchitecture.Presentation.Endpoints;
using BuildingBlocks.CleanArchitecture.Presentation.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;

namespace BuildingBlocks.CleanArchitecture.Presentation.Server;

public class WebServer
{
    private readonly WebApplicationBuilder _builder;
    private WebApplication? _app;

    private bool _controllersConfigured = false;
    private bool _minimalApiEndpointsConfigured = false;
    private bool _swaggerConfigured = false;
    private bool _jwtAuthConfigured = false;
    private string? _corsPolicyName;

    private ApiVersion[]? _preconfiguredVersions;

    private WebServer(string[] args)
    {
        _builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .Enrich.WithProperty("Phase", "Bootstrap")
            .CreateBootstrapLogger();

        Log.Information("Bootstrap logger initialized");
    }

    public static WebServer Create(string[] args) 
        => new(args);
    
    public WebServer ConfigureLogging(Action<HostBuilderContext, LoggerConfiguration, IConfiguration>? configure = null)
    {
        _builder.Host.UseSerilog((context, config) =>
        {
            config
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", _builder.Environment.ApplicationName);

            // apply additional configuration if it was provided
            configure?.Invoke(context, config, _builder.Configuration);
        });

        Log.Information("Full serilog configuration applied");

        return this;
    }

    public WebServer ConfigureCors(
        string policyName = "AllowAll", 
        Action<CorsPolicyBuilder>? configurePolicy = null)
    {
        _builder.Services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
            {
                if(configurePolicy is not null)
                {
                    configurePolicy?.Invoke(policy);
                }
                else
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        _corsPolicyName = policyName;
        Log.Information("CORS policy applied");

        return this;
    }

    public WebServer ConfigureJwtAuthentication(Func<IConfiguration, JwtTokenSettings> settingsFactory)
    {
        var settings = settingsFactory.Invoke(_builder.Configuration);
        _builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var keyBytes = Encoding.ASCII.GetBytes(settings.Token);
            options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
            options.SaveToken = true;
            options.TokenValidationParameters = 
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };
        });

        _jwtAuthConfigured = true;
        Log.Information("JWT authentication applied");

        return this;
    }

    public WebServer ConfigureSwagger<TSwaggerGenOptions>(bool configureBearerTokenSecurity = true)
        where TSwaggerGenOptions : class, IConfigureOptions<SwaggerGenOptions>
    {
        _builder.Services.AddEndpointsApiExplorer();
        _builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, TSwaggerGenOptions>();
        _builder.Services.AddSwaggerGen(options =>
        {
            if (configureBearerTokenSecurity)
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Insert 'Bearer' [space] and your token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            }

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        _swaggerConfigured = true;
        Log.Information("Swagger configuration applied");

        return this;
    }


    public WebServer ConfigureApiVersioning(params ApiVersion[] versions)
    {
        _preconfiguredVersions = versions;
        _builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        Log.Information("API Versioning configured");
        return this;
    }

    public WebServer ConfigureVersionedEndpoints()
    {
        _builder.Services.AddVersionedEndpoints();
        _minimalApiEndpointsConfigured = true;
        Log.Information("Versioned endpoints configured");
        return this;
    }

    public WebServer ConfigureControllers()
    {
        _builder.Services.AddVersionedEndpoints();
        _controllersConfigured = true;
        Log.Information("Controllers configured");
        return this;
    }

    public WebServer ConfigureServices(Action<IServiceCollection, IConfiguration> serviceBuilder)
    {
        serviceBuilder.Invoke(_builder.Services, _builder.Configuration);
        return this;
    }

    public WebServer Build()
    {
        _app = _builder.Build();

        if(_preconfiguredVersions is not null)
        {
            var versionSetBuilder = _app.NewApiVersionSet().ReportApiVersions();
            foreach(var version in _preconfiguredVersions)
            {
                versionSetBuilder.HasApiVersion(version);
            }

            _app.MapVersionedEndpoints(versionSetBuilder.Build());
        }

        if(_corsPolicyName is not null)
        {
            _app.UseCors(_corsPolicyName!);
        }

        _app.UseMiddleware<CorrelationIdMiddleware>();
       
        if(_swaggerConfigured)
        {
            _app.UseSwagger();

            _app.UseSwaggerUI(options =>
            {
                var descriptions = _app.DescribeApiVersions();

                foreach (var description in descriptions)
                {
                    var url = $"/swagger/{description.GroupName}/swagger.json";
                    var name = description.GroupName.ToUpperInvariant();
                    options.SwaggerEndpoint(url, name);
                }
            });
        }

        _app.UseHttpsRedirection();

        if(_jwtAuthConfigured)
        {
            _app.UseAuthentication();
            _app.UseAuthorization();
        }

        _app.UseMiddleware<ValidationExceptionHandlingMiddleware>();
        _app.UseAntiforgery();
        _app.UseStaticFiles();

        if(_controllersConfigured)
        {
            _app.MapControllers();
        }
        
        return this;
    }


    public async Task RunAsync()
    {
        try
        {
            if(_app is not null)
            {
                Log.Information($"Starting server '{_builder.Environment.ApplicationName}'...");

                await _app.StartAsync().ConfigureAwait(false);

                foreach (var address in _app.Urls)
                {
                    Log.Information($"Server '{_builder.Environment.ApplicationName}' is now listening on {address}");
                }

                await _app.WaitForShutdownAsync().ConfigureAwait(false);
            }
            else
            {
                Log.Warning("Server starting failure. You must configure server first!");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, $"Unhandled exception in '{_builder.Environment.ApplicationName}'");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
