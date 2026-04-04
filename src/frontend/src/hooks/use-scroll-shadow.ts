import { useEffect, useRef, useState } from "react";

export type ScrollShadowDirection = "top" | "bottom";

/**
 * Hook that tracks scroll position to trigger shadow effects
 * @param selector - Optional selector for the scroll container (default: "[data-radix-scroll-area-viewport]")
 * @param direction - Shadow direction: "bottom" shows shadow when scrolled down from top, "top" shows shadow when not at bottom (default: "bottom")
 * @returns Object containing isScrolled state and scrollRef
 */
export function useScrollShadow(
  selector = "[data-radix-scroll-area-viewport]",
  direction: ScrollShadowDirection = "bottom",
) {
  const [isScrolled, setIsScrolled] = useState(false);
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const viewport = scrollRef.current?.querySelector(selector);
    if (!viewport) return;

    const handleScroll = () => {
      if (direction === "bottom") {
        setIsScrolled(viewport.scrollTop > 0);
      } else {
        const { scrollTop, scrollHeight, clientHeight } = viewport;
        setIsScrolled(scrollTop < scrollHeight - clientHeight - 1);
      }
    };

    handleScroll();

    viewport.addEventListener("scroll", handleScroll);

    const resizeObserver = direction === "top" ? new ResizeObserver(handleScroll) : null;
    if (resizeObserver) {
      resizeObserver.observe(viewport);
    }

    return () => {
      viewport.removeEventListener("scroll", handleScroll);
      resizeObserver?.disconnect();
    };
  }, [selector, direction]);

  return { isScrolled, scrollRef };
}
