import React, { useState, useEffect } from "react";
import "./App.css";
import "./styles/EditComponents.css";

// Import components
import UploadComponent from "./components/UploadComponent";
import ExportComponent from "./components/ExportComponent";
import LoadingSpinner from "./components/LoadingSpinner";
import MessageDisplay from "./components/MessageDisplay";
import TextMarkupComponent from "./components/TextMarkupComponent";
import TextAnnotationComponent from "./components/TextAnnotationComponent";

function App() {
    const [previewPdf, setPreviewPdf] = useState(null);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState(null);
    const [success, setSuccess] = useState(null);
    const [activeTab, setActiveTab] = useState("upload");
    const [editedPdfUrl, setEditedPdfUrl] = useState(null);
    const [editMode, setEditMode] = useState("markup");

    // Check URL parameters and restore PDF state on component mount
    useEffect(() => {
        const urlParams = new URLSearchParams(window.location.search);
        const tab = urlParams.get('tab');
        const pdfUrl = urlParams.get('pdf');
        
        if (tab && ['upload', 'edit', 'export'].includes(tab)) {
            setActiveTab(tab);
        }
        
        // Restore PDF state if pdfUrl exists in URL
        if (pdfUrl) {
            setPreviewPdf(pdfUrl);
            setEditedPdfUrl(pdfUrl);
            restorePdfState(pdfUrl);
        }
    }, []);


    // Function to restore PDF state in backend
    const restorePdfState = async (pdfUrl) => {
        try {
            const response = await fetch('http://localhost:5015/api/Upload/restore', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ fileUrl: pdfUrl })
            });

            if (response.ok) {
                const data = await response.json();
                console.log('PDF state restored successfully:', data.message);
            } else {
                const errorData = await response.json();
                console.warn('Failed to restore PDF state:', errorData.error);
            }
        } catch (error) {
            console.error('Error restoring PDF state:', error);
        }
    };

    // Update URL when tab changes
    const handleTabChange = (tab) => {
        setActiveTab(tab);
        const url = new URL(window.location);
        url.searchParams.set('tab', tab);
        
        // Keep PDF URL in the URL for all tabs to maintain state
        if (previewPdf) {
            url.searchParams.set('pdf', previewPdf);
        }
        
        window.history.pushState({}, '', url);
    };

    // Event handlers
    const handleUploadSuccess = (data) => {
        setPreviewPdf(data.fileUrl);
        setEditedPdfUrl(data.fileUrl);
        setSuccess("File uploaded successfully!");
        
        // Update URL with PDF information
        const url = new URL(window.location);
        url.searchParams.set('pdf', data.fileUrl);
        window.history.pushState({}, '', url);
        
        // Don't auto-switch to edit tab - let user choose
    };

    const handleError = (errorMessage) => {
        setError(errorMessage);
    };

    const handleSuccess = (successMessage) => {
        setSuccess(successMessage);
    };

    const handleLoadingChange = (loading) => {
        setIsLoading(loading);
    };

    const handlePdfUpdate = (updatedPdfUrl) => {
        setEditedPdfUrl(updatedPdfUrl);
        
        // Update URL with the new PDF URL
        const url = new URL(window.location);
        url.searchParams.set('pdf', updatedPdfUrl);
        window.history.pushState({}, '', url);
    };


    const clearMessages = () => {
        setError(null);
        setSuccess(null);
    };

    // Global coordinate detection for PDF
    const [isGlobalPositioningMode, setIsGlobalPositioningMode] = useState(false);
    const [globalMousePosition, setGlobalMousePosition] = useState({ x: 0, y: 0 });

    const handlePdfClick = (e) => {
        console.log('=== PDF CLICKED ===');
        console.log('Positioning mode active:', isGlobalPositioningMode);
        console.log('Event:', e);
        
        if (!isGlobalPositioningMode) {
            console.log('Positioning mode is not active, ignoring click');
            return;
        }
        
        const rect = e.currentTarget.getBoundingClientRect();
        const xPos = Math.round(e.clientX - rect.left);
        const yPos = Math.round(e.clientY - rect.top);
        
        console.log('Click position relative to overlay:', { x: xPos, y: yPos });
        console.log('Overlay rect:', rect);
        
        // Dispatch custom event with coordinates
        const coordinateEvent = new CustomEvent('pdfCoordinateSelected', {
            detail: { x: xPos, y: yPos }
        });
        console.log('Dispatching coordinate event:', coordinateEvent);
        window.dispatchEvent(coordinateEvent);
        
        setIsGlobalPositioningMode(false);
        hideCoordinateOverlay();
        console.log('Positioning mode disabled and overlay hidden');
    };

    const handlePdfMouseMove = (e) => {
        if (!isGlobalPositioningMode) return;
        
        const rect = e.currentTarget.getBoundingClientRect();
        const xPos = Math.round(e.clientX - rect.left);
        const yPos = Math.round(e.clientY - rect.top);
        
        setGlobalMousePosition({ x: xPos, y: yPos });
        
        // Update coordinate display
        const coordinateText = document.getElementById('coordinate-text');
        if (coordinateText) {
            coordinateText.textContent = `X: ${xPos}, Y: ${yPos}`;
        }
        
        // Update coordinate display position
        const coordinateDisplay = document.getElementById('coordinate-display');
        if (coordinateDisplay) {
            coordinateDisplay.style.left = `${xPos + 10}px`;
            coordinateDisplay.style.top = `${yPos - 30}px`;
        }
    };

    const showCoordinateOverlay = () => {
        console.log('=== SHOWING COORDINATE OVERLAY ===');
        const overlay = document.getElementById('pdf-coordinate-overlay');
        console.log('Overlay element:', overlay);
        if (overlay) {
            overlay.style.display = 'block';
            console.log('Overlay displayed successfully');
            console.log('Overlay style:', overlay.style.cssText);
        } else {
            console.error('Overlay element not found!');
        }
        setIsGlobalPositioningMode(true);
        console.log('Global positioning mode set to true');
    };

    const hideCoordinateOverlay = () => {
        const overlay = document.getElementById('pdf-coordinate-overlay');
        if (overlay) {
            overlay.style.display = 'none';
        }
        setIsGlobalPositioningMode(false);
    };

    // Listen for positioning mode requests from components
    useEffect(() => {
        const handlePositioningModeRequest = (event) => {
            if (event.detail.enable) {
                showCoordinateOverlay();
            } else {
                hideCoordinateOverlay();
            }
        };

        window.addEventListener('requestPositioningMode', handlePositioningModeRequest);
        return () => {
            window.removeEventListener('requestPositioningMode', handlePositioningModeRequest);
        };
    }, []);



    return (
        <div className="app-container">
            <header className="app-header">
                <div className="header-content">
                    <h1>Hansika PDF Editor</h1>
                    <div className="header-tools">
                        <button 
                            className={activeTab === "upload" ? "active" : ""} 
                            onClick={() => handleTabChange("upload")}
                        >
                            Upload
                        </button>
                        <button 
                            className={activeTab === "edit" ? "active" : ""} 
                            onClick={() => handleTabChange("edit")}
                            disabled={!previewPdf}
                        >
                            Edit
                        </button>
                        <button 
                            className={activeTab === "export" ? "active" : ""} 
                            onClick={() => handleTabChange("export")}
                            disabled={!previewPdf}
                        >
                            Export 
                        </button>
                    </div>
                </div>
            </header>


            {/* Messages */}
            {previewPdf && (
                <MessageDisplay 
                    error={error} 
                    success={success} 
                    onClear={clearMessages} 
                />
            )}

            {/* Loading Indicator */}
            <LoadingSpinner isLoading={isLoading} />

            {/* Main Layout: Sidebar + PDF Preview */}
            <div className="main-layout">
                {/* Left Sidebar */}
                <div className="left-sidebar">

                    {/* Edit Mode Selector */}
                    {activeTab === "edit" && previewPdf && (
                        <div className="sidebar-section">
                            <h3>🛠️ Edit Tools</h3>
                            <div className="edit-mode-buttons">
                                <button 
                                    className={editMode === "markup" ? "active" : ""}
                                    onClick={() => setEditMode("markup")}
                                >
                                    📝 Text Markup
                                </button>
                                <button 
                                    className={editMode === "annotation" ? "active" : ""}
                                    onClick={() => setEditMode("annotation")}
                                >
                                    📋 Text Annotations
                                </button>
                            </div>
                        </div>
                    )}

                    {/* Tab Content */}
                    <div className="sidebar-content">
                        {activeTab === "upload" && (
                            <UploadComponent 
                                onUploadSuccess={handleUploadSuccess}
                                onError={handleError}
                                onSuccess={handleSuccess}
                                onLoadingChange={handleLoadingChange}
                                isLoading={isLoading}
                            />
                        )}

                        {activeTab === "edit" && previewPdf && (
                            <div className="edit-components">
                                {editMode === "markup" && (
                                    <TextMarkupComponent 
                                        pdfUrl={previewPdf}
                                        onPdfUpdate={handlePdfUpdate}
                                    />
                                )}
                                
                                {editMode === "annotation" && (
                                    <TextAnnotationComponent 
                                        pdfUrl={previewPdf}
                                        onPdfUpdate={handlePdfUpdate}
                                    />
                                )}
                            </div>
                        )}

                        {activeTab === "export" && previewPdf && (
                            <ExportComponent 
                                onError={handleError}
                                onSuccess={handleSuccess}
                                onLoadingChange={handleLoadingChange}
                                isLoading={isLoading}
                                highlightedPdfUrl={editedPdfUrl || previewPdf}
                            />
                        )}
                    </div>
                </div>

                {/* Right PDF Preview */}
                <div className="right-preview">
                    {(editedPdfUrl || previewPdf) ? (
                        <div className="pdf-preview-container">
                            <iframe
                                src={editedPdfUrl || previewPdf}
                                width="100%"
                                height="100%"
                                style={{ 
                                    border: '1px solid #ccc', 
                                    borderRadius: '8px'
                                }}
                                title="PDF Preview"
                                id="pdf-preview-iframe"
                            />
                            <div 
                                className="pdf-coordinate-overlay" 
                                id="pdf-coordinate-overlay" 
                                style={{ display: 'none' }}
                                onClick={handlePdfClick}
                                onMouseMove={handlePdfMouseMove}
                            >
                                <div className="coordinate-display" id="coordinate-display">
                                    <span id="coordinate-text">X: 0, Y: 0</span>
                                </div>
                            </div>
                        </div>
                    ) : (
                        <div className="no-pdf-placeholder">
                            <div className="placeholder-content">
                                <h2>📄 No PDF Selected</h2>
                                <p>Upload a PDF file to start editing</p>
                                <button 
                                    onClick={() => handleTabChange("upload")}
                                    className="btn-upload-placeholder"
                                >
                                    📤 Upload PDF
                                </button>
                            </div>
                        </div>
                    )}
                </div>
            </div>

        </div>
    );
}

export default App;
