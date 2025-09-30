import React, { useState } from 'react';
import axios from 'axios';

const TextMarkupComponent = ({ pdfUrl, onPdfUpdate }) => {
    const [selectedText, setSelectedText] = useState('');
    const [pageNumber, setPageNumber] = useState(1);
    const [markupColor, setMarkupColor] = useState('#FFFF00');
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [showTextModal, setShowTextModal] = useState(false);
    const [manualText, setManualText] = useState('');
    const [isSelecting, setIsSelecting] = useState(false);

    const handleTextSelection = () => {
        setIsSelecting(true);
        
        // Add visual feedback
        const button = document.querySelector('.btn-select');
        if (button) {
            button.style.transform = 'scale(0.95)';
            setTimeout(() => {
                button.style.transform = '';
            }, 150);
        }
        
        // Get selected text from the PDF preview iframe
        const iframe = document.querySelector('iframe[title="PDF Preview"]');
        if (iframe && iframe.contentWindow) {
            try {
                const selection = iframe.contentWindow.getSelection();
                if (selection && selection.toString().trim()) {
                    setSelectedText(selection.toString().trim());
                    setSuccess('Text selected successfully!');
                    setTimeout(() => setSuccess(null), 2000);
                } else {
                    // Fallback: show custom modal for manual input
                    setShowTextModal(true);
                }
            } catch (e) {
                // Cross-origin restriction - show custom modal
                setShowTextModal(true);
            }
        } else {
            // No iframe or cross-origin - show custom modal
            setShowTextModal(true);
        }
        
        setTimeout(() => setIsSelecting(false), 500);
    };

    const handleManualTextSubmit = () => {
        if (manualText.trim()) {
            setSelectedText(manualText.trim());
            setShowTextModal(false);
            setManualText('');
        }
    };

    const handleModalClose = () => {
        setShowTextModal(false);
        setManualText('');
    };


    const addMarkup = async (markupType) => {
        if (!selectedText.trim()) {
            setError('Please select or enter text first');
            return;
        }

        setIsLoading(true);
        setError(null);
        setSuccess(null);

        try {
            const response = await axios.post(`http://localhost:5015/api/TextMarkup/${markupType}`, {
                text: selectedText,
                pageNumber: pageNumber,
                x: 50, // Approximate position - will be improved
                y: 50,
                width: selectedText.length * 8, // Approximate width
                height: 20,
                color: markupColor
            });

            if (response.data.success) {
                setSuccess(response.data.message);
                // Update the PDF preview with the new annotated version
                if (onPdfUpdate) {
                    // Convert relative URL to full URL
                    const fullUrl = `http://localhost:5015${response.data.fileUrl}`;
                    onPdfUpdate(fullUrl);
                }
                // Clear selection after successful markup
                setSelectedText('');
            }
        } catch (err) {
            setError(err.response?.data?.error || 'Failed to add markup');
        } finally {
            setIsLoading(false);
        }
    };


    return (
        <div className="organized-markup-editor">
            <div className="markup-header">
                <div className="header-icon">🖍️</div>
                <h3>Text Markup (Highlight & Underline)</h3>
            </div>
            
            <div className="markup-controls">
                <div className="text-selection-section">
                    <button 
                        onClick={handleTextSelection} 
                        className={`btn-select-text ${isSelecting ? 'selecting' : ''}`}
                        disabled={isSelecting}
                    >
                        {isSelecting ? 'Selecting...' : 'Select Text'}
                    </button>
                    
                    <div className={`text-display ${selectedText ? 'has-text' : ''}`}>
                        {selectedText || 'No text selected'}
                    </div>
                </div>
                
                <div className="markup-options">
                    <div className="color-page-row">
                        <div className="color-input-group">
                            <label>Color:</label>
                            <input 
                                type="color" 
                                value={markupColor}
                                onChange={(e) => setMarkupColor(e.target.value)}
                                className="color-picker"
                                title="Choose markup color"
                            />
                        </div>
                        
                        <div className="page-input-group">
                            <label>Page:</label>
                            <input 
                                type="number" 
                                value={pageNumber}
                                onChange={(e) => setPageNumber(parseInt(e.target.value) || 1)}
                                min="1"
                                className="page-number-input"
                                placeholder="1"
                                title="Enter page number"
                            />
                        </div>
                    </div>
                    
                    <div className="markup-action-buttons">
                        <button 
                            onClick={() => addMarkup('highlight')}
                            disabled={!selectedText.trim() || isLoading}
                            className="btn-highlight"
                        >
                            Highlight
                        </button>
                        <button 
                            onClick={() => addMarkup('underline')}
                            disabled={!selectedText.trim() || isLoading}
                            className="btn-underline"
                        >
                            Underline
                        </button>
                        <button 
                            onClick={() => addMarkup('strikethrough')}
                            disabled={!selectedText.trim() || isLoading}
                            className="btn-strikethrough"
                        >
                            Strike
                        </button>
                    </div>
                </div>
            </div>

            {isLoading && <div className="loading">Processing...</div>}
            {error && <div className="error">{error}</div>}
            {success && <div className="success">{success}</div>}

            {/* Beautiful Text Selection Modal */}
            {showTextModal && (
                <div className="modal-overlay">
                    <div className="text-selection-modal">
                        <div className="modal-header">
                            <h3>📝 Enter Text to Markup</h3>
                            <button className="modal-close" onClick={handleModalClose}>
                                ×
                            </button>
                        </div>
                        <div className="modal-body">
                            <p>Enter text to markup:</p>
                            <div className="paste-buttons">
                                <button 
                                    type="button"
                                    onClick={async () => {
                                        try {
                                            const text = await navigator.clipboard.readText();
                                            setManualText(text);
                                        } catch (err) {
                                            // Fallback: create temporary input for paste
                                            const tempInput = document.createElement('input');
                                            tempInput.style.position = 'absolute';
                                            tempInput.style.left = '-9999px';
                                            document.body.appendChild(tempInput);
                                            tempInput.focus();
                                            document.execCommand('paste');
                                            const pastedText = tempInput.value;
                                            document.body.removeChild(tempInput);
                                            if (pastedText) {
                                                setManualText(pastedText);
                                            }
                                        }
                                    }}
                                    className="btn-paste"
                                >
                                    📋 Paste
                                </button>
                                <button 
                                    type="button"
                                    onClick={() => setManualText('')}
                                    className="btn-clear"
                                >
                                    🗑️ Clear
                                </button>
                            </div>
                            <textarea
                                value={manualText}
                                onChange={(e) => setManualText(e.target.value)}
                                placeholder="Type or paste text here..."
                                className="modal-textarea"
                                rows="3"
                                autoFocus
                            />
                        </div>
                        <div className="modal-footer">
                            <button className="btn-cancel" onClick={handleModalClose}>
                                Cancel
                            </button>
                            <button 
                                className="btn-confirm" 
                                onClick={handleManualTextSubmit}
                                disabled={!manualText.trim()}
                            >
                                Add Text
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default TextMarkupComponent;

