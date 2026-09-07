using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml;
using CsvHelper;
using System.Globalization;
using RagChatSystem.Api.Models;

namespace RagChatSystem.Api.Services;

public interface IDocumentProcessingService
{
    Task<List<DocumentChunk>> ProcessDocumentAsync(Stream fileStream, string fileName, string documentId, Dictionary<string, string> metadata);
    Task<List<DocumentChunk>> ProcessPdfAsync(Stream pdfStream, string fileName, string documentId, Dictionary<string, string> metadata);
    Task<List<DocumentChunk>> ProcessDocxAsync(Stream docxStream, string fileName, string documentId, Dictionary<string, string> metadata);
    Task<List<DocumentChunk>> ProcessExcelAsync(Stream excelStream, string fileName, string documentId, Dictionary<string, string> metadata);
    Task<List<DocumentChunk>> ProcessTextAsync(Stream textStream, string fileName, string documentId, Dictionary<string, string> metadata);
    Task<List<DocumentChunk>> ProcessCsvAsync(Stream csvStream, string fileName, string documentId, Dictionary<string, string> metadata);
}

public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly DocumentProcessingSettings _settings;
    private readonly ILogger<DocumentProcessingService> _logger;

    public DocumentProcessingService(
        IConfiguration configuration,
        ILogger<DocumentProcessingService> logger)
    {
        _settings = configuration.GetSection("DocumentProcessing").Get<DocumentProcessingSettings>() 
            ?? new DocumentProcessingSettings();
        _logger = logger;
    }

    public async Task<List<DocumentChunk>> ProcessDocumentAsync(
        Stream fileStream, 
        string fileName, 
        string documentId, 
        Dictionary<string, string> metadata)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".pdf" => await ProcessPdfAsync(fileStream, fileName, documentId, metadata),
            ".docx" => await ProcessDocxAsync(fileStream, fileName, documentId, metadata),
            ".xlsx" or ".xls" => await ProcessExcelAsync(fileStream, fileName, documentId, metadata),
            ".txt" => await ProcessTextAsync(fileStream, fileName, documentId, metadata),
            ".csv" => await ProcessCsvAsync(fileStream, fileName, documentId, metadata),
            _ => throw new NotSupportedException($"File type {extension} is not supported. Supported formats: PDF, DOCX, XLSX, XLS, TXT, CSV")
        };
    }

    public async Task<List<DocumentChunk>> ProcessPdfAsync(
        Stream pdfStream, 
        string fileName, 
        string documentId, 
        Dictionary<string, string> metadata)
    {
        try
        {
            var text = await ExtractTextFromPdfAsync(pdfStream);
            var chunks = ChunkText(text, documentId, fileName, metadata);
            
            _logger.LogInformation("Processed PDF {FileName} into {ChunkCount} chunks", fileName, chunks.Count);
            return chunks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PDF {FileName}", fileName);
            throw;
        }
    }

    public async Task<List<DocumentChunk>> ProcessDocxAsync(
        Stream docxStream, 
        string fileName, 
        string documentId, 
        Dictionary<string, string> metadata)
    {
        try
        {
            var text = await ExtractTextFromDocxAsync(docxStream);
            var chunks = ChunkText(text, documentId, fileName, metadata);
            
            _logger.LogInformation("Processed DOCX {FileName} into {ChunkCount} chunks", fileName, chunks.Count);
            return chunks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DOCX {FileName}", fileName);
            throw;
        }
    }

    public async Task<List<DocumentChunk>> ProcessExcelAsync(
        Stream excelStream, 
        string fileName, 
        string documentId, 
        Dictionary<string, string> metadata)
    {
        try
        {
            var text = await ExtractTextFromExcelAsync(excelStream);
            var chunks = ChunkText(text, documentId, fileName, metadata);
            
            _logger.LogInformation("Processed Excel {FileName} into {ChunkCount} chunks", fileName, chunks.Count);
            return chunks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Excel {FileName}", fileName);
            throw;
        }
    }

    public async Task<List<DocumentChunk>> ProcessTextAsync(
        Stream textStream, 
        string fileName, 
        string documentId, 
        Dictionary<string, string> metadata)
    {
        try
        {
            var text = await ExtractTextFromTextAsync(textStream);
            var chunks = ChunkText(text, documentId, fileName, metadata);
            
            _logger.LogInformation("Processed text {FileName} into {ChunkCount} chunks", fileName, chunks.Count);
            return chunks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing text {FileName}", fileName);
            throw;
        }
    }

    public async Task<List<DocumentChunk>> ProcessCsvAsync(
        Stream csvStream, 
        string fileName, 
        string documentId, 
        Dictionary<string, string> metadata)
    {
        try
        {
            var text = await ExtractTextFromCsvAsync(csvStream);
            var chunks = ChunkText(text, documentId, fileName, metadata);
            
            _logger.LogInformation("Processed CSV {FileName} into {ChunkCount} chunks", fileName, chunks.Count);
            return chunks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CSV {FileName}", fileName);
            throw;
        }
    }

    private async Task<string> ExtractTextFromPdfAsync(Stream pdfStream)
    {
        return await Task.Run(() =>
        {
            using var pdfReader = new PdfReader(pdfStream);
            using var pdfDocument = new PdfDocument(pdfReader);
            
            var text = new System.Text.StringBuilder();
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                text.AppendLine(pageText);
            }
            
            return text.ToString();
        });
    }

    private async Task<string> ExtractTextFromDocxAsync(Stream docxStream)
    {
        return await Task.Run(() =>
        {
            using var document = WordprocessingDocument.Open(docxStream, false);
            var body = document.MainDocumentPart?.Document?.Body;
            
            if (body == null)
                return string.Empty;

            var text = new System.Text.StringBuilder();
            foreach (var paragraph in body.Elements<Paragraph>())
            {
                text.AppendLine(paragraph.InnerText);
            }
            
            return text.ToString();
        });
    }

    private async Task<string> ExtractTextFromExcelAsync(Stream excelStream)
    {
        return await Task.Run(() =>
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(excelStream);
            var text = new System.Text.StringBuilder();
            
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                text.AppendLine($"Sheet: {worksheet.Name}");
                
                var start = worksheet.Dimension?.Start;
                var end = worksheet.Dimension?.End;
                
                if (start != null && end != null)
                {
                    for (int row = start.Row; row <= end.Row; row++)
                    {
                        var rowData = new List<string>();
                        for (int col = start.Column; col <= end.Column; col++)
                        {
                            var cellValue = worksheet.Cells[row, col].Text;
                            if (!string.IsNullOrWhiteSpace(cellValue))
                                rowData.Add(cellValue);
                        }
                        if (rowData.Any())
                            text.AppendLine(string.Join(" | ", rowData));
                    }
                }
                text.AppendLine();
            }
            
            return text.ToString();
        });
    }

    private async Task<string> ExtractTextFromTextAsync(Stream textStream)
    {
        using var reader = new StreamReader(textStream);
        return await reader.ReadToEndAsync();
    }

    private async Task<string> ExtractTextFromCsvAsync(Stream csvStream)
    {
        return await Task.Run(() =>
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
            var text = new System.Text.StringBuilder();
            var records = csv.GetRecords<dynamic>().ToList();
            
            foreach (var record in records)
            {
                var values = ((IDictionary<string, object>)record).Values
                    .Where(v => v != null && !string.IsNullOrWhiteSpace(v.ToString()))
                    .Select(v => v.ToString());
                
                if (values.Any())
                    text.AppendLine(string.Join(" | ", values));
            }
            
            return text.ToString();
        });
    }

    private List<DocumentChunk> ChunkText(
        string text, 
        string documentId, 
        string fileName, 
        Dictionary<string, string> metadata)
    {
        var chunks = new List<DocumentChunk>();
        var chunkSize = _settings.ChunkSize;
        var overlap = _settings.ChunkOverlap;

        // Split by sentences to avoid breaking in the middle of sentences
        var sentences = text.Split(new[] { ". ", ".\n", ".\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new System.Text.StringBuilder();
        var chunkIndex = 0;

        foreach (var sentence in sentences)
        {
            if (currentChunk.Length + sentence.Length > chunkSize && currentChunk.Length > 0)
            {
                // Create chunk
                chunks.Add(new DocumentChunk
                {
                    Id = $"{documentId}_chunk_{chunkIndex}",
                    DocumentId = documentId,
                    FileName = fileName,
                    Content = currentChunk.ToString().Trim(),
                    ChunkIndex = chunkIndex,
                    Metadata = new Dictionary<string, string>(metadata)
                });

                chunkIndex++;

                // Keep overlap
                var words = currentChunk.ToString().Split(' ');
                var overlapWords = words.TakeLast(Math.Min(overlap / 5, words.Length)).ToArray();
                currentChunk.Clear();
                currentChunk.Append(string.Join(" ", overlapWords) + " ");
            }

            currentChunk.Append(sentence + ". ");
        }

        // Add the last chunk
        if (currentChunk.Length > 0)
        {
            chunks.Add(new DocumentChunk
            {
                Id = $"{documentId}_chunk_{chunkIndex}",
                DocumentId = documentId,
                FileName = fileName,
                Content = currentChunk.ToString().Trim(),
                ChunkIndex = chunkIndex,
                Metadata = new Dictionary<string, string>(metadata)
            });
        }

        return chunks;
    }
}
