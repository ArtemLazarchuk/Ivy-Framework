import React from 'react';
import { validateImageUrl } from '@/lib/url';

interface ImageOverlayProps {
  src: string | undefined;
  alt: string | undefined;
  onClose: () => void;
}

export const ImageOverlay: React.FC<ImageOverlayProps> = ({
  src,
  alt,
  onClose,
}) => {
  // Handle click on the overlay background to close it
  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  // Validate and sanitize image URL to prevent open redirect vulnerabilities
  const validatedSrc = src ? validateImageUrl(src) : null;
  if (!validatedSrc) {
    // Invalid URL, don't render image
    return null;
  }

  return (
    <div
      className="fixed inset-0 bg-black/70 flex items-center justify-center z-50 cursor-zoom-out"
      onClick={handleBackdropClick}
      role="presentation"
      onKeyDown={e => e.key === 'Escape' && onClose()}
    >
      <div className="relative max-w-[90vw] max-h-[90vh]">
        <img
          src={validatedSrc}
          alt={alt}
          className="max-w-full max-h-[90vh] object-contain"
        />
        <button
          className="absolute top-4 right-4 bg-black/50 text-white rounded-full w-8 h-8 flex items-center justify-center"
          onClick={onClose}
        >
          ✕
        </button>
      </div>
    </div>
  );
};
