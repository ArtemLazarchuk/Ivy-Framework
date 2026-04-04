import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

/**
 * Tests for the useScrollShadow hook.
 *
 * Since @testing-library/react is not available in this project, we verify
 * the source code contains the expected patterns. Visual behavior is covered
 * by the IvyFrameworkVerification end-to-end test.
 */
describe("useScrollShadow", () => {
  const hookSource = fs.readFileSync(path.resolve(__dirname, "./use-scroll-shadow.ts"), "utf-8");

  it("should export useScrollShadow function", () => {
    expect(hookSource).toContain("export function useScrollShadow");
  });

  it("should use useState for isScrolled state", () => {
    expect(hookSource).toContain("useState(false)");
  });

  it("should use useRef for scrollRef", () => {
    expect(hookSource).toContain("useRef<HTMLDivElement>(null)");
  });

  it("should default selector to radix scroll area viewport", () => {
    expect(hookSource).toContain("[data-radix-scroll-area-viewport]");
  });

  it("should accept a custom selector parameter", () => {
    expect(hookSource).toMatch(/function useScrollShadow\(\s*selector\s*=/);
  });

  it("should listen for scroll events and check scrollTop", () => {
    expect(hookSource).toContain('addEventListener("scroll"');
    expect(hookSource).toContain("scrollTop > 0");
  });

  it("should clean up the scroll event listener on unmount", () => {
    expect(hookSource).toContain('removeEventListener("scroll"');
  });

  it("should include selector in useEffect dependency array", () => {
    expect(hookSource).toContain("[selector]");
  });

  it("should return isScrolled and scrollRef", () => {
    expect(hookSource).toContain("return { isScrolled, scrollRef }");
  });
});

describe("HeaderLayoutWidget uses useScrollShadow hook", () => {
  const widgetSource = fs.readFileSync(
    path.resolve(__dirname, "../widgets/layouts/HeaderLayoutWidget.tsx"),
    "utf-8",
  );

  it("should import useScrollShadow", () => {
    expect(widgetSource).toContain('from "@/hooks/use-scroll-shadow"');
  });

  it("should use the hook instead of inline implementation", () => {
    expect(widgetSource).toContain("useScrollShadow()");
    // Should NOT have the inline scroll listener pattern
    expect(widgetSource).not.toContain('addEventListener("scroll"');
    expect(widgetSource).not.toContain("setIsScrolled");
  });

  it("should destructure isScrolled and scrollRef from the hook", () => {
    expect(widgetSource).toContain("const { isScrolled, scrollRef } = useScrollShadow()");
  });
});
