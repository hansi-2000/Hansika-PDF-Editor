import React, { useState } from "react";
import axios from "axios";

const ExportComponent = ({ 
    onError, 
    onSuccess,
    onLoadingChange,
    isLoading,
    highlightedPdfUrl
}) => {
    const [exportFormat, setExportFormat] = useState("pdf");

    const handleExport = async () => {
        if (!highlightedPdfUrl) {
            onError("No PDF available for export");
            return;
        }

        onLoadingChange(true);
        onError(null);

        try {
            let response;
            let fileName;
            let extension;

            if (exportFormat === 'images') {
                // Use ImagesExportController for proper image export
                response = await axios.get(`http://localhost:5015/api/ImagesExport`, {
                    responseType: 'blob'
                });
                extension = 'zip';
                fileName = `exported_images.${extension}`;
            } else {
                // Use ExportController for PDF and DOCX
                response = await axios.get(`http://localhost:5015/api/Export/${exportFormat}`, {
                    responseType: 'blob'
                });
                extension = exportFormat;
                fileName = `exported_document.${extension}`;
            }

            const blob = new Blob([response.data]);

            // Use showSaveFilePicker for native save dialog
            if ('showSaveFilePicker' in window) {
                try {
                    const fileHandle = await window.showSaveFilePicker({
                        suggestedName: fileName,
                        types: [{
                            description: `${exportFormat.toUpperCase()} files`,
                            accept: {
                                [`application/${extension === 'zip' ? 'zip' : extension}`]: [`.${extension}`]
                            }
                        }]
                    });
                    
                    const writable = await fileHandle.createWritable();
                    await writable.write(blob);
                    await writable.close();
                    
                    onSuccess(`Document saved as ${fileHandle.name}!`);
                } catch (err) {
                    if (err.name === 'AbortError') {
                        onSuccess("Save cancelled.");
                    } else {
                        // Fallback to browser download
                        const url = window.URL.createObjectURL(blob);
                        const link = document.createElement('a');
                        link.href = url;
                        link.setAttribute('download', fileName);
                        
                        document.body.appendChild(link);
                        link.click();
                        link.remove();
                        window.URL.revokeObjectURL(url);

                        onSuccess(`Document exported as ${fileName}! (Save dialog failed)`);
                    }
                }
            } else {
                // Fallback to browser download
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.setAttribute('download', fileName);
                
                document.body.appendChild(link);
                link.click();
                link.remove();
                window.URL.revokeObjectURL(url);

                onSuccess(`Document exported as ${fileName}! (Save dialog not supported)`);
            }
        } catch (err) {
            onError(err.response?.data?.error || "Export failed");
        } finally {
            onLoadingChange(false);
        }
    };

    return (
        <div className="tab-content">
            <div className="export-section">
                <h2>Export Document</h2>
                <div className="export-options">
                    <div className="export-option">
                        <h3>Choose Export Format</h3>
                        <div className="format-selection">
                            <label>
                                <input 
                                    type="radio" 
                                    value="pdf" 
                                    checked={exportFormat === "pdf"}
                                    onChange={(e) => setExportFormat(e.target.value)}
                                />
                                PDF Document
                            </label>
                            <label>
                                <input 
                                    type="radio" 
                                    value="docx" 
                                    checked={exportFormat === "docx"}
                                    onChange={(e) => setExportFormat(e.target.value)}
                                />
                                Word Document
                            </label>
                            <label>
                                <input 
                                    type="radio" 
                                    value="images" 
                                    checked={exportFormat === "images"}
                                    onChange={(e) => setExportFormat(e.target.value)}
                                />
                                ZIP Document
                            </label>
                        </div>
                        
                        
                        <button 
                            onClick={handleExport}
                            disabled={isLoading}
                            className="export-btn"
                        >
                            {isLoading ? "Exporting..." : `Export as ${exportFormat.toUpperCase()}`}
                        </button>
                    </div>
                </div>
            </div>
            
        </div>
    );
};

export default ExportComponent;
