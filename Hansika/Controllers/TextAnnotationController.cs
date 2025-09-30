using Microsoft.AspNetCore.Mvc;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System.IO;
using System;
using System.Drawing;
using SyncfusionRectangleF = Syncfusion.Drawing.RectangleF;
using Hansika.Models;

namespace Hansika.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextAnnotationController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "TextAnnotation controller is working", timestamp = DateTime.Now });
        }

        [HttpGet("list")]
        public IActionResult GetAnnotations()
        {
            return Ok(new { 
                annotations = AnnotationList.Annotations,
                count = AnnotationList.Annotations.Count 
            });
        }

        [HttpGet("debug")]
        public IActionResult DebugInfo()
        {
            try
            {
                string inputFile = UploadController.UploadedFilePath;
                var originalFile = GetOriginalPdfPath(inputFile);
                
                return Ok(new {
                    inputFile = inputFile,
                    inputFileExists = System.IO.File.Exists(inputFile),
                    originalFile = originalFile,
                    originalFileExists = System.IO.File.Exists(originalFile),
                    annotationsCount = AnnotationList.Annotations.Count,
                    annotations = AnnotationList.Annotations.Select(a => new {
                        id = a.Id,
                        type = a.Type,
                        text = a.Text,
                        page = a.PageNumber,
                        position = new { x = a.X, y = a.Y }
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAnnotation(string id)
        {
            try
            {
                Console.WriteLine($"=== DELETE ANNOTATION REQUEST ===");
                Console.WriteLine($"Annotation ID: {id}");
                Console.WriteLine($"Total annotations before deletion: {AnnotationList.Annotations.Count}");
                
                var annotation = AnnotationList.Annotations.FirstOrDefault(a => a.Id == id);
                if (annotation == null)
                {
                    Console.WriteLine("Annotation not found in list");
                    return NotFound(new { error = "Annotation not found" });
                }

                Console.WriteLine($"Found annotation: {annotation.Type} - {annotation.Text}");
                Console.WriteLine($"Position: ({annotation.X}, {annotation.Y})");
                
                AnnotationList.Annotations.Remove(annotation);
                Console.WriteLine($"Total annotations after deletion: {AnnotationList.Annotations.Count}");
                
                // Re-render the PDF without the deleted annotation
                Console.WriteLine("Starting PDF re-render...");
                ReRenderPdf();
                Console.WriteLine("PDF re-render completed");
                
                return Ok(new { 
                    success = true, 
                    message = "Annotation deleted successfully",
                    fileUrl = $"/uploads/{Path.GetFileName(UploadController.UploadedFilePath)}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting annotation: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = $"Failed to delete annotation: {ex.Message}" });
            }
        }

        [HttpPost("clear-all")]
        public IActionResult ClearAllAnnotations()
        {
            try
            {
                AnnotationList.Annotations.Clear();
                
                // Re-render the PDF without any annotations
                ReRenderPdf();
                
                return Ok(new { 
                    success = true, 
                    message = "All annotations cleared successfully",
                    fileUrl = $"/uploads/{Path.GetFileName(UploadController.UploadedFilePath)}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Failed to clear annotations: {ex.Message}" });
            }
        }

        private void ReRenderPdf()
        {
            try
            {
                string inputFile = UploadController.UploadedFilePath;
                if (string.IsNullOrEmpty(inputFile) || !System.IO.File.Exists(inputFile))
                {
                    Console.WriteLine("Input file is null or doesn't exist");
                    return;
                }

                // Get the original PDF (without annotations)
                var originalFile = GetOriginalPdfPath(inputFile);
                Console.WriteLine($"Looking for original file: {originalFile}");
                
                if (!System.IO.File.Exists(originalFile))
                {
                    Console.WriteLine("No original file found - this means the upload didn't create it properly");
                    Console.WriteLine("Creating original file now from current file");
                    System.IO.File.Copy(inputFile, originalFile, true);
                }

                Console.WriteLine($"Re-rendering PDF with {AnnotationList.Annotations.Count} annotations");
                Console.WriteLine($"Original file: {originalFile}");
                Console.WriteLine($"Output file: {inputFile}");

                // Load the original PDF (clean version)
                using (var fileStream = new FileStream(originalFile, FileMode.Open, FileAccess.Read))
                using (var pdfDocument = new PdfLoadedDocument(fileStream))
                {
                    Console.WriteLine($"Loaded PDF with {pdfDocument.Pages.Count} pages");
                    
                    // If no annotations remain, just save the clean original
                    if (AnnotationList.Annotations.Count == 0)
                    {
                        Console.WriteLine("No annotations remaining, saving clean PDF");
                        pdfDocument.Save(inputFile);
                        Console.WriteLine("Clean PDF saved successfully");
                        return;
                    }
                    
                    // Re-render all remaining annotations
                    foreach (var annotation in AnnotationList.Annotations)
                    {
                        Console.WriteLine($"Processing annotation: {annotation.Type} - {annotation.Text}");
                        Console.WriteLine($"Page: {annotation.PageNumber}, Position: ({annotation.X}, {annotation.Y})");
                        
                        if (annotation.PageNumber >= 1 && annotation.PageNumber <= pdfDocument.Pages.Count)
                        {
                            var page = pdfDocument.Pages[annotation.PageNumber - 1];
                            Console.WriteLine($"Rendering annotation on page {annotation.PageNumber}");
                            RenderAnnotation(page, annotation);
                        }
                        else
                        {
                            Console.WriteLine($"Invalid page number: {annotation.PageNumber}");
                        }
                    }

                    // Save the updated PDF
                    Console.WriteLine($"Saving PDF to: {inputFile}");
                    pdfDocument.Save(inputFile);
                    Console.WriteLine("PDF re-rendered and saved successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error re-rendering PDF: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private string GetOriginalPdfPath(string currentPath)
        {
            var directory = Path.GetDirectoryName(currentPath);
            var fileName = Path.GetFileNameWithoutExtension(currentPath);
            return Path.Combine(directory!, $"{fileName}_original.pdf");
        }

        private void RenderAnnotation(Syncfusion.Pdf.PdfPageBase page, Annotation annotation)
        {
            var graphics = page.Graphics;
            
            switch (annotation.Type)
            {
                case "sticky-note":
                    RenderStickyNote(graphics, annotation);
                    break;
                case "free-text":
                    RenderFreeText(graphics, annotation);
                    break;
                case "callout":
                    RenderCallout(graphics, annotation);
                    break;
            }
        }

        private void RenderStickyNote(Syncfusion.Pdf.Graphics.PdfGraphics graphics, Annotation annotation)
        {
            var stickyNoteBrush = new PdfSolidBrush(ConvertHexToPdfColor(annotation.Color));
            var font = new PdfStandardFont(PdfFontFamily.Helvetica, annotation.FontSize);
            var textSize = font.MeasureString(annotation.Text);
            
            var bounds = new SyncfusionRectangleF(
                annotation.X,
                annotation.Y,
                Math.Max(textSize.Width + 10, 50),
                Math.Max(textSize.Height + 10, 20)
            );
            
            graphics.DrawRectangle(stickyNoteBrush, bounds);
            
            var textBrush = new PdfSolidBrush(new PdfColor(0, 0, 0));
            graphics.DrawString(annotation.Text, font, textBrush, bounds.X + 5, bounds.Y + 5);
        }

        private void RenderFreeText(Syncfusion.Pdf.Graphics.PdfGraphics graphics, Annotation annotation)
        {
            var backgroundColorBrush = new PdfSolidBrush(ConvertHexToPdfColor(annotation.BackgroundColor));
            var borderBrush = new PdfSolidBrush(ConvertHexToPdfColor(annotation.BorderColor));
            var textBrush = new PdfSolidBrush(ConvertHexToPdfColor(annotation.TextColor));
            var font = new PdfStandardFont(PdfFontFamily.Helvetica, annotation.FontSize);
            
            var bounds = new SyncfusionRectangleF(
                annotation.X,
                annotation.Y,
                annotation.Width,
                annotation.Height
            );
            
            graphics.DrawRectangle(backgroundColorBrush, bounds);
            graphics.DrawRectangle(borderBrush, bounds);
            graphics.DrawString(annotation.Text, font, textBrush, bounds.X + 5, bounds.Y + 5);
        }

        private void RenderCallout(Syncfusion.Pdf.Graphics.PdfGraphics graphics, Annotation annotation)
        {
            var calloutBrush = new PdfSolidBrush(ConvertHexToPdfColor(annotation.Color));
            var font = new PdfStandardFont(PdfFontFamily.Helvetica, annotation.FontSize);
            var textSize = font.MeasureString(annotation.Text);
            
            var bounds = new SyncfusionRectangleF(
                annotation.X,
                annotation.Y,
                Math.Max(textSize.Width + 20, annotation.Width),
                Math.Max(textSize.Height + 20, annotation.Height)
            );
            
            graphics.DrawRectangle(calloutBrush, bounds);
            
            var textBrush = new PdfSolidBrush(new PdfColor(0, 0, 0));
            graphics.DrawString(annotation.Text, font, textBrush, bounds.X + 10, bounds.Y + 10);
        }
        [HttpPost("sticky-note")]
        public IActionResult AddStickyNote([FromBody] StickyNoteRequest request)
        {
            try
            {
                Console.WriteLine($"Sticky note request: X={request.X}, Y={request.Y}, Text={request.Text}");
                
                string inputFile = UploadController.UploadedFilePath;
                if (string.IsNullOrEmpty(inputFile) || !System.IO.File.Exists(inputFile))
                    return BadRequest(new { error = "No PDF uploaded yet. Please upload a PDF first.", uploadedFilePath = inputFile });

                var outputPath = GetOutputPath(inputFile, "sticky_note");
                
                // Load PDF using Syncfusion
                using (var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (var pdfDocument = new PdfLoadedDocument(fileStream))
                {
                    if (request.PageNumber < 1 || request.PageNumber > pdfDocument.Pages.Count)
                        return BadRequest("Invalid page number.");

                    var page = pdfDocument.Pages[request.PageNumber - 1];
                    
                    // Create sticky note annotation using Syncfusion graphics
                    var graphics = page.Graphics;
                    var stickyNoteBrush = new PdfSolidBrush(ConvertHexToPdfColor(request.Color ?? "#FFFF00"));
                    
                    // Use the actual text from the request
                    var textToShow = !string.IsNullOrEmpty(request.Text) ? request.Text : "Note";
                    
                    // Calculate text size to determine bounds
                    var font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                    var textSize = font.MeasureString(textToShow);
                    
                    // Get page dimensions
                    var pageSize = page.Size;
                    var pageHeight = pageSize.Height;
                    var pageWidth = pageSize.Width;
                    
                    Console.WriteLine($"Page dimensions: {pageWidth}x{pageHeight}");
                    Console.WriteLine($"Received coordinates: X={request.X}, Y={request.Y}");
                    
                    // Convert screen coordinates to PDF coordinates
                    // Screen coordinates are relative to the iframe, we need to scale them to PDF dimensions
                    var iframeWidth = request.IframeWidth ?? 800;
                    var iframeHeight = request.IframeHeight ?? 700;
                    
                    Console.WriteLine($"Iframe dimensions: {iframeWidth}x{iframeHeight}");
                    
                    // Calculate scale factors
                    var scaleX = pageWidth / iframeWidth;
                    var scaleY = pageHeight / iframeHeight;
                    
                    Console.WriteLine($"Scale factors: X={scaleX:F2}, Y={scaleY:F2}");
                    
                    // Try different scaling approaches
                    // Approach 1: Direct mapping (current)
                    var pdfX1 = request.X ?? 0;
                    var pdfY1 = pageHeight - (request.Y ?? 0) - 20;
                    
                    // Approach 2: Scale by 0.75 (common PDF display scale)
                    var pdfX2 = (request.X ?? 0) * 0.75f;
                    var pdfY2 = pageHeight - ((request.Y ?? 0) * 0.75f) - 20;
                    
                    // Approach 3: Scale by iframe ratio
                    var pdfX3 = (request.X ?? 0) * scaleX;
                    var pdfY3 = pageHeight - ((request.Y ?? 0) * scaleY) - 20;
                    
                    // Approach 4: Scale by page ratio (assuming iframe shows full page)
                    var pdfX4 = (request.X ?? 0) * (pageWidth / iframeWidth);
                    var pdfY4 = pageHeight - ((request.Y ?? 0) * (pageHeight / iframeHeight)) - 20;
                    
                    Console.WriteLine($"Approach 1 (Direct): X={pdfX1:F2}, Y={pdfY1:F2}");
                    Console.WriteLine($"Approach 2 (0.75 scale): X={pdfX2:F2}, Y={pdfY2:F2}");
                    Console.WriteLine($"Approach 3 (Scale factors): X={pdfX3:F2}, Y={pdfY3:F2}");
                    Console.WriteLine($"Approach 4 (Page ratio): X={pdfX4:F2}, Y={pdfY4:F2}");
                    
                    // Try a completely different approach
                    // Assume the iframe shows the PDF at 100% zoom and calculate the actual scale
                    var actualScaleX = pageWidth / iframeWidth;
                    var actualScaleY = pageHeight / iframeHeight;
                    
                    Console.WriteLine($"Actual scale factors: X={actualScaleX:F3}, Y={actualScaleY:F3}");
                    
                    // Coordinate system is correct! Now use proper scaling
                    var pdfX = (request.X ?? 0) * actualScaleX;
                    var pdfY = (request.Y ?? 0) * actualScaleY;
                    
                    // Adjust Y position to account for annotation height
                    // The annotation appears below the click because we need to subtract the height
                    var annotationHeight = Math.Max(textSize.Height + 10, 20);
                    pdfY = pdfY - annotationHeight;
                    
                    Console.WriteLine($"Received coordinates: X={request.X}, Y={request.Y}");
                    Console.WriteLine($"Scale factors: X={actualScaleX:F3}, Y={actualScaleY:F3}");
                    Console.WriteLine($"Annotation height: {annotationHeight:F2}");
                    Console.WriteLine($"Adjusted coordinates: X={pdfX:F2}, Y={pdfY:F2}");
                    
                    var bounds = new SyncfusionRectangleF(
                        pdfX,
                        pdfY,
                        Math.Max(textSize.Width + 10, 50),
                        Math.Max(textSize.Height + 10, 20)
                    );
                    
                    Console.WriteLine($"Page size: {pageSize.Width}x{pageSize.Height}");
                    Console.WriteLine($"Original coordinates: X={request.X}, Y={request.Y}");
                    Console.WriteLine($"Converted coordinates: X={pdfX}, Y={pdfY}");
                    Console.WriteLine($"Drawing sticky note at: X={bounds.X}, Y={bounds.Y}, Width={bounds.Width}, Height={bounds.Height}");
                    
                    // Create annotation object and store it
                    var annotation = new Annotation
                    {
                        Type = "sticky-note",
                        Text = textToShow,
                        PageNumber = request.PageNumber,
                        X = pdfX,
                        Y = pdfY,
                        Width = bounds.Width,
                        Height = bounds.Height,
                        Color = request.Color ?? "#FFFF00",
                        FontSize = 12,
                        Subject = "Sticky Note"
                    };
                    
                    AnnotationList.Annotations.Add(annotation);
                    
                    // Draw sticky note as a rectangle
                    graphics.DrawRectangle(stickyNoteBrush, bounds);
                    
                    // Add text
                    var textBrush = new PdfSolidBrush(new PdfColor(0, 0, 0));
                    graphics.DrawString(textToShow, font, textBrush, bounds.X + 5, bounds.Y + 5);
                    
                    // Save the document
                    pdfDocument.Save(outputPath);
                }
                
                // Update the uploaded file path to the new annotated version
                UploadController.UploadedFilePath = outputPath;
                
                return Ok(new { 
                    success = true, 
                    message = "Sticky note added successfully",
                    fileUrl = $"/uploads/{Path.GetFileName(outputPath)}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = $"Sticky note failed: {ex.Message}",
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpPost("free-text")]
        public IActionResult AddFreeText([FromBody] FreeTextRequest request)
        {
            try
            {
                string inputFile = UploadController.UploadedFilePath;
                if (string.IsNullOrEmpty(inputFile) || !System.IO.File.Exists(inputFile))
                    return BadRequest(new { error = "No PDF uploaded yet. Please upload a PDF first.", uploadedFilePath = inputFile });

                var outputPath = GetOutputPath(inputFile, "free_text");
                
                // Load PDF using Syncfusion
                using (var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (var pdfDocument = new PdfLoadedDocument(fileStream))
                {
                    if (request.PageNumber < 1 || request.PageNumber > pdfDocument.Pages.Count)
                        return BadRequest("Invalid page number.");

                    var page = pdfDocument.Pages[request.PageNumber - 1];
                    
                    // Create free text annotation using Syncfusion graphics
                    var graphics = page.Graphics;
                    var backgroundBrush = new PdfSolidBrush(ConvertHexToPdfColor(request.BackgroundColor ?? "#FFFFFF"));
                    var textBrush = new PdfSolidBrush(ConvertHexToPdfColor(request.TextColor ?? "#000000"));
                    
                    // Get page dimensions and convert coordinates
                    var pageSize = page.Size;
                    var pageHeight = pageSize.Height;
                    var pageWidth = pageSize.Width;
                    
                    // Convert screen coordinates to PDF coordinates
                    var iframeWidth = request.IframeWidth ?? 800;
                    var iframeHeight = request.IframeHeight ?? 700;
                    
                    Console.WriteLine($"Free text - Iframe dimensions: {iframeWidth}x{iframeHeight}");
                    
                    // Use the same proper scaling approach as sticky notes
                    var actualScaleX = pageWidth / iframeWidth;
                    var actualScaleY = pageHeight / iframeHeight;
                    var pdfX = (request.X ?? 0) * actualScaleX;
                    var pdfY = (request.Y ?? 0) * actualScaleY;
                    
                    // Adjust Y position to account for annotation height
                    var annotationHeight = request.Height ?? 30;
                    pdfY = pdfY - annotationHeight;
                    
                    var bounds = new SyncfusionRectangleF(
                        pdfX,
                        pdfY,
                        request.Width ?? 100,
                        request.Height ?? 30
                    );
                    
                    Console.WriteLine($"Free text - Original: X={request.X}, Y={request.Y}");
                    Console.WriteLine($"Free text - Converted: X={pdfX}, Y={pdfY}");
                    
                    // Create annotation object and store it
                    var annotation = new Annotation
                    {
                        Type = "free-text",
                        Text = request.Text ?? "Free text",
                        PageNumber = request.PageNumber,
                        X = pdfX,
                        Y = pdfY,
                        Width = bounds.Width,
                        Height = bounds.Height,
                        FontSize = request.FontSize ?? 12,
                        TextColor = request.TextColor,
                        BackgroundColor = request.BackgroundColor,
                        BorderColor = request.BorderColor
                    };
                    
                    AnnotationList.Annotations.Add(annotation);
                    
                    // Draw background rectangle
                    graphics.DrawRectangle(backgroundBrush, bounds);
                    
                    // Draw text
                    var font = new PdfStandardFont(PdfFontFamily.Helvetica, request.FontSize ?? 12);
                    graphics.DrawString(request.Text ?? "Free text", font, textBrush, bounds.X + 5, bounds.Y + 5);
                    
                    // Save the document
                    pdfDocument.Save(outputPath);
                }
                
                // Update the uploaded file path to the new annotated version
                UploadController.UploadedFilePath = outputPath;
                
                return Ok(new { 
                    success = true, 
                    message = "Free text added successfully",
                    fileUrl = $"/uploads/{Path.GetFileName(outputPath)}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Free text failed: {ex.Message}" });
            }
        }

        [HttpPost("callout")]
        public IActionResult AddCallout([FromBody] CalloutRequest request)
        {
            try
            {
                string inputFile = UploadController.UploadedFilePath;
                if (string.IsNullOrEmpty(inputFile) || !System.IO.File.Exists(inputFile))
                    return BadRequest(new { error = "No PDF uploaded yet. Please upload a PDF first.", uploadedFilePath = inputFile });

                var outputPath = GetOutputPath(inputFile, "callout");
                
                // Load PDF using Syncfusion
                using (var fileStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                using (var pdfDocument = new PdfLoadedDocument(fileStream))
                {
                    if (request.PageNumber < 1 || request.PageNumber > pdfDocument.Pages.Count)
                        return BadRequest("Invalid page number.");

                    var page = pdfDocument.Pages[request.PageNumber - 1];
                    
                    // Create callout annotation using Syncfusion graphics
                    var graphics = page.Graphics;
                    var calloutBrush = new PdfSolidBrush(ConvertHexToPdfColor(request.Color ?? "#FFFF00"));
                    var textBrush = new PdfSolidBrush(new PdfColor(0, 0, 0));
                    var bounds = new SyncfusionRectangleF(
                        request.X ?? 0,
                        request.Y ?? 0,
                        request.Width ?? 100,
                        request.Height ?? 30
                    );
                    
                    // Create annotation object and store it
                    var annotation = new Annotation
                    {
                        Type = "callout",
                        Text = request.Text ?? "Callout text",
                        PageNumber = request.PageNumber,
                        X = request.X ?? 0,
                        Y = request.Y ?? 0,
                        Width = bounds.Width,
                        Height = bounds.Height,
                        Color = request.Color ?? "#FFFF00",
                        Subject = "Callout"
                    };
                    
                    AnnotationList.Annotations.Add(annotation);
                    
                    // Draw callout background
                    graphics.DrawRectangle(calloutBrush, bounds);
                    
                    // Draw callout text
                    var font = new PdfStandardFont(PdfFontFamily.Helvetica, 10);
                    graphics.DrawString(request.Text ?? "Callout text", font, textBrush, bounds.X + 5, bounds.Y + 5);
                    
                    // Save the document
                    pdfDocument.Save(outputPath);
                }
                
                // Update the uploaded file path to the new annotated version
                UploadController.UploadedFilePath = outputPath;
                
                return Ok(new { 
                    success = true, 
                    message = "Callout added successfully",
                    fileUrl = $"/uploads/{Path.GetFileName(outputPath)}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Callout failed: {ex.Message}" });
            }
        }

        private string GetOutputPath(string inputFile, string annotationType)
        {
            var directory = Path.GetDirectoryName(inputFile);
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            var extension = Path.GetExtension(inputFile);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            // Limit filename length to prevent Windows path length issues
            if (fileName.Length > 50)
            {
                fileName = fileName.Substring(0, 50);
            }
            
            // Create a shorter, unique filename
            var shortFileName = $"{fileName}_{annotationType}_{timestamp}";
            
            // Ensure total path length doesn't exceed Windows limits
            var fullPath = Path.Combine(directory!, $"{shortFileName}{extension}");
            
            // If still too long, use just the timestamp
            if (fullPath.Length > 200)
            {
                shortFileName = $"{annotationType}_{timestamp}";
                fullPath = Path.Combine(directory!, $"{shortFileName}{extension}");
            }
            
            return fullPath;
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

    public class StickyNoteRequest
    {
        public string? Text { get; set; }
        public string? Subject { get; set; }
        public int PageNumber { get; set; } = 1;
        public float? X { get; set; }
        public float? Y { get; set; }
        public string? Color { get; set; }
        public float? IframeWidth { get; set; }
        public float? IframeHeight { get; set; }
    }

    public class FreeTextRequest
    {
        public string? Text { get; set; }
        public int PageNumber { get; set; } = 1;
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public int? FontSize { get; set; }
        public string? TextColor { get; set; }
        public string? BorderColor { get; set; }
        public string? BackgroundColor { get; set; }
        public float? IframeWidth { get; set; }
        public float? IframeHeight { get; set; }
    }

    public class CalloutRequest
    {
        public string? Text { get; set; }
        public string? Subject { get; set; }
        public int PageNumber { get; set; } = 1;
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public string? Color { get; set; }
    }
}
