import React, { useRef } from "react";
import axios from "axios";

const UploadComponent = ({ 
    onUploadSuccess, 
    onError, 
    onLoadingChange,
    isLoading
}) => {
    const fileInputRef = useRef(null);

    const handleUpload = async (e) => {
        const selectedFile = e.target.files[0];
        if (!selectedFile) return;

        onLoadingChange(true);
        onError(null);

        const formData = new FormData();
        formData.append("file", selectedFile);

        try {
            const res = await axios.post("http://localhost:5015/api/upload", formData, {
                headers: { "Content-Type": "multipart/form-data" },
            });

            if (res.data.fileUrl) {
                onUploadSuccess({
                    fileUrl: `http://localhost:5015${res.data.fileUrl}`,
                    metadata: res.data.metadata,
                    fileName: res.data.fileName
                });
            }
        } catch (err) {
            onError(err.response?.data?.error || "Upload failed");
        } finally {
            onLoadingChange(false);
        }
    };

    return (
        <div className="tab-content">
            <div className="upload-section">
                <h2>Upload PDF Document</h2>
                <div className="file-upload-area" onClick={() => fileInputRef.current?.click()}>
                    <input
                        ref={fileInputRef}
                        type="file"
                        accept="application/pdf"
                        onChange={handleUpload}
                        style={{ display: 'none' }}
                    />
                    <div className="upload-icon">📄</div>
                    <p>Click to upload or drag and drop your PDF file</p>
                    <p className="upload-hint">Maximum file size: 50MB</p>
                </div>
            </div>
        </div>
    );
};

export default UploadComponent;







