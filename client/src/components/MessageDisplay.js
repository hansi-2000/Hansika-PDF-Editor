import React from "react";

const MessageDisplay = ({ error, success, onClear }) => {
    return (
        <>
            {error && (
                <div className="message error">
                    <span>{error}</span>
                    <button onClick={onClear}>×</button>
                </div>
            )}
            {success && (
                <div className="message success">
                    <span>{success}</span>
                    <button onClick={onClear}>×</button>
                </div>
            )}
        </>
    );
};

export default MessageDisplay;



