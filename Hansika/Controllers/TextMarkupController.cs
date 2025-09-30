using Microsoft.AspNetCore.Mvc;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System.IO;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using SyncfusionRectangleF = Syncfusion.Drawing.RectangleF;

namespace Hansika.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextMarkupController : ControllerBase
    {
        [HttpPost("highlight")]
        public IActionResult AddHighlight([FromBody] TextMarkupRequest request)
        {
            try
            {
                string inputFile = UploadController.UploadedFilePath;
                if (string.IsNullOrEmpty(inputFile) || !System.IO.File.Exists(inputFile))
                    return BadRequest(new { error = "No PDF uploaded yet. Please upload a PDF first.", uploadedFilePath = inputFile });

                var outputPath = GetOutputPath(inputFile, "highlighted");
                
                // Load PDF using Syncfusion
                using (var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (var pdfDocument = new PdfLoadedDocument(fileStream))
                {
                    if (request.PageNumber < 1 || request.PageNumber > pdfDocument.Pages.Count)
                        return BadRequest("Invalid page number.");

                    var page = pdfDocument.Pages[request.PageNumber - 1];
                    
                    // Find text in PDF and apply highlight
                    var textToFind = request.Text ?? "sample text";
                    var textBounds = FindTextInPage((PdfLoadedPage)page, textToFind);
                    
                    if (textBounds.Count > 0)
                    {
                        var graphics = page.Graphics;
                        var highlightBrush = new PdfSolidBrush(ConvertHexToPdfColor(request.Color ?? "#FFFF00"));
                        
                        // Apply highlight to all found text instances
                        foreach (var bounds in textBounds)
                        {
                            graphics.DrawRectangle(highlightBrush, bounds);
                        }
                    }
                    else
                    {
                        // If text not found, apply at specified coordinates
                        var graphics = page.Graphics;
                        var highlightBrush = new PdfSolidBrush(ConvertHexToPdfColor(request.Color ?? "#FFFF00"));
                        var bounds = new SyncfusionRectangleF(
                            request.X ?? 0,
                            request.Y ?? 0,
                            request.Width ?? 100,
                            request.Height ?? 20
                        );
                        graphics.DrawRectangle(highlightBrush, bounds);
                    }
                    
                    // Graphics are already drawn on the page
                    
                    // Save the document
                    pdfDocument.Save(outputPath);
                }
                
                // Update the uploaded file path to the new annotated version
                UploadController.UploadedFilePath = outputPath;
                
                return Ok(new { 
                    success = true, 
                    message = "Text highlighted successfully",
                    fileUrl = $"/uploads/{Path.GetFileName(outputPath)}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = $"Highlight failed: {ex.Message}",
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("underline")]
        public IActionResult AddUnderline([FromBody] TextMarkupRequest request)
        {
            try
            {
                string inputFile = UploadController.UploadedFilePath;
                if (string.IsNullOrEmpty(inputFile) || !System.IO.File.Exists(inputFile))
                    return BadRequest(new { error = "No PDF uploaded yet. Please upload a PDF first.", uploadedFilePath = inputFile });

                var outputPath = GetOutputPath(inputFile, "underlined");
                
                // Load PDF using Syncfusion
                using (var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (var pdfDocument = new PdfLoadedDocument(fileStream))
                {
                    if (request.PageNumber < 1 || request.PageNumber > pdfDocument.Pages.Count)
                        return BadRequest("Invalid page number.");

                    var page = pdfDocument.Pages[request.PageNumber - 1];
                    
                    // Find text in PDF and apply underline
                    var textToFind = request.Text ?? "sample text";
                    var textBounds = FindTextInPage((PdfLoadedPage)page, textToFind);
                    
                    if (textBounds.Count > 0)
                    {
                        var graphics = page.Graphics;
                        var underlinePen = new PdfPen(ConvertHexToPdfColor(request.Color ?? "#0000FF"), 2);
                        
                        // Apply underline to all found text instances
                        foreach (var bounds in textBounds)
                        {
                            graphics.DrawLine(underlinePen, bounds.X, bounds.Y + bounds.Height - 2, bounds.X + bounds.Width, bounds.Y + bounds.Height - 2);
                        }
                    }
                    else
                    {
                        // If text not found, apply at specified coordinates
                        var graphics = page.Graphics;
                        var underlinePen = new PdfPen(ConvertHexToPdfColor(request.Color ?? "#0000FF"), 2);
                        var bounds = new SyncfusionRectangleF(
                            request.X ?? 0,
                            request.Y ?? 0,
                            request.Width ?? 100,
                            request.Height ?? 20
                        );
                        graphics.DrawLine(underlinePen, bounds.X, bounds.Y + bounds.Height - 2, bounds.X + bounds.Width, bounds.Y + bounds.Height - 2);
                    }
                    
                    // Save the document
                    pdfDocument.Save(outputPath);
                }
                
                // Update the uploaded file path to the new annotated version
                UploadController.UploadedFilePath = outputPath;
                
                return Ok(new { 
                    success = true, 
                    message = "Text underlined successfully",
                    fileUrl = $"/uploads/{Path.GetFileName(outputPath)}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Underline failed: {ex.Message}" });
            }
        }

        [HttpPost("strikethrough")]
        public IActionResult AddStrikethrough([FromBody] TextMarkupRequest request)
        {
            try
            {
                string inputFile = UploadController.UploadedFilePath;
                if (string.IsNullOrEmpty(inputFile) || !System.IO.File.Exists(inputFile))
                    return BadRequest(new { error = "No PDF uploaded yet. Please upload a PDF first.", uploadedFilePath = inputFile });

                var outputPath = GetOutputPath(inputFile, "strikethrough");
                
                // Load PDF using Syncfusion
                using (var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (var pdfDocument = new PdfLoadedDocument(fileStream))
                {
                    if (request.PageNumber < 1 || request.PageNumber > pdfDocument.Pages.Count)
                        return BadRequest("Invalid page number.");

                    var page = pdfDocument.Pages[request.PageNumber - 1];
                    
                    // Find text in PDF and apply strikethrough
                    var textToFind = request.Text ?? "sample text";
                    var textBounds = FindTextInPage((PdfLoadedPage)page, textToFind);
                    
                    if (textBounds.Count > 0)
                    {
                        var graphics = page.Graphics;
                        var strikethroughPen = new PdfPen(ConvertHexToPdfColor(request.Color ?? "#FF0000"), 2);
                        
                        // Apply strikethrough to all found text instances
                        foreach (var bounds in textBounds)
                        {
                            graphics.DrawLine(strikethroughPen, bounds.X, bounds.Y + bounds.Height / 2, bounds.X + bounds.Width, bounds.Y + bounds.Height / 2);
                        }
                    }
                    else
                    {
                        // If text not found, apply at specified coordinates
                        var graphics = page.Graphics;
                        var strikethroughPen = new PdfPen(ConvertHexToPdfColor(request.Color ?? "#FF0000"), 2);
                        var bounds = new SyncfusionRectangleF(
                            request.X ?? 0,
                            request.Y ?? 0,
                            request.Width ?? 100,
                            request.Height ?? 20
                        );
                        graphics.DrawLine(strikethroughPen, bounds.X, bounds.Y + bounds.Height / 2, bounds.X + bounds.Width, bounds.Y + bounds.Height / 2);
                    }
                    
                    // Save the document
                    pdfDocument.Save(outputPath);
                }
                
                // Update the uploaded file path to the new annotated version
                UploadController.UploadedFilePath = outputPath;
                
                return Ok(new { 
                    success = true, 
                    message = "Text strikethrough added successfully",
                    fileUrl = $"/uploads/{Path.GetFileName(outputPath)}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Strikethrough failed: {ex.Message}" });
            }
        }

        private string GetOutputPath(string inputFile, string annotationType)
        {
            var directory = Path.GetDirectoryName(inputFile);
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            var extension = Path.GetExtension(inputFile);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            return Path.Combine(directory!, $"{fileName}_{annotationType}_{timestamp}{extension}");
        }

        private List<SyncfusionRectangleF> FindTextInPage(PdfLoadedPage page, string searchText)
        {
            var textBounds = new List<SyncfusionRectangleF>();
            
            try
            {
                // For now, return empty list - text search will be implemented later
                // This allows the annotation to fall back to coordinate-based positioning
                Console.WriteLine($"Text search for '{searchText}' - returning empty bounds for coordinate fallback");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Text search failed: {ex.Message}");
            }
            
            return textBounds;
        }

        private PdfColor ConvertHexToPdfColor(string hexColor)
        {
            hexColor = hexColor.Replace("#", "");
            if (hexColor.Length == 6)
            {
                var r = (byte)Convert.ToInt32(hexColor.Substring(0, 2), 16);
                var g = (byte)Convert.ToInt32(hexColor.Substring(2, 2), 16);
                var b = (byte)Convert.ToInt32(hexColor.Substring(4, 2), 16);
                return new PdfColor(r, g, b);
            }
            return new PdfColor(0, 0, 0);
        }
    }

    public class TextMarkupRequest
    {
        public string? Text { get; set; }
        public int PageNumber { get; set; } = 1;
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public string? Color { get; set; }
    }
}
