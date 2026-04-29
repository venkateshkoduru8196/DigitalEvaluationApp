using DIGITALEVALUATION.Contexts;
using DIGITALEVALUATION.Helpers;
using DIGITALEVALUATION.Models;
using DIGITALEVALUATION.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DIGITALEVALUATION.Exceptions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =============================
// JWT CONFIG
// =============================
builder.Services.Configure<JWT>(
    builder.Configuration.GetSection("JWT")
);

// =============================
// DATABASE CONNECTION
// =============================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// =============================
// IDENTITY
// =============================
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// =============================
// DEPENDENCY INJECTION SERVICES
// =============================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ICollegeService, CollegeService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<ICourseSubjectService, CourseSubjectService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IFacultyService, FacultyService>();
builder.Services.AddScoped<IFacultySubjectService, FacultySubjectService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IAnswerSheetService, AnswerSheetService>();
builder.Services.AddScoped<IEvaluationService, EvaluationService>();
builder.Services.AddScoped<IRevaluationRequestService, RevaluationRequestService>();
builder.Services.AddScoped<IRevaluationAssignmentService, RevaluationAssignmentService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IQuestionOptionService, QuestionOptionService>();
builder.Services.AddScoped<IExamQuestionService, ExamQuestionService>();
builder.Services.AddScoped<IExamSectionService, ExamSectionService>();
builder.Services.AddScoped<IStudentExamAssignmentService, StudentExamAssignmentService>();
builder.Services.AddScoped<IStudentAnswerService, StudentAnswerService>();
builder.Services.AddScoped<IRevaluationMarkService, RevaluationMarkService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IExportService, ExportService>();

// =============================
// JWT AUTHENTICATION
// =============================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Change to true after SSL is fully working
    options.RequireHttpsMetadata = false;

    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,

        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["JWT:Key"]!
            )
        ),

        ClockSkew = TimeSpan.Zero
    };

    // Allow OPTIONS requests for CORS preflight
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.HttpContext.Request.Method == "OPTIONS")
            {
                context.NoResult();
            }

            return Task.CompletedTask;
        }
    };
});

// =============================
// CORS (SUBDOMAIN + LOCAL)
// =============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                // Production Subdomain
                "https://app.genbasesoftware.com",
                "http://app.genbasesoftware.com",

                // Local Development
                "http://localhost:5173",
                "http://localhost:5174"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// =============================
// CONTROLLERS
// =============================
builder.Services.AddControllers();

// =============================
// SWAGGER + JWT
// =============================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "DIGITAL EVALUATION API",
            Version = "v1"
        });

    options.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter: Bearer YOUR_TOKEN"
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

// =============================
// BUILD APP
// =============================
var app = builder.Build();

// =============================
// ROLE SEEDING
// =============================
using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    string[] roles = { "Admin", "User" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(
                new ApplicationRole
                {
                    Name = role
                });
        }
    }
}

// =============================
// PIPELINE
// =============================

// Swagger enabled during deployment/testing
app.UseSwagger();
app.UseSwaggerUI();

// Global Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();

// HTTPS Redirect
app.UseHttpsRedirection();

// Disable aggressive caching during updates
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cache-Control", "no-store");
    await next();
});

// Default file support (wwwroot/index.html)
app.UseDefaultFiles();

// Static React files from wwwroot
app.UseStaticFiles();

// Routing
app.UseRouting();

// CORS BEFORE AUTH
app.UseCors("AllowFrontend");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// API Controllers
app.MapControllers();

// React SPA fallback
app.MapFallbackToFile("index.html");

// =============================
// RUN APP
// =============================
app.Run();