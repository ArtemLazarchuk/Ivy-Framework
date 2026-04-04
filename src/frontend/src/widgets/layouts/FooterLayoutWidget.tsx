import React from "react";
import { ScrollArea } from "@/components/ui/scroll-area";
import { cn } from "@/lib/utils";

interface FooterLayoutWidgetProps {
  slots?: {
    Footer?: React.ReactNode[];
    Content?: React.ReactNode[];
  };
}

export const FooterLayoutWidget: React.FC<FooterLayoutWidgetProps> = ({ slots }) => {
  const [hasMoreContent, setHasMoreContent] = React.useState(false);
  const scrollRef = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    const viewport = scrollRef.current?.querySelector("[data-radix-scroll-area-viewport]");
    if (!viewport) return;

    const handleScroll = () => {
      const { scrollTop, scrollHeight, clientHeight } = viewport;
      setHasMoreContent(scrollTop < scrollHeight - clientHeight - 1);
    };

    handleScroll();

    viewport.addEventListener("scroll", handleScroll);
    const resizeObserver = new ResizeObserver(handleScroll);
    resizeObserver.observe(viewport);

    return () => {
      viewport.removeEventListener("scroll", handleScroll);
      resizeObserver.disconnect();
    };
  }, []);

  if (!slots?.Footer || !slots?.Content) {
    return (
      <div className="text-red-500">
        Error: FooterLayout requires both Footer and Content slots.
      </div>
    );
  }

  return (
    <div className="h-full flex flex-col relative remove-parent-padding">
      <div ref={scrollRef} className="flex-1 min-h-0 overflow-hidden">
        <ScrollArea className="h-full">
          <div className="p-4">{slots.Content}</div>
        </ScrollArea>
      </div>
      <div
        className={cn(
          "flex-none w-full bg-background transition-shadow",
          hasMoreContent && "shadow-[0_-2px_4px_rgba(0,0,0,0.1)]",
        )}
      >
        <div className="border-t"></div>
        <div className="p-4">{slots.Footer}</div>
      </div>
    </div>
  );
};
