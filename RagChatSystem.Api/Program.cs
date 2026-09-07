using Microsoft.SemanticKernel;
using RagChatSystem.Api.Models;
using RagChatSystem.Api.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using RagChatSystem.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RAG Chat System API",
        Version = "v1",
        Description = "AI-Powered PDF Q&A using Azure OpenAI, Azure AI Search, and Azure Blob Storage. Upload PDFs and ask questions to get intelligent answers with citations.",
        Contact = new OpenApiContact
        {
            Name = "RAG Chat System",
            Email = "support@example.com"
        }
    });

    // Include XML comments for better documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Enable file upload support in Swagger UI
    options.OperationFilter<FileUploadOperationFilter>();
});

// Configure settings from appsettings.json
builder.Services.Configure<AzureOpenAISettings>(
    builder.Configuration.GetSection("AzureOpenAI"));
builder.Services.Configure<AzureSearchSettings>(
    builder.Configuration.GetSection("AzureSearch"));
builder.Services.Configure<AzureBlobStorageSettings>(
    builder.Configuration.GetSection("AzureBlobStorage"));
builder.Services.Configure<DocumentProcessingSettings>(
    builder.Configuration.GetSection("DocumentProcessing"));
builder.Services.Configure<AgenticAISettings>(
    builder.Configuration.GetSection("AgenticAI"));

// Configure Semantic Kernel with Azure OpenAI
var openAISettings = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAISettings>()
    ?? throw new InvalidOperationException("AzureOpenAI configuration is missing");

var kernelBuilder = Kernel.CreateBuilder();

// Add Azure OpenAI Chat Completion
kernelBuilder.AddAzureOpenAIChatCompletion(
    deploymentName: openAISettings.DeploymentName,
    endpoint: openAISettings.Endpoint,
    apiKey: openAISettings.ApiKey);

var kernel = kernelBuilder.Build();
builder.Services.AddSingleton(kernel);

// Register Azure services
builder.Services.AddSingleton<IAzureBlobStorageService, AzureBlobStorageService>();
builder.Services.AddSingleton<IAzureSearchService, AzureSearchService>();
builder.Services.AddSingleton<IDocumentProcessingService, DocumentProcessingService>();

// Toggle between Normal RAG and Agentic AI
var useAgenticAI = builder.Configuration.GetValue<bool>("AgenticAI:Enabled", false);

if (useAgenticAI)
{
    builder.Services.AddSingleton<IRagOrchestrationService, AgenticRagService>();
    builder.Services.AddHttpClient();
}
else
{
    builder.Services.AddSingleton<IRagOrchestrationService, RagOrchestrationService>();
}

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize Azure AI Search index on startup
using (var scope = app.Services.CreateScope())
{
    var searchService = scope.ServiceProvider.GetRequiredService<IAzureSearchService>();
    try
    {
        await searchService.InitializeIndexAsync();
        app.Logger.LogInformation("Azure AI Search index initialized successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to initialize Azure AI Search index");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("🚀 RAG Chat System started - Full Azure Stack (Azure OpenAI + Azure AI Search + Azure Blob Storage)");

app.Run();
