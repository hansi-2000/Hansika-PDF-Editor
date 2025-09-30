using Microsoft.AspNetCore.Mvc;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Pdf.Parsing;
using Syncfusion.PdfToImageConverter;
using System.IO;
using System.IO.Compression;
using System.Drawing.Imaging;
using System;

namespace Hansika.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        [HttpGet("{format}")]
        public IActionResult Export(string format)
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
                
                string fileName = format.ToLower() switch
                {
                    "pdf" => $"{baseFileName}_export_{timestamp}.pdf",
                    "docx" => $"{baseFileName}_export_{timestamp}.docx",
                    "images" => $"{baseFileName}_images_{timestamp}.zip",
                    _ => throw new ArgumentException("Unknown format")
                };

                // Export based on format
                return format.ToLower() switch
                {
                    "pdf" => ExportAsPdf(inputFile, fileName),
                    "docx" => ExportAsDocx(inputFile, fileName),
                    "images" => ExportAsImages(inputFile, fileName),
                    _ => BadRequest("Unknown format")
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Export failed: {ex.Message}" });
            }
        }

        [HttpPost("custom")]
        public IActionResult CustomExport([FromBody] CustomExportRequest request)
        {
            try
            {
                string inputFile = GetCurrentFilePath();
                if (string.IsNullOrEmpty(inputFile) || !System.IO.File.Exists(inputFile))
                    return BadRequest("No PDF available to export.");

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var baseFileName = Path.GetFileNameWithoutExtension(inputFile);
                
                string fileName = $"{baseFileName}_{request.Format}_{timestamp}.{GetFileExtension(request.Format)}";

                return request.Format.ToLower() switch
                {
                    "pdf" => ExportAsPdf(inputFile, fileName),
                    "docx" => ExportAsDocx(inputFile, fileName),
                    "images" => ExportAsImages(inputFile, fileName),
                    _ => BadRequest("Unknown format")
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Custom export failed: {ex.Message}" });
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

        private IActionResult ExportAsDocx(string inputFile, string fileName)
        {
            try
            {
                // Create Word document
                WordDocument wordDoc = new WordDocument();
                var section = wordDoc.AddSection();
                
                // Add document title
                var titleParagraph = section.AddParagraph();
                var titleText = titleParagraph.AppendText($"PDF to Word Conversion - {Path.GetFileName(inputFile)}");
                titleText.CharacterFormat.Bold = true;
                titleText.CharacterFormat.FontSize = 16;
                titleParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
                titleParagraph.ParagraphFormat.AfterSpacing = 20;
                
                // Add conversion info
                var infoParagraph = section.AddParagraph();
                infoParagraph.AppendText($"Converted from: {Path.GetFileName(inputFile)}");
                infoParagraph.ParagraphFormat.AfterSpacing = 5;
                infoParagraph.AppendText($"Conversion Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                infoParagraph.ParagraphFormat.AfterSpacing = 20;
                
                // Load PDF and extract text content
                using (var pdfDocument = new PdfLoadedDocument(inputFile))
                {
                    var pageCount = pdfDocument.Pages.Count;
                    
                    for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                    {
                        // Add page break for pages after the first
                        if (pageIndex > 0)
                        {
                            var pageBreakParagraph = section.AddParagraph();
                            pageBreakParagraph.AppendBreak(BreakType.PageBreak);
                        }
                        
                        // Add page header
                        var pageHeaderParagraph = section.AddParagraph();
                        var pageHeaderText = pageHeaderParagraph.AppendText($"--- Page {pageIndex + 1} ---");
                        pageHeaderText.CharacterFormat.Bold = true;
                        pageHeaderText.CharacterFormat.FontSize = 12;
                        pageHeaderParagraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
                        pageHeaderParagraph.ParagraphFormat.AfterSpacing = 15;
                        
                        try
                        {
                            // Extract text from PDF page
                            var page = pdfDocument.Pages[pageIndex];
                            var pageText = page.ExtractText();
                            
                            if (!string.IsNullOrEmpty(pageText))
                            {
                                // Process text to detect and preserve table structure
                                ProcessPageTextWithTables(section, pageText);
                            }
                            else
                            {
                                // If no text found, add note
                                var noTextParagraph = section.AddParagraph();
                                noTextParagraph.AppendText("[No text content found on this page]");
                                noTextParagraph.ParagraphFormat.AfterSpacing = 10;
                            }
                        }
                        catch (Exception ex)
                        {
                            // If page extraction fails, add error note
                            var errorParagraph = section.AddParagraph();
                            errorParagraph.AppendText($"[Error extracting content from page {pageIndex + 1}: {ex.Message}]");
                            errorParagraph.ParagraphFormat.AfterSpacing = 10;
                        }
                    }
                }

                using (MemoryStream output = new MemoryStream())
                {
                    wordDoc.Save(output, FormatType.Docx);
                    output.Position = 0;
                    return File(output.ToArray(),
                                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                                fileName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"DOCX export failed: {ex.Message}" });
            }
        }

        private void ProcessPageTextWithTables(IWSection section, string pageText)
        {
            // Split text into lines
            var lines = pageText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Group lines into potential table blocks
            var tableBlocks = new List<List<string>>();
            var currentBlock = new List<string>();
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var trimmedLine = line.Trim();
                
                // Check if this line looks like a table row
                if (IsTableRow(trimmedLine))
                {
                    currentBlock.Add(trimmedLine);
                }
                else
                {
                    // If we have a table block, process it
                    if (currentBlock.Count > 0)
                    {
                        tableBlocks.Add(new List<string>(currentBlock));
                        currentBlock.Clear();
                    }
                    
                    // Add non-table content directly
                    if (!string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        var paragraph = section.AddParagraph();
                        paragraph.AppendText(trimmedLine);
                        paragraph.ParagraphFormat.AfterSpacing = 6;
                    }
                }
            }
            
            // Process any remaining table block
            if (currentBlock.Count > 0)
            {
                tableBlocks.Add(currentBlock);
            }
            
            // Create tables from detected blocks
            foreach (var tableBlock in tableBlocks)
            {
                CreateTableFromLines(section, tableBlock);
            }
        }

        private bool IsTableRow(string line)
        {
            // Check for table patterns:
            // 1. Multiple spaces between words (indicating columns)
            // 2. Tab-separated content
            // 3. Pipe-separated content
            // 4. Lines that look like "Term" followed by "Definition"
            
            var spaceCount = line.Count(c => c == ' ');
            var tabCount = line.Count(c => c == '\t');
            var pipeCount = line.Count(c => c == '|');
            
            // Check for common table patterns
            if (spaceCount > 3 && line.Length > 20)
            {
                // Look for multiple words separated by multiple spaces
                var words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2)
                {
                    // Check if there are significant gaps between words
                    var parts = line.Split(new[] { "  " }, StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length >= 2;
                }
            }
            
            // Check for tab or pipe separators
            if (tabCount > 0 || pipeCount > 0)
            {
                return true;
            }
            
            // Check for specific patterns like "Term" and "Definition"
            if (line.Contains("Term") && line.Contains("Definition"))
            {
                return true;
            }
            
            return false;
        }

        private void CreateTableFromLines(IWSection section, List<string> tableLines)
        {
            if (tableLines.Count == 0) return;
            
            // Parse table data
            var tableData = new List<string[]>();
            int maxColumns = 0;
            
            foreach (var line in tableLines)
            {
                string[] cells;
                
                // Try different parsing methods
                if (line.Contains("\t"))
                {
                    // Tab-separated
                    cells = line.Split('\t', StringSplitOptions.RemoveEmptyEntries)
                               .Select(cell => cell.Trim())
                               .ToArray();
                }
                else if (line.Contains("|"))
                {
                    // Pipe-separated
                    cells = line.Split('|', StringSplitOptions.RemoveEmptyEntries)
                               .Select(cell => cell.Trim())
                               .ToArray();
                }
                else
                {
                    // Space-separated (look for multiple spaces)
                    var parts = line.Split(new[] { "  " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        cells = parts.Select(part => part.Trim()).ToArray();
                    }
                    else
                    {
                        // Fallback: split by single spaces and group
                        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (words.Length >= 2)
                        {
                            // Try to detect column boundaries
                            cells = DetectTableColumns(line);
                        }
                        else
                        {
                            cells = new[] { line.Trim() };
                        }
                    }
                }
                
                if (cells.Length > 0)
                {
                    tableData.Add(cells);
                    maxColumns = Math.Max(maxColumns, cells.Length);
                }
            }
            
            // Create Word table if we have valid table data
            if (maxColumns > 1 && tableData.Count > 0)
            {
                var table = section.AddTable();
                table.ResetCells(tableData.Count, maxColumns);
                
                for (int row = 0; row < tableData.Count; row++)
                {
                    var cells = tableData[row];
                    for (int col = 0; col < maxColumns; col++)
                    {
                        var cell = table.Rows[row].Cells[col];
                        var paragraph = cell.AddParagraph();
                        paragraph.AppendText(col < cells.Length ? cells[col] : "");
                    }
                }
                
                // Add spacing after table
                var spacingParagraph = section.AddParagraph();
                spacingParagraph.ParagraphFormat.AfterSpacing = 10;
            }
            else
            {
                // Fallback to regular paragraphs
                foreach (var line in tableLines)
                {
                    var paragraph = section.AddParagraph();
                    paragraph.AppendText(line);
                    paragraph.ParagraphFormat.AfterSpacing = 6;
                }
            }
        }

        private string[] DetectTableColumns(string line)
        {
            // Simple heuristic to detect table columns
            // Look for patterns like "Term" followed by "Definition"
            if (line.Contains("Term") && line.Contains("Definition"))
            {
                var termIndex = line.IndexOf("Term");
                var definitionIndex = line.IndexOf("Definition");
                
                if (termIndex < definitionIndex)
                {
                    return new[] { "Term", "Definition" };
                }
            }
            
            // Look for multiple words with significant spacing
            var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 2)
            {
                // Try to find a natural break point
                var midPoint = words.Length / 2;
                var firstPart = string.Join(" ", words.Take(midPoint));
                var secondPart = string.Join(" ", words.Skip(midPoint));
                
                return new[] { firstPart, secondPart };
            }
            
            return new[] { line.Trim() };
        }

        private IActionResult ExportAsImages(string inputFile, string fileName)
        {
            try
            {
                // Create a simple ZIP file with PDF information
                using (var zipStream = new MemoryStream())
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    // Create a text file with PDF information
                    var entry = archive.CreateEntry("pdf_info.txt");
                    using (var entryStream = entry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        writer.WriteLine($"PDF Export Information");
                        writer.WriteLine($"=====================");
                        writer.WriteLine($"Source File: {Path.GetFileName(inputFile)}");
                        writer.WriteLine($"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        writer.WriteLine($"File Size: {new FileInfo(inputFile).Length} bytes");
                        writer.WriteLine();
                        writer.WriteLine("Note: This is a simplified image export. For actual page images, please use the PDF export option.");
                    }

                    zipStream.Position = 0;
                    return File(zipStream.ToArray(), "application/zip", fileName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Images export failed: {ex.Message}" });
            }
        }

        private string GetCurrentFilePath()
        {
            // Check for overlayed file first, then original
            string overlayedFile = Path.Combine(Path.GetDirectoryName(UploadController.UploadedFilePath)!, "overlayed.pdf");
            if (System.IO.File.Exists(overlayedFile))
                return overlayedFile;
            
            string textReplacedFile = Path.Combine(Path.GetDirectoryName(UploadController.UploadedFilePath)!, "text_replaced.pdf");
            if (System.IO.File.Exists(textReplacedFile))
                return textReplacedFile;
            
            string watermarkedFile = Path.Combine(Path.GetDirectoryName(UploadController.UploadedFilePath)!, "watermarked.pdf");
            if (System.IO.File.Exists(watermarkedFile))
                return watermarkedFile;
            
            return UploadController.UploadedFilePath;
        }

        private string GetFileExtension(string format)
        {
            return format.ToLower() switch
            {
                "pdf" => "pdf",
                "docx" => "docx",
                "images" => "zip",
                _ => "pdf"
            };
        }
    }

    public class CustomExportRequest
    {
        public string Format { get; set; } = "";
        public string? CustomFileName { get; set; }
        public bool IncludePageNumbers { get; set; } = true;
        public int? ImageQuality { get; set; } = 100;
    }
}

