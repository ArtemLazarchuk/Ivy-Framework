import React from "react";
import { X } from "lucide-react";
import { cva } from "class-variance-authority";
import { Button } from "@/components/ui/button";
import { Densities } from "@/types/density";
import { FileItem, FileUploadStatus } from "./types";
import { formatBytes } from "../file-input-validation";

const compactContainerVariant = cva("flex flex-wrap", {
  variants: {
    density: {
      Small: "gap-1 px-2 pb-1",
      Medium: "gap-2 px-3 pb-2",
      Large: "gap-2 px-3 pb-2",
    },
  },
  defaultVariants: { density: "Medium" },
});

const compactItemVariant = cva(
  "flex items-center border border-muted-foreground/25 rounded-md bg-muted/30",
  {
    variants: {
      density: {
        Small: "gap-1 px-1.5 py-0.5 text-[10px]",
        Medium: "gap-2 px-2 py-1 text-xs",
        Large: "gap-2 px-2.5 py-1.5 text-sm",
      },
    },
    defaultVariants: { density: "Medium" },
  },
);

const compactCancelVariant = cva("shrink-0 p-0", {
  variants: {
    density: {
      Small: "h-4 w-4",
      Medium: "h-5 w-5",
      Large: "h-6 w-6",
    },
  },
  defaultVariants: { density: "Medium" },
});

const compactCancelIconVariant = cva("", {
  variants: {
    density: {
      Small: "h-2.5 w-2.5",
      Medium: "h-3 w-3",
      Large: "h-4 w-4",
    },
  },
  defaultVariants: { density: "Medium" },
});

interface FileAttachmentListProps {
  files: FileItem[];
  onCancel?: (fileId: string) => void;
  hasCancelHandler: boolean;
  variant?: "compact" | "card";
  density?: Densities;
}

export const FileAttachmentList: React.FC<FileAttachmentListProps> = ({
  files,
  onCancel,
  hasCancelHandler,
  variant = "compact",
  density = Densities.Medium,
}) => {
  if (files.length === 0) return null;

  if (variant === "card") {
    return (
      <div className="space-y-2">
        {files.map((file) => {
          const isLoading = file.status === FileUploadStatus.Loading;
          const fileProgress = file.progress ?? 0;

          return (
            <div
              key={file.id}
              data-file-item
              className="flex items-center gap-3 p-3 border border-muted-foreground/25 rounded-md bg-transparent"
            >
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium truncate">{file.fileName}</p>
                {isLoading && (
                  <div className="mt-2">
                    <div className="w-full bg-muted rounded-full h-1.5">
                      <div
                        className="bg-primary h-1.5 rounded-full transition-all duration-300"
                        style={{ width: `${fileProgress * 100}%` }}
                      />
                    </div>
                  </div>
                )}
              </div>
              {hasCancelHandler && (
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8 shrink-0"
                  onClick={(e) => {
                    e.stopPropagation();
                    onCancel?.(file.id);
                  }}
                >
                  <X className="h-4 w-4" />
                </Button>
              )}
            </div>
          );
        })}
      </div>
    );
  }

  return (
    <div className={compactContainerVariant({ density })}>
      {files.map((file) => {
        const isLoading = file.status === FileUploadStatus.Loading;
        return (
          <div key={file.id} data-file-item className={compactItemVariant({ density })}>
            <span className="truncate max-w-[150px]" title={file.fileName}>
              {file.fileName}
            </span>
            <span className="text-muted-foreground whitespace-nowrap">
              {formatBytes(file.length)}
            </span>
            {isLoading && (
              <div className="w-12 bg-muted rounded-full h-1">
                <div
                  className="bg-primary h-1 rounded-full transition-all duration-300"
                  style={{ width: `${file.progress * 100}%` }}
                />
              </div>
            )}
            {hasCancelHandler && (
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className={compactCancelVariant({ density })}
                onClick={(e) => {
                  e.stopPropagation();
                  onCancel?.(file.id);
                }}
              >
                <X className={compactCancelIconVariant({ density })} />
              </Button>
            )}
          </div>
        );
      })}
    </div>
  );
};
