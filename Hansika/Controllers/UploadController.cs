using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Syncfusion.Pdf.Parsing;
using System.Text.Json;
using System;

namespace Hansika.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        public static string UploadedFilePath { get; set; } = "";
        public static PdfDocumentInfo? DocumentInfo { get; set; }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                if (file.Length > 50 * 1024 * 1024) // 50MB limit
                    return BadRequest("File size exceeds 50MB limit");

                if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Only PDF files are allowed");

                // Ensure uploads directory exists
                var uploads = Path.Combine(Directory.GetCurrentDirectory()!, "Uploads");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                // Handle duplicate file names with length limit
                var originalFileName = file.FileName;
                var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
                var extension = Path.GetExtension(originalFileName);
                
                // Limit filename length to avoid Windows path length issues
                if (nameWithoutExt.Length > 100)
                {
                    nameWithoutExt = nameWithoutExt.Substring(0, 100);
                }
                
                var fileName = $"{nameWithoutExt}{extension}";
                var filePath = Path.Combine(uploads, fileName);
                var counter = 1;
                
                while (System.IO.File.Exists(filePath))
                {
                    // Use shorter counter format to avoid long filenames
                    var shortName = nameWithoutExt.Length > 80 ? nameWithoutExt.Substring(0, 80) : nameWithoutExt;
                    fileName = $"{shortName}_{counter}{extension}";
                    filePath = Path.Combine(uploads, fileName);
                    counter++;
                    
                    // Prevent infinite loop with very long names
                    if (counter > 9999) break;
                }

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Extract PDF metadata
                DocumentInfo = ExtractPdfMetadata(filePath);
                UploadedFilePath = filePath;
                
                // Create original file for annotation system
                CreateOriginalFile(filePath);

                return Ok(new
                {
                    message = "File uploaded successfully",
                    fileUrl = $"/uploads/{fileName}",
                    fileName = fileName,
                    fileSize = file.Length,
                    metadata = DocumentInfo,
                    uploadedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Upload failed: {ex.Message}" });
            }
        }

        [HttpGet("metadata")]
        public IActionResult GetMetadata()
        {
            if (DocumentInfo == null)
                return BadRequest("No PDF uploaded yet");

            return Ok(DocumentInfo);
        }

        [HttpPost("restore")]
        public IActionResult RestorePdfState([FromBody] RestorePdfRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FileUrl))
                    return BadRequest("File URL is required");

                // Extract filename from URL
                var fileName = Path.GetFileName(request.FileUrl);
                if (string.IsNullOrEmpty(fileName))
                    return BadRequest("Invalid file URL");

                // Check if file exists in uploads directory
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory()!, "Uploads");
                var filePath = Path.Combine(uploadsPath, fileName);
                
                if (!System.IO.File.Exists(filePath))
                    return BadRequest("File not found in uploads directory");

                // Restore the file path and metadata
                UploadedFilePath = filePath;
                DocumentInfo = ExtractPdfMetadata(filePath);

                return Ok(new
                {
                    success = true,
                    message = "PDF state restored successfully",
                    fileUrl = request.FileUrl,
                    fileName = fileName,
                    fileSize = new FileInfo(filePath).Length,
                    metadata = DocumentInfo,
                    restoredAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Restore failed: {ex.Message}" });
            }
        }

        [HttpPost("metadata")]
        public IActionResult UpdateMetadata([FromBody] PdfDocumentInfo metadata)
        {
            if (string.IsNullOrEmpty(UploadedFilePath) || !System.IO.File.Exists(UploadedFilePath))
                return BadRequest("No PDF uploaded yet");

            try
            {
                UpdatePdfMetadata(UploadedFilePath, metadata);
                DocumentInfo = metadata;
                
                return Ok(new { message = "Metadata updated successfully", metadata = metadata });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Failed to update metadata: {ex.Message}" });
            }
        }

        private PdfDocumentInfo ExtractPdfMetadata(string filePath)
        {
            try
            {
                using (var document = new PdfLoadedDocument(filePath))
                {
                    var info = document.DocumentInformation;
                    return new PdfDocumentInfo
                    {
                        Title = info.Title ?? "",
                        Author = info.Author ?? "",
                        Subject = info.Subject ?? "",
                        Keywords = info.Keywords ?? "",
                        Creator = info.Creator ?? "",
                        Producer = info.Producer ?? "",
                        CreationDate = info.CreationDate,
                        ModificationDate = info.ModificationDate,
                        PageCount = document.Pages.Count
                    };
                }
            }
            catch
            {
                return new PdfDocumentInfo
                {
                    Title = "",
                    Author = "",
                    Subject = "",
                    Keywords = "",
                    Creator = "",
                    Producer = "",
                    CreationDate = DateTime.Now,
                    ModificationDate = DateTime.Now,
                    PageCount = 0
                };
            }
        }

        private void CreateOriginalFile(string filePath)
        {
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var originalPath = Path.Combine(directory!, $"{fileName}_original.pdf");
                
                // Copy the uploaded file as the original (clean version)
                System.IO.File.Copy(filePath, originalPath, true);
                Console.WriteLine($"Original file created: {originalPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating original file: {ex.Message}");
            }
        }

        private void UpdatePdfMetadata(string filePath, PdfDocumentInfo metadata)
        {
            using (var document = new PdfLoadedDocument(filePath))
            {
                var info = document.DocumentInformation;
                info.Title = metadata.Title;
                info.Author = metadata.Author;
                info.Subject = metadata.Subject;
                info.Keywords = metadata.Keywords;
                info.Creator = metadata.Creator;
                info.Producer = metadata.Producer;
                
                document.Save(filePath);
            }
        }
    }

    public class PdfDocumentInfo
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Keywords { get; set; } = "";
        public string Creator { get; set; } = "";
        public string Producer { get; set; } = "";
        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }
        public int PageCount { get; set; }
    }

    public class RestorePdfRequest
    {
        public string? FileUrl { get; set; }
    }
}
