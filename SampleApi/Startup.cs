using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using SampleApi.Contexts;
using SampleApi.OperationFilters;
using SampleApi.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SampleApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddHttpCacheHeaders((expirationModelOptions) =>
            {
                expirationModelOptions.MaxAge = 60;
                expirationModelOptions.CacheLocation = Marvin.Cache.Headers.CacheLocation.Private;
            },
           (validationModelOptions) =>
           {
               validationModelOptions.MustRevalidate = true;
           });


            services.AddResponseCaching();

            services.AddControllers(setupAction =>
            {

                services.AddHttpCacheHeaders();

                setupAction.ReturnHttpNotAcceptable = true;

                setupAction.CacheProfiles.Add("240SecondsCacheProfile",
                                                new CacheProfile()
                                                {
                                                    Duration = 240
                                                });


            }).AddNewtonsoftJson(setupAction =>
            {
                setupAction.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
            })
            .AddXmlDataContractSerializerFormatters()
            .ConfigureApiBehaviorOptions(setupAction =>
            {
                setupAction.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Type = "https://sampleApi.com/modelvalidationproblem",
                        Title = "One or more model validation errors occured.",
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Detail = "See the errors property for details.",
                        Instance = context.HttpContext.Request.Path
                    };

                    problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

                    return new UnprocessableEntityObjectResult(problemDetails)
                    {
                        ContentTypes = {"application/problem+json"}
                    };
                };
            });

            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters
                .OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

                if (newtonsoftJsonOutputFormatter != null)
                {
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.marvin.hateoas+json");
                }
            });

            //register PropertyMappingService
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();

            //register PropertyCheckerService
            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();

            // Configure EntityFrameWork With Sql Server
            var connectionString = Configuration["ConnectionStrings:SampleApiDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));


            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc("SampleAPIOpenAPISpecification", 
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                        Title = "Sample API",
                        Version = "1",
                        Description = "Through this API you can access authors and their books.",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                        {
                            Email = "olaideadebanjo@gmail.com",
                            Name = "Olaide Adebanjo",
                            Url = new Uri("https://www.twitter.com/olaideadebanjo")
                        },
                        License  = new Microsoft.OpenApi.Models.OpenApiLicense()
                        {
                            Name = "MLT License",
                            Url = new Uri("https://opensource.org/licenses/MIT")
                        },
                        
                });

                setupAction.OperationFilter<CreateAuthorOperationFilter>();
                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

                setupAction.IncludeXmlComments(xmlCommentsFullPath);

                 
            });

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }

            // app.UseResponseCaching();

            //app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint(
                    "/swagger/SampleAPIOpenAPISpecification/swagger.json",
                    "Sample API");

                setupAction.RoutePrefix = "";

            });


            app.UseHttpCacheHeaders();

            app.UseRouting();


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
