import React, { useState, useEffect } from 'react';
import axios from 'axios';

const TextAnnotationComponent = ({ pdfUrl, onPdfUpdate }) => {
    const [annotationType, setAnnotationType] = useState('sticky-note');
    const [annotationText, setAnnotationText] = useState('');
    const [pageNumber, setPageNumber] = useState(1);
    const [x, setX] = useState('');
    const [y, setY] = useState('');
    const [width, setWidth] = useState(100);
    const [height, setHeight] = useState(30);
    const [fontSize, setFontSize] = useState(12);
    const [textColor, setTextColor] = useState('#000000');
    const [backgroundColor, setBackgroundColor] = useState('#FFFFFF');
    const [borderColor] = useState('#000000');
    const [annotationColor, setAnnotationColor] = useState('#FFFF00');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [isPositioningMode, setIsPositioningMode] = useState(false);


    const addAnnotation = async () => {
        if (!annotationText.trim()) {
            setError('Please enter annotation text');
            return;
        }

        setIsLoading(true);
        setError(null);
        setSuccess(null);

        try {
            // Get iframe dimensions for coordinate conversion
            const iframe = document.getElementById('pdf-preview-iframe');
            const iframeWidth = iframe ? iframe.getBoundingClientRect().width : 800;
            const iframeHeight = iframe ? iframe.getBoundingClientRect().height : 700;
            
            let requestData = {
                text: annotationText,
                pageNumber: pageNumber,
                x: x ? parseFloat(x) : 50,
                y: y ? parseFloat(y) : 50,
                color: annotationColor,
                iframeWidth: iframeWidth,
                iframeHeight: iframeHeight
            };

            console.log('Sending annotation request:', requestData);

            // Add type-specific properties
            if (annotationType === 'free-text') {
                requestData = {
                    ...requestData,
                    width: width,
                    height: height,
                    fontSize: fontSize,
                    textColor: textColor,
                    backgroundColor: backgroundColor,
                    borderColor: borderColor
                };
            } else if (annotationType === 'callout') {
                requestData = {
                    ...requestData,
                    width: width,
                    height: height,
                    subject: 'Callout'
                };
            } else if (annotationType === 'sticky-note') {
                requestData = {
                    ...requestData,
                    subject: 'Sticky Note'
                };
            }

            const response = await axios.post(`http://localhost:5015/api/TextAnnotation/${annotationType}`, requestData);

            if (response.data.success) {
                       setSuccess(response.data.message);
                       // Update the PDF preview with the new annotated version
                       if (onPdfUpdate) {
                           // Convert relative URL to full URL
                           const fullUrl = `http://localhost:5015${response.data.fileUrl}`;
                           onPdfUpdate(fullUrl);
                       }
                       // Clear form after successful annotation
                       setAnnotationText('');
            }
        } catch (err) {
            setError(err.response?.data?.error || 'Failed to add annotation');
        } finally {
            setIsLoading(false);
        }
    };

    const togglePositioningMode = () => {
        const newMode = !isPositioningMode;
        console.log('Toggling positioning mode to:', newMode);
        setIsPositioningMode(newMode);
        
        // Request global positioning mode
        const event = new CustomEvent('requestPositioningMode', {
            detail: { enable: newMode }
        });
        window.dispatchEvent(event);
        console.log('Positioning mode request dispatched');
    };

    // Convert screen coordinates to PDF coordinates
    const convertToPdfCoordinates = (screenX, screenY) => {
        // Get the iframe dimensions
        const iframe = document.getElementById('pdf-preview-iframe');
        if (!iframe) {
            console.log('Iframe not found, returning screen coordinates');
            return { x: screenX, y: screenY };
        }
        
        const rect = iframe.getBoundingClientRect();
        const iframeWidth = rect.width;
        const iframeHeight = rect.height;
        
        console.log('Iframe dimensions:', { iframeWidth, iframeHeight });
        console.log('Screen coordinates:', { screenX, screenY });
        
        // For now, just return the screen coordinates
        // The backend will handle the conversion to PDF coordinates
        return { x: screenX, y: screenY };
    };

    // Listen for coordinate selection from PDF
    useEffect(() => {
        const handleCoordinateSelected = (event) => {
            console.log('=== COORDINATE EVENT RECEIVED ===');
            console.log('Event detail:', event.detail);
            console.log('Current positioning mode:', isPositioningMode);
            console.log('Current X state:', x);
            console.log('Current Y state:', y);
            
            if (isPositioningMode) {
                console.log('Processing coordinate selection...');
                const pdfCoords = convertToPdfCoordinates(event.detail.x, event.detail.y);
                console.log('Converted coordinates:', pdfCoords);
                
                // Update state
                setX(pdfCoords.x.toString());
                setY(pdfCoords.y.toString());
                setIsPositioningMode(false);
                
                console.log('State updated, positioning mode disabled');
                
                // Also update DOM directly as backup
                setTimeout(() => {
                    const xInput = document.querySelector('input[placeholder="X position"]');
                    const yInput = document.querySelector('input[placeholder="Y position"]');
                    console.log('Found inputs:', { xInput, yInput });
                    if (xInput) {
                        xInput.value = pdfCoords.x.toString();
                        console.log('X input updated to:', xInput.value);
                    }
                    if (yInput) {
                        yInput.value = pdfCoords.y.toString();
                        console.log('Y input updated to:', yInput.value);
                    }
                }, 50);
            } else {
                console.log('Positioning mode not active, ignoring coordinate selection');
            }
        };

        console.log('Adding event listener for pdfCoordinateSelected');
        window.addEventListener('pdfCoordinateSelected', handleCoordinateSelected);
        return () => {
            console.log('Removing event listener for pdfCoordinateSelected');
            window.removeEventListener('pdfCoordinateSelected', handleCoordinateSelected);
        };
    }, [isPositioningMode, x, y]);

    return (
        <div className="modern-annotation-editor">
            <div className="annotation-header">
                <div className="header-content">
                    <div className="header-text">
                        <h3>Text Annotations</h3>
                        <p>Add notes, comments, and callouts to your PDF</p>
                    </div>
                </div>
                <div className="annotation-controls">
                    <div className="page-control">
                        <label className="control-label">Page:</label>
                        <input 
                            type="number" 
                            value={pageNumber}
                            onChange={(e) => setPageNumber(parseInt(e.target.value) || 1)}
                            min="1"
                            className="page-control-input"
                            title="Page number"
                        />
                    </div>
                    <div className="color-control">
                        <label className="control-label">Color:</label>
                        <input 
                            type="color" 
                            value={annotationColor}
                            onChange={(e) => setAnnotationColor(e.target.value)}
                            className="color-picker"
                            title="Annotation color"
                        />
                    </div>
                </div>
            </div>
            
            <div className="annotation-workspace">
                <div className="type-selector-modern">
                    <div className="selector-label">Annotation Type</div>
                    <div className="type-buttons">
                        <button 
                            className={`type-btn ${annotationType === 'sticky-note' ? 'active' : ''}`}
                            onClick={() => setAnnotationType('sticky-note')}
                        >
                            <span className="type-icon">📌</span>
                            <span className="type-text">Sticky Note</span>
                        </button>
                        <button 
                            className={`type-btn ${annotationType === 'free-text' ? 'active' : ''}`}
                            onClick={() => setAnnotationType('free-text')}
                        >
                            <span className="type-icon">📝</span>
                            <span className="type-text">Free Text</span>
                        </button>
                        <button 
                            className={`type-btn ${annotationType === 'callout' ? 'active' : ''}`}
                            onClick={() => setAnnotationType('callout')}
                        >
                            <span className="type-icon">💬</span>
                            <span className="type-text">Callout</span>
                        </button>
                    </div>
                </div>
                
                <div className="text-input-section">
                    <div className="input-label">Annotation Text</div>
                    <div className="text-input-container">
                        <textarea 
                            value={annotationText}
                            onChange={(e) => setAnnotationText(e.target.value)}
                            placeholder="Enter your annotation text here..."
                            className="modern-text-input"
                            rows="3"
                        />
                        <div className="char-counter">
                            {annotationText.length}/500
                        </div>
                    </div>
                </div>
                
                <div className="positioning-section">
                    <div className="positioning-controls">
                        <button 
                            type="button"
                            onClick={togglePositioningMode}
                            className={`btn-position-modern ${isPositioningMode ? 'active' : ''}`}
                            title="Click to enable click-to-position mode"
                        >
                            <span className="btn-text">
                                {isPositioningMode ? 'Click on PDF to Position' : 'Click to Position'}
                            </span>
                        </button>
                        
                        {isPositioningMode && (
                            <div className="positioning-feedback">
                                <div className="feedback-text">
                                    <strong>Positioning Mode Active</strong>
                                    <p>Click anywhere on the PDF preview to set the position</p>
                                </div>
                            </div>
                        )}
                    </div>
                </div>
                
                {annotationType === 'free-text' && (
                    <div className="size-controls">
                        <div className="input-group">
                            <label>Width:</label>
                            <input 
                                type="number" 
                                value={width}
                                onChange={(e) => setWidth(parseInt(e.target.value) || 100)}
                                min="50"
                                placeholder="Width"
                                className="size-input"
                                title="Text box width"
                            />
                        </div>
                        <div className="input-group">
                            <label>Height:</label>
                            <input 
                                type="number" 
                                value={height}
                                onChange={(e) => setHeight(parseInt(e.target.value) || 30)}
                                min="20"
                                placeholder="Height"
                                className="size-input"
                                title="Text box height"
                            />
                        </div>
                        <div className="input-group">
                            <label>Font Size:</label>
                            <input 
                                type="number" 
                                value={fontSize}
                                onChange={(e) => setFontSize(parseInt(e.target.value) || 12)}
                                min="8"
                                max="72"
                                placeholder="Font Size"
                                className="size-input"
                                title="Text font size"
                            />
                        </div>
                    </div>
                )}
                
                <div className="coordinate-section">
                    <div className="coordinate-controls">
                        <div className="coord-control">
                            <label className="coord-label">X:</label>
                            <input 
                                type="number" 
                                value={x}
                                onChange={(e) => setX(e.target.value)}
                                placeholder="0"
                                className="coord-input"
                                title="Horizontal position"
                            />
                        </div>
                        <div className="coord-control">
                            <label className="coord-label">Y:</label>
                            <input 
                                type="number" 
                                value={y}
                                onChange={(e) => setY(e.target.value)}
                                placeholder="0"
                                className="coord-input"
                                title="Vertical position"
                            />
                        </div>
                        {annotationType === 'free-text' && (
                            <>
                                <div className="color-input-group">
                                    <label className="color-label">Text:</label>
                                    <input 
                                        type="color" 
                                        value={textColor}
                                        onChange={(e) => setTextColor(e.target.value)}
                                        className="color-picker"
                                        title="Text color"
                                    />
                                </div>
                                <div className="color-input-group">
                                    <label className="color-label">Background:</label>
                                    <input 
                                        type="color" 
                                        value={backgroundColor}
                                        onChange={(e) => setBackgroundColor(e.target.value)}
                                        className="color-picker"
                                        title="Background color"
                                    />
                                </div>
                            </>
                        )}
                    </div>
                </div>
                
                <button 
                    onClick={addAnnotation}
                    disabled={!annotationText.trim() || isLoading}
                    className="btn-add"
                >
                    Add Annotation
                </button>
            </div>

            {isLoading && <div className="loading">Processing...</div>}
            {error && <div className="error">{error}</div>}
            {success && <div className="success">{success}</div>}

        </div>
    );
};

export default TextAnnotationComponent;

