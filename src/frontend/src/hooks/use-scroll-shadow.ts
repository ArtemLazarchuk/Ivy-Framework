import { useEffect, useRef, useState } from "react";

/**
 * Hook that tracks scroll position to trigger shadow effects
 * @param selector - Optional selector for the scroll container (default: "[data-radix-scroll-area-viewport]")
 * @returns Object containing isScrolled state and scrollRef
 */
export function useScrollShadow(selector = "[data-radix-scroll-area-viewport]") {
  const [isScrolled, setIsScrolled] = useState(false);
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const viewport = scrollRef.current?.querySelector(selector);
    if (!viewport) return;

    const handleScroll = () => {
      setIsScrolled(viewport.scrollTop > 0);
    };

    viewport.addEventListener("scroll", handleScroll);
    return () => viewport.removeEventListener("scroll", handleScroll);
  }, [selector]);

  return { isScrolled, scrollRef };
}
