import React, { useState, useRef, useEffect } from "react";

const PDFPreviewComponent = ({ pdfUrl, onPageChange }) => {
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(0);
    const [scale, setScale] = useState(1.0);
    const iframeRef = useRef(null);

    useEffect(() => {
        if (pdfUrl && iframeRef.current) {
            // Reset page when PDF changes
            setCurrentPage(1);
        }
    }, [pdfUrl]);

    const handlePageChange = (page) => {
        if (page >= 1 && page <= totalPages) {
            setCurrentPage(page);
            if (onPageChange) {
                onPageChange(page);
            }
        }
    };

    const handleScaleChange = (newScale) => {
        setScale(newScale);
    };

    if (!pdfUrl) {
        return (
            <div className="pdf-preview-container">
                <div className="no-pdf-message">
                    <p>No PDF loaded</p>
                </div>
            </div>
        );
    }

    return (
        <div className="pdf-preview-container">
            <div className="pdf-controls">
                <div className="page-controls">
                    <button 
                        onClick={() => handlePageChange(currentPage - 1)}
                        disabled={currentPage <= 1}
                        className="page-btn"
                    >
                        Previous
                    </button>
                    <span className="page-info">
                        Page {currentPage} of {totalPages || '?'}
                    </span>
                    <button 
                        onClick={() => handlePageChange(currentPage + 1)}
                        disabled={currentPage >= totalPages}
                        className="page-btn"
                    >
                        Next
                    </button>
                </div>
                <div className="scale-controls">
                    <button 
                        onClick={() => handleScaleChange(scale - 0.1)}
                        disabled={scale <= 0.5}
                        className="scale-btn"
                    >
                        -
                    </button>
                    <span className="scale-info">
                        {Math.round(scale * 100)}%
                    </span>
                    <button 
                        onClick={() => handleScaleChange(scale + 0.1)}
                        disabled={scale >= 2.0}
                        className="scale-btn"
                    >
                        +
                    </button>
                </div>
            </div>
            <div className="pdf-viewer">
                <iframe
                    ref={iframeRef}
                    src={pdfUrl}
                    width="100%"
                    height="600"
                    style={{ 
                        border: '1px solid #ccc', 
                        borderRadius: '8px',
                        transform: `scale(${scale})`,
                        transformOrigin: 'top left'
                    }}
                    title="PDF Preview"
                />
            </div>
        </div>
    );
};

export default PDFPreviewComponent;

