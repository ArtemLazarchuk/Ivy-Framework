# Review: Fix Minified React Error #130

## Changes Made

1. **MemoizedWidget.tsx** - Added `WidgetErrorBoundary` class that wraps each widget render, preventing single widget errors from crashing the entire React app. Improved the unknown widget type check to also validate the component is a function or object.

2. **widgetRenderer.tsx** - Changed `node.type in widgetMap` to `Object.hasOwn(widgetMap, node.type)` to avoid prototype chain false positives. Added `console.warn` for unknown widget types with widget ID for debugging.

3. **MetricViewTestApp.cs** - New sample app exercising MetricView with various states (normal, no goal, error).

## Manual Verification Checklist

- [ ] Run the Seattle Weather demo (from Foo.Bar.zip repro case) and confirm no React Error #130
- [ ] Open browser console and verify no `[Ivy] Unknown widget type` warnings appear
- [ ] Verify MetricView cards display correctly: metric values, trend arrows, progress bars
- [ ] Verify charts (line, pie, bar) render without errors
- [ ] Run the new MetricViewTestApp sample and verify all 4 metric cards render:
  - "Test Metric" with progress bar and goal
  - "Another Metric" with negative trend
  - "No Goal Metric" without progress bar
  - "Error Metric" showing error state
- [ ] Intentionally break a widget (e.g., pass invalid data) and verify the error boundary shows an inline error message instead of crashing the page
