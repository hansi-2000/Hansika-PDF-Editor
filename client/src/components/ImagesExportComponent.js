import React, { useState } from "react";
import axios from "axios";

const ImagesExportComponent = ({ 
    onError, 
    onSuccess,
    onLoadingChange,
    isLoading,
    highlightedPdfUrl
}) => {
    const [exportFormat, setExportFormat] = useState("png");
    const [imageQuality, setImageQuality] = useState("high");

    const handleExport = async () => {
        if (!highlightedPdfUrl) {
            onError("No PDF available for export");
            return;
        }

        onLoadingChange(true);
        onError(null);

        try {
            const response = await axios.post("http://localhost:5015/api/ImagesExport/export", {
                pdfUrl: highlightedPdfUrl,
                format: exportFormat,
                quality: imageQuality
            }, {
                responseType: 'blob'
            });

            // Create download link
            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', `exported_images.${exportFormat}`);
            document.body.appendChild(link);
            link.click();
            link.remove();
            window.URL.revokeObjectURL(url);

            onSuccess("Images exported successfully!");
        } catch (err) {
            onError(err.response?.data?.error || "Export failed");
        } finally {
            onLoadingChange(false);
        }
    };

    return (
        <div className="export-option">
            <h3>Export as Images</h3>
            <div className="export-settings">
                <div className="setting-group">
                    <label>Format:</label>
                    <select 
                        value={exportFormat} 
                        onChange={(e) => setExportFormat(e.target.value)}
                    >
                        <option value="png">PNG</option>
                        <option value="jpg">JPG</option>
                        <option value="tiff">TIFF</option>
                    </select>
                </div>
                <div className="setting-group">
                    <label>Quality:</label>
                    <select 
                        value={imageQuality} 
                        onChange={(e) => setImageQuality(e.target.value)}
                    >
                        <option value="high">High</option>
                        <option value="medium">Medium</option>
                        <option value="low">Low</option>
                    </select>
                </div>
            </div>
            <button 
                onClick={handleExport}
                disabled={isLoading}
                className="export-btn"
            >
                {isLoading ? "Exporting..." : "Export Images"}
            </button>
        </div>
    );
};

export default ImagesExportComponent;

