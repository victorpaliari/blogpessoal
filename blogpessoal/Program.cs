using blogpessoal.Data;
using blogpessoal.Model;
using blogpessoal.Security.Implements;
using blogpessoal.Security;
using blogpessoal.Service;
using blogpessoal.Service.Implements;
using blogpessoal.Validator;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using blogpessoal.Configuration;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace blogpessoal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            // Conex�o com o banco de dados
            // Conex�o com o Banco de dados

            if (builder.Configuration["Enviroment:Start"] == "PROD")
            {
                /* Conex�o Remota (Nuvem) - PostgreSQL */

                builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("secrets.json");

                var connectionString = builder.Configuration
                    .GetConnectionString("ProdConnection");

                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString)
                );

            }
            else
            {
                /* Conex�o Local - SQL Server */

                var connectionString = builder.Configuration.
                    GetConnectionString("DefaultConnection");

                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString)
                );
            }

            //Registrar a valida��o das entidades
            builder.Services.AddTransient<IValidator<Postagem>, PostagemValidator>();
            builder.Services.AddTransient<IValidator<Tema>, TemaValidator>();
            builder.Services.AddTransient<IValidator<User>, UserValidator>();

            // Registrar as classes de servi�o (Service)
            builder.Services.AddScoped<IPostagemService, PostagemService>();
            builder.Services.AddScoped<ITemaService, TemaService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddTransient<IAuthService, AuthService>();


            // Adicionar a Valida��o do Token JWT
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {

                var Key = Encoding.UTF8.GetBytes(Settings.Secret);
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Key)
                };
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //Configura��o das informa��es do desenvolvedor
            builder.Services.AddSwaggerGen(options =>
            {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Projeto Blog Pessoal",
                Description = "Projeto Blog Pessoal - ASP.NET Core 7.0",
                Contact = new OpenApiContact
                {
                    Name = "Victor Paliari",
                    Email = "victorrpaliari@gmail.com",
                    Url = new Uri("https://github.com/victorpaliari")
                },
                License = new OpenApiLicense
                {
                    Name = "GitHub",
                    Url = new Uri("https://github.com/victorpaliari")
                }
            });

                //Adicionar a Seguran�a no Swagger
                options.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Digite um Token JWT v�lido!",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                //Adicionar a configura��o visual da Seguran�a no Swagger
                options.OperationFilter<AuthResponsesOperationFilter>();

            });


            //Adiconar o Fluent Validation no Swagger
            builder.Services.AddFluentValidationRulesToSwagger();

            // Configura��o do CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "MyPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            }
            );

            var app = builder.Build();

            // Criando o banco de dados e as tabelas
            using (var scope = app.Services.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }

            app.UseDeveloperExceptionPage();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();

            // Swagger como P�gina Inicial (Home) na Nuvem
            if (app.Environment.IsProduction())
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog Pessoal - v1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseSwaggerUI();

            //}

            // Habilitar a Autentica��o e a Autoriza��o
            app.UseAuthentication();

            app.UseAuthorization();

            //Inicia o CORS
            app.UseCors("MyPolicy");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}