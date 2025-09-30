import React, { useState } from "react";
import axios from "axios";

const PdfExportComponent = ({ 
    onError, 
    onSuccess,
    onLoadingChange,
    isLoading,
    highlightedPdfUrl
}) => {
    const [exportOptions, setExportOptions] = useState({
        includeAnnotations: true,
        includeMetadata: true,
        compressionLevel: "medium"
    });

    const handleExport = async () => {
        if (!highlightedPdfUrl) {
            onError("No PDF available for export");
            return;
        }

        onLoadingChange(true);
        onError(null);

        try {
            const response = await axios.post("http://localhost:5015/api/PdfExport/export", {
                pdfUrl: highlightedPdfUrl,
                options: exportOptions
            }, {
                responseType: 'blob'
            });

            // Create download link
            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', 'exported_document.pdf');
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);

            onSuccess("PDF exported successfully!");
        } catch (err) {
            onError(err.response?.data?.error || "Export failed");
        } finally {
            onLoadingChange(false);
        }
    };

    return (
        <div className="export-option">
            <h3>Export PDF</h3>
            <div className="export-settings">
                <div className="setting-group">
                    <label>
                        <input 
                            type="checkbox" 
                            checked={exportOptions.includeAnnotations}
                            onChange={(e) => setExportOptions({
                                ...exportOptions,
                                includeAnnotations: e.target.checked
                            })}
                        />
                        Include Annotations
                    </label>
                </div>
                <div className="setting-group">
                    <label>
                        <input 
                            type="checkbox" 
                            checked={exportOptions.includeMetadata}
                            onChange={(e) => setExportOptions({
                                ...exportOptions,
                                includeMetadata: e.target.checked
                            })}
                        />
                        Include Metadata
                    </label>
                </div>
                <div className="setting-group">
                    <label>Compression:</label>
                    <select 
                        value={exportOptions.compressionLevel}
                        onChange={(e) => setExportOptions({
                            ...exportOptions,
                            compressionLevel: e.target.value
                        })}
                    >
                        <option value="low">Low</option>
                        <option value="medium">Medium</option>
                        <option value="high">High</option>
                    </select>
                </div>
            </div>
            <button 
                onClick={handleExport}
                disabled={isLoading}
                className="export-btn"
            >
                {isLoading ? "Exporting..." : "Export PDF"}
            </button>
        </div>
    );
};

export default PdfExportComponent;

