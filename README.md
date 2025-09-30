# Hansika-PDF-Editor
I have developed a very simple PDF editor using React with JS as frontend and backend in C# ASP.NET Core (MVC). This is my very first project related to .NET tech stack.

## Features

- **PDF Upload & Preview** - Upload and preview PDF documents
- **Text Markup** - Highlight, underline, and strikethrough text
- **Text Annotations** - Add sticky notes, free text, and callouts
- **Export Options** - Export to PDF, Word, and Images
- **Coordinate-based Positioning** - Click-to-position annotation system
- **Modern UI** - Responsive design with sidebar layout

## Setup Instructions

### Prerequisites

- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+** - [Download here](https://nodejs.org/)
- **Visual Studio 2022** 

### Backend Setup (.NET)

1. **Navigate to backend directory:**
   ```bash
   cd Hansika
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Run the backend:**
   ```bash
   dotnet run
   ```
   - Backend will start on `https://localhost:5015`

### Frontend Setup (React)

1. **Navigate to frontend directory:**
   ```bash
   cd client
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Start the development server:**
   ```bash
   npm start
   ```
   - Frontend will start on `http://localhost:3000`

### Full Application

- Open `http://localhost:3000` in your browser
- The React app will communicate with the .NET backend automatically

## Libraries & Packages Used

### Backend (.NET 9.0)

- **Syncfusion.Pdf.Net.Core** (31.1.21) - PDF manipulation and rendering
- **Syncfusion.DocIO.Net.Core** (31.1.21) - Word document processing
- **Syncfusion.DocIORenderer.Net.Core** (31.1.21) - Document rendering
- **Syncfusion.PdfToImageConverter.Net** (31.1.21) - PDF to image conversion
- **DocumentFormat.OpenXml** (3.3.0) - Office document handling
- **IronPdf** (2025.9.4) - PDF generation and processing
- **System.Drawing.Common** (9.0.9) - Image processing


### Frontend (React)

- **React** (19.1.1) - UI framework
- **Axios** (1.12.2) - HTTP client for API calls
- **CSS3** - Modern styling with flexbox and grid

## Assumptions Made

### Development Environment
- **Windows 10/11** - Primary development platform
- **Visual Studio 2022** - IDE with .NET 9.0 support
- **Chrome/Edge** - Primary browser for testing

### PDF Processing
- **Syncfusion License** - Commercial license required for production
- **PDF Format** - Standard PDF 1.7+ compatibility


## API Endpoints

- `POST /api/upload` - Upload PDF file
- `POST /api/TextMarkup/{type}` - Add text markup
- `POST /api/TextAnnotation/{type}` - Add annotations
- `GET /api/Export/{format}` - Export PDF
- `GET /api/ImagesExport` - Export as images

  
### Edit Options Issues

#### 1. **Text Markup Functionality**
- **Text Selection Limitation**: The current implementation requires exact text matching, which may not work with:
  - Text with special characters or formatting
  - Text spanning multiple lines
  - Text with different font sizes or styles
- **Coordinate System Mismatch**: Screen coordinates from frontend may not accurately map to PDF coordinates

#### 2. **Text Annotation Positioning**
- **Coordinate Conversion Issues**: 
  - Screen-to-PDF coordinate conversion may be inaccurate
  - Different PDF scaling factors not properly handled
  - Iframe dimensions may not match actual PDF display size
- **Annotation Placement**: Annotations may appear in wrong positions due to:
  - Incorrect Y-axis inversion calculations
  - Missing annotation height adjustments
  - Scale factor miscalculations

#### 3. **PDF Processing**
- **Syncfusion License**: Trial version limitations apply (watermarks, page limits)
- **PDF Format**: Assumes standard PDF 1.7+ compatibility


## License

This project uses commercial library (Syncfusion) that require proper licensing for production use.





