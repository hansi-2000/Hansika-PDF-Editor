import React from "react";

const LoadingSpinner = ({ isLoading, message = "Processing..." }) => {
    if (!isLoading) return null;

    return (
        <div className="loading-overlay">
            <div className="spinner"></div>
            <p>{message}</p>
        </div>
    );
};

export default LoadingSpinner;








