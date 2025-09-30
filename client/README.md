# Hansika Frontend Documentation

## Overview

The Hansika frontend is a React application that provides a user-friendly interface for PDF processing operations. It communicates with the .NET backend API to handle file uploads, text overlays, and exports.

## Project Structure

```
client/
├── public/                 # Static assets
│   ├── index.html         # Main HTML template
│   ├── favicon.ico        # App icon
│   └── manifest.json      # PWA manifest
├── src/                   # Source code
│   ├── components/        # React components
│   │   └── CommonComponents.js
│   ├── hooks/             # Custom React hooks
│   │   └── usePdfOperations.js
│   ├── services/          # API services
│   │   └── api.js
│   ├── styles/            # CSS styles
│   │   └── main.css
│   ├── App.js             # Main App component
│   ├── App.css            # App-specific styles
│   ├── index.js           # Application entry point
│   └── index.css          # Global styles
├── package.json           # Dependencies and scripts
└── README.md              # Frontend documentation
```

## Key Features

### 1. File Upload
- Drag and drop support
- PDF file validation
- Progress indicators
- Error handling

### 2. PDF Preview
- Embedded PDF viewer
- Responsive design
- Loading states

### 3. Text Overlay
- Customizable text positioning
- Font size and color options
- Real-time preview

### 4. Export Options
- Multiple format support (PDF, Word, Images)
- Modern file saving with File System Access API
- Fallback to traditional downloads

## Components

### CommonComponents.js
Contains reusable UI components:

- **FileUpload**: Drag-and-drop file upload component
- **PdfPreview**: PDF preview with iframe
- **OverlayForm**: Form for configuring text overlays
- **ExportButtons**: Export format selection buttons
- **LoadingSpinner**: Loading indicator
- **Notification**: Toast notifications
- **NotificationContainer**: Notification management

### Custom Hooks

#### usePdfUpload
Manages PDF upload functionality:
```javascript
const { uploadPdf, isUploading, uploadError, uploadedFile } = usePdfUpload();
```

#### usePdfOverlay
Handles text overlay operations:
```javascript
const { addOverlay, isProcessing, overlayError, overlayedFile } = usePdfOverlay();
```

#### usePdfExport
Manages export operations:
```javascript
const { exportPdf, isExporting, exportError } = usePdfExport();
```

#### useFileOperations
Tracks file history and current state:
```javascript
const { currentFile, fileHistory, setCurrentFile } = useFileOperations();
```

#### useAppState
Manages global application state:
```javascript
const { isLoading, notifications, addNotification } = useAppState();
```

## Services

### API Service (api.js)
Centralized API communication with the following services:

- **UploadService**: Handles PDF file uploads
- **OverlayService**: Manages text overlay operations
- **ExportService**: Handles file exports and downloads
- **StatusService**: Application status management

## Styling

### CSS Variables
The application uses CSS custom properties for consistent theming:

```css
:root {
  --primary-color: #2563eb;
  --success-color: #10b981;
  --error-color: #ef4444;
  --background-color: #f8fafc;
  /* ... more variables */
}
```

### Responsive Design
- Mobile-first approach
- Flexible grid layouts
- Adaptive typography
- Touch-friendly interactions

## State Management

The application uses React hooks for state management:

1. **Local State**: Component-level state with `useState`
2. **Custom Hooks**: Reusable stateful logic
3. **Context**: Global state when needed (not currently implemented)

## Error Handling

Comprehensive error handling at multiple levels:

1. **API Level**: Axios interceptors for request/response errors
2. **Service Level**: Try-catch blocks with user-friendly messages
3. **Component Level**: Error boundaries and fallback UI
4. **User Level**: Toast notifications and inline error messages

## Performance Optimizations

1. **Code Splitting**: Lazy loading of components
2. **Memoization**: `useCallback` and `useMemo` for expensive operations
3. **Bundle Optimization**: Tree shaking and minification
4. **Image Optimization**: Proper sizing and lazy loading

## Browser Support

- **Modern Browsers**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- **File System Access API**: Chrome 86+ (with fallback)
- **ES6+ Features**: Transpiled for broader compatibility

## Development

### Prerequisites
- Node.js 18+
- npm or yarn

### Setup
```bash
cd client
npm install
npm start
```

### Available Scripts
- `npm start`: Start development server
- `npm build`: Build for production
- `npm test`: Run tests
- `npm eject`: Eject from Create React App

### Environment Variables
Create a `.env` file in the client directory:

```env
REACT_APP_API_URL=http://localhost:5015/api
```

## Testing

The application includes:
- Unit tests for utility functions
- Component tests with React Testing Library
- Integration tests for API services
- E2E tests (planned)

## Deployment

### Production Build
```bash
npm run build
```

### Docker
The frontend can be containerized using the provided Dockerfile:

```bash
docker build -f Dockerfile.frontend -t hansika-frontend .
```

### Static Hosting
The built application can be deployed to:
- Netlify
- Vercel
- AWS S3 + CloudFront
- Azure Static Web Apps

## Accessibility

The application follows WCAG 2.1 guidelines:

- **Keyboard Navigation**: Full keyboard support
- **Screen Readers**: Proper ARIA labels and roles
- **Color Contrast**: WCAG AA compliant colors
- **Focus Management**: Visible focus indicators
- **Semantic HTML**: Proper heading structure

## Security Considerations

1. **File Validation**: Client-side PDF validation
2. **XSS Prevention**: Proper input sanitization
3. **CSRF Protection**: Token-based requests
4. **Content Security Policy**: Restricted resource loading

## Future Enhancements

1. **PWA Support**: Offline functionality and app installation
2. **Real-time Updates**: WebSocket integration
3. **Advanced Editing**: More overlay options
4. **Batch Processing**: Multiple file handling
5. **User Authentication**: Login and user management
6. **File History**: Persistent file storage
7. **Collaboration**: Real-time collaboration features