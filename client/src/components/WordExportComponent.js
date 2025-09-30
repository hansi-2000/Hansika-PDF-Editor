import React, { useState } from "react";
import axios from "axios";

const WordExportComponent = ({ 
    onError, 
    onSuccess,
    onLoadingChange,
    isLoading,
    highlightedPdfUrl
}) => {
    const [exportOptions, setExportOptions] = useState({
        includeImages: true,
        preserveFormatting: true,
        extractTextOnly: false
    });

    const handleExport = async () => {
        if (!highlightedPdfUrl) {
            onError("No PDF available for export");
            return;
        }

        onLoadingChange(true);
        onError(null);

        try {
            const response = await axios.post("http://localhost:5015/api/WordExport/export", {
                pdfUrl: highlightedPdfUrl,
                options: exportOptions
            }, {
                responseType: 'blob'
            });

            // Create download link
            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', 'exported_document.docx');
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);

            onSuccess("Word document exported successfully!");
        } catch (err) {
            onError(err.response?.data?.error || "Export failed");
        } finally {
            onLoadingChange(false);
        }
    };

    return (
        <div className="export-option">
            <h3>Export as Word Document</h3>
            <div className="export-settings">
                <div className="setting-group">
                    <label>
                        <input 
                            type="checkbox" 
                            checked={exportOptions.includeImages}
                            onChange={(e) => setExportOptions({
                                ...exportOptions,
                                includeImages: e.target.checked
                            })}
                        />
                        Include Images
                    </label>
                </div>
                <div className="setting-group">
                    <label>
                        <input 
                            type="checkbox" 
                            checked={exportOptions.preserveFormatting}
                            onChange={(e) => setExportOptions({
                                ...exportOptions,
                                preserveFormatting: e.target.checked
                            })}
                        />
                        Preserve Formatting
                    </label>
                </div>
                <div className="setting-group">
                    <label>
                        <input 
                            type="checkbox" 
                            checked={exportOptions.extractTextOnly}
                            onChange={(e) => setExportOptions({
                                ...exportOptions,
                                extractTextOnly: e.target.checked
                            })}
                        />
                        Extract Text Only
                    </label>
                </div>
            </div>
            <button 
                onClick={handleExport}
                disabled={isLoading}
                className="export-btn"
            >
                {isLoading ? "Exporting..." : "Export Word"}
            </button>
        </div>
    );
};

export default WordExportComponent;

