using Microsoft.AspNetCore.Mvc;
using IronPdf;
using System.IO;
using System.IO.Compression;

namespace Hansika.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesExportController : ControllerBase
    {
        [HttpGet]
        public IActionResult ExportImages()
        {
            try
            {
                // Determine input file path
                string inputFile = GetCurrentFilePath();
                if (string.IsNullOrEmpty(inputFile) || !System.IO.File.Exists(inputFile))
                    return BadRequest("No PDF available to export.");

                // Generate unique filename with timestamp
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var baseFileName = Path.GetFileNameWithoutExtension(inputFile);
                string fileName = $"{baseFileName}_images_{timestamp}.zip";

                // Export Images
                return ExportAsImages(inputFile, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Images export failed: {ex.Message}" });
            }
        }

        private IActionResult ExportAsImages(string inputFile, string fileName)
        {
            try
            {
                // Create a temporary directory for images
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);
                
                try
                {
                    // Load PDF document using IronPDF
                    var pdfDocument = IronPdf.PdfDocument.FromFile(inputFile);
                    var pageCount = pdfDocument.PageCount;
                    
                    // Convert each page to image
                    for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                    {
                        try
                        {
                            // Convert page to image
                            var imageFileName = Path.Combine(tempDir, $"page_{pageIndex + 1:D3}.png");
                            pdfDocument.RasterizeToImageFiles(imageFileName, new int[] { pageIndex });
                        }
                        catch (Exception ex)
                        {
                            // If page conversion fails, create error file
                            var errorFileName = Path.Combine(tempDir, $"page_{pageIndex + 1:D3}_error.txt");
                            System.IO.File.WriteAllText(errorFileName, $"Error converting page {pageIndex + 1} to image: {ex.Message}");
                        }
                    }
                    
                    // Create ZIP file from the temporary directory
                    var zipBytes = CreateZipFromDirectory(tempDir);
                    
                    return File(zipBytes, "application/zip", fileName);
                }
                finally
                {
                    // Clean up temporary directory
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Images export failed: {ex.Message}" });
            }
        }
        
        private byte[] CreateZipFromDirectory(string directoryPath)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // Add all files from the directory to the ZIP
                    var files = Directory.GetFiles(directoryPath);
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        var entry = archive.CreateEntry(fileName);
                        
                        using (var entryStream = entry.Open())
                        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
                
                return memoryStream.ToArray();
            }
        }

        private string GetCurrentFilePath()
        {
            // Always use the latest uploaded PDF file
            if (string.IsNullOrEmpty(UploadController.UploadedFilePath) || !System.IO.File.Exists(UploadController.UploadedFilePath))
                return "";
            
            // Return the current uploaded file (latest PDF)
            return UploadController.UploadedFilePath;
        }
    }
}
