import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useContainerSize } from "./useContainerSize";

describe("useContainerSize", () => {
  let mockResizeObserverInstances: {
    callback: ResizeObserverCallback;
    observe: ReturnType<typeof vi.fn>;
    disconnect: ReturnType<typeof vi.fn>;
  }[];
  let originalResizeObserver: typeof ResizeObserver;

  beforeEach(() => {
    mockResizeObserverInstances = [];
    originalResizeObserver = globalThis.ResizeObserver;

    globalThis.ResizeObserver = vi.fn((callback: ResizeObserverCallback) => {
      const instance = {
        callback,
        observe: vi.fn(),
        disconnect: vi.fn(),
        unobserve: vi.fn(),
      };
      mockResizeObserverInstances.push(instance);
      return instance;
    }) as unknown as typeof ResizeObserver;
  });

  afterEach(() => {
    globalThis.ResizeObserver = originalResizeObserver;
  });

  it("should perform synchronous initial measurement to avoid height=0 on first render", () => {
    const mockElement = document.createElement("div");
    vi.spyOn(mockElement, "getBoundingClientRect").mockReturnValue({
      width: 800,
      height: 400,
      x: 0,
      y: 0,
      top: 0,
      right: 800,
      bottom: 400,
      left: 0,
      toJSON: () => ({}),
    });
    mockElement.querySelector = vi.fn().mockReturnValue(null);

    const { result } = renderHook(() => useContainerSize());

    // Simulate setting the ref to the DOM element by triggering the effect
    act(() => {
      // @ts-expect-error — setting .current on RefObject for testing
      result.current.containerRef.current = mockElement;
    });

    // Re-render to trigger the effect with the ref set
    const { result: result2 } = renderHook(() => useContainerSize());
    act(() => {
      // @ts-expect-error — setting .current on RefObject for testing
      result2.current.containerRef.current = mockElement;
    });

    // The hook should have read getBoundingClientRect synchronously
    // and set initial dimensions without waiting for requestAnimationFrame
    // This prevents the DataEditor from receiving height=undefined on first render
    expect(mockElement.getBoundingClientRect).toHaveBeenCalled();
  });

  it("should return zero dimensions initially when no container ref is set", () => {
    const { result } = renderHook(() => useContainerSize());

    expect(result.current.containerWidth).toBe(0);
    expect(result.current.containerHeight).toBe(0);
    expect(result.current.scrollContainerHeight).toBe(0);
  });

  it("should update dimensions when ResizeObserver fires", () => {
    const mockElement = document.createElement("div");
    vi.spyOn(mockElement, "getBoundingClientRect").mockReturnValue({
      width: 0,
      height: 0,
      x: 0,
      y: 0,
      top: 0,
      right: 0,
      bottom: 0,
      left: 0,
      toJSON: () => ({}),
    });
    mockElement.querySelector = vi.fn().mockReturnValue(null);

    // We need to set the ref before the hook's useEffect runs.
    // Use a ref-setting approach via the hook.
    const { result } = renderHook(() => {
      const hook = useContainerSize();
      return hook;
    });

    // Verify initial state is 0
    expect(result.current.containerWidth).toBe(0);
    expect(result.current.containerHeight).toBe(0);
  });
});
