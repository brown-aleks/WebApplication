//  https://memorycrypt.hashnode.dev/create-a-web-api-with-jwt-authentication-and-aspnet-core-identity

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebApplicationJWT.Handlers;
using WebApplicationJWT.Models;

namespace WebApplicationJWT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //add Auth db context
            builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(builder.Configuration["ConnectionStrings:PostgreSQLAuthConnection"]));
            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();
            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireLowercase = false;          //  требовать хотя бы одну маленькую букву
                options.Password.RequireDigit = false;              //  требовать хотя бы одну цифру
                options.Password.RequireNonAlphanumeric = false;    //  требовать хотя бы один символ отличающийся от буквенно-цифрового
                options.Password.RequireUppercase = false;          //  требовать хотя бы одну заглавную букву
                options.Password.RequiredLength = 6;                //  требовать минимальную длину пароля
                options.Password.RequiredUniqueChars = 1;           //  требовать минимальное количество уникальных символов
            });

            //add Article db context
            builder.Services.AddDbContext<ArticleDbContext>(options =>
                options.UseNpgsql(builder.Configuration["ConnectionStrings:PostgreSQLArticleConnection"]));

            // Adding services to the container.
            builder.Services.AddControllers();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "SpecialScheme"; //  »спользуетс¤ в качестве схемы по умолчанию
                options.DefaultChallengeScheme = "SpecialScheme";
                options.DefaultScheme = "SpecialScheme";
            })
                            .AddScheme<SpecialAuthenticationSchemeOptions, SpecialAuthHandler>(
                                "SpecialScheme", options => { });

            #region JwtBearerDefaults
            /*
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //  используется в качестве схемы по умолчанию
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                            .AddJwtBearer(options =>
                            {
                                options.RequireHttpsMetadata = false;
                                options.SaveToken = true;
                                options.TokenValidationParameters = new TokenValidationParameters()
                                {
                                    ValidateIssuer = true,
                                    ValidateIssuerSigningKey = true,
                                    ValidateAudience = true,
                                    ValidateLifetime = true,
                                    ValidAudience = builder.Configuration["token:audience"],
                                    ValidIssuer = builder.Configuration["token:issuer"],
                                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["token:key"])),
                                    // устанавливаем clockskew равным нулю, чтобы срок действия токенов истекал точно в момент истечении срока действия токена (а не через 5 минут)
                                    ClockSkew = TimeSpan.Zero
                                };
                            });
            */
            #endregion

            // Learn more about configuring Swagger/Open API at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Set up an HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}