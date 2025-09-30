using Microsoft.AspNetCore.Mvc;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Syncfusion.Pdf.Parsing;

namespace Hansika.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WordExportController : ControllerBase
    {
        public WordExportController()
        {
            // Syncfusion license is already set in Program.cs
        }

        [HttpGet]
        public IActionResult ExportWord()
        {
            try
            {
                // Check if a PDF has been uploaded
                if (string.IsNullOrEmpty(UploadController.UploadedFilePath))
                {
                    return BadRequest("No PDF available to export. Please upload a PDF first.");
                }

                string inputFile = UploadController.UploadedFilePath;
                
                if (!System.IO.File.Exists(inputFile))
                {
                    return BadRequest("PDF file not found. Please upload a PDF first.");
                }

                // Generate filename
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var baseFileName = Path.GetFileNameWithoutExtension(inputFile);
                string fileName = $"{baseFileName}_export_{timestamp}.docx";

                // Create Word document
                var wordBytes = CreateWordDocument(inputFile);

                return File(wordBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Word export failed: {ex.Message}" });
            }
        }

        private byte[] CreateWordDocument(string pdfFilePath)
        {
            using (var stream = new MemoryStream())
            {
                using (var wordDocument = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
                {
                    // Add a main document part
                    var mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    var body = mainPart.Document.AppendChild(new Body());

                    // Add title
                    var titleParagraph = new Paragraph();
                    var titleRun = new Run();
                    var titleText = new Text($"PDF Export - {Path.GetFileName(pdfFilePath)}");
                    titleRun.Append(titleText);
                    titleParagraph.Append(titleRun);
                    body.Append(titleParagraph);

                    // Add empty paragraph
                    body.Append(new Paragraph());

                    // Add export info
                    var infoParagraph = new Paragraph();
                    var infoRun = new Run();
                    var infoText = new Text($"Exported on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    infoRun.Append(infoText);
                    infoParagraph.Append(infoRun);
                    body.Append(infoParagraph);

                    // Add empty paragraph
                    body.Append(new Paragraph());

                    // Extract PDF content using Syncfusion
                    try
                    {
                        var pdfDocument = new PdfLoadedDocument(pdfFilePath);
                        var extractedText = "";
                        
                        // Extract text from all pages
                        for (int i = 0; i < pdfDocument.Pages.Count; i++)
                        {
                            var page = pdfDocument.Pages[i];
                            extractedText += page.ExtractText();
                            if (i < pdfDocument.Pages.Count - 1)
                            {
                                extractedText += "\n\n";
                            }
                        }
                        
                        pdfDocument.Close();
                        
                        if (!string.IsNullOrEmpty(extractedText))
                        {
                            // Split text into paragraphs and add to Word document
                            var paragraphs = extractedText.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            
                            foreach (var paragraphText in paragraphs)
                            {
                                if (!string.IsNullOrWhiteSpace(paragraphText))
                                {
                                    var contentParagraph = new Paragraph();
                                    var contentRun = new Run();
                                    var contentText = new Text(paragraphText.Trim());
                                    contentRun.Append(contentText);
                                    contentParagraph.Append(contentRun);
                                    body.Append(contentParagraph);
                                    
                                    // Add empty paragraph between content
                                    body.Append(new Paragraph());
                                }
                            }
                        }
                        else
                        {
                            // Fallback if no text extracted
                            var contentParagraph = new Paragraph();
                            var contentRun = new Run();
                            var contentText = new Text("No text content could be extracted from the PDF.");
                            contentRun.Append(contentText);
                            contentParagraph.Append(contentRun);
                            body.Append(contentParagraph);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Error handling for PDF extraction
                        var errorParagraph = new Paragraph();
                        var errorRun = new Run();
                        var errorText = new Text($"Error extracting PDF content: {ex.Message}");
                        errorRun.Append(errorText);
                        errorParagraph.Append(errorRun);
                        body.Append(errorParagraph);
                    }
                }

                return stream.ToArray();
            }
        }
    }
}




