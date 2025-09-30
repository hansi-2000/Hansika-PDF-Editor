using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Hansika.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfExportController : ControllerBase
    {
        [HttpGet]
        public IActionResult ExportPdf()
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
                string fileName = $"{baseFileName}_export_{timestamp}.pdf";

                // Export PDF
                return ExportAsPdf(inputFile, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"PDF export failed: {ex.Message}" });
            }
        }

        private IActionResult ExportAsPdf(string inputFile, string fileName)
        {
            try
            {
                var fileBytes = System.IO.File.ReadAllBytes(inputFile);
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"PDF export failed: {ex.Message}" });
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

