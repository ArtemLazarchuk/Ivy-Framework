# Testing Best Practices and Verification

## Playwright Test Project Setup

- Tests live in `.ivy/tests/` within the Ivy project
- Use `package.json` with `@playwright/test` dependency
- `playwright.config.ts` targets Chromium only, single worker, no retries
- Config uses `process.env.APP_PORT` for base URL
- **Viewport configuration**: Use `{ width: 1920, height: 1920 }` for square screenshots. Must be set in BOTH `use` and `projects[0].use` to override device preset defaults.

## Video Recording

- Enable via `video: { mode: 'on', dir: './videos' }` in both `use` and `projects[0].use` config
- Videos are saved as `.webm` files in the configured directory
- Use `test.afterEach` to rename videos with descriptive names matching test titles
- Access video path via `page.video()?.path()` — only available after the page context closes
- Videos add ~1-2 seconds overhead per test but provide invaluable debugging evidence
- `test.use({ video })` CANNOT be inside `test.describe()` — must be top-level in the file

## Test Structure Patterns

- `test.beforeAll` runs once per `test.describe` block. Multiple `test.describe` blocks = multiple `beforeAll` executions = multiple server spawns and potential DLL lock conflicts.
- For shared server: put all tests in one `test.describe`, or use top-level `test()` calls outside describe blocks which share the file-level `beforeAll`.
- Always kill app processes after test runs on Windows — lingering processes lock DLLs
- Screenshot/log path resolution: use `path.resolve(__dirname)` in spec files, NOT `process.cwd()`

## Common Test Categories

1. **Visibility tests** — verify UI elements render correctly
2. **Interaction tests** — click buttons, toggle switches, move sliders
3. **State change tests** — verify UI updates after interactions
4. **Output validation** — check generated/computed values appear correctly

## General Testing Tips

- Ivy apps may take a few seconds to start; 30s timeout is safe
- Use `shell: true` in spawn options on Windows
- Password/random generation tests: compare two outputs rather than asserting exact values
- Switch/toggle labels include the label text, use `getByText()` to find them
- After clicking a button, use `waitForTimeout(500)` or `waitFor()` before asserting changes
- Badges with strength/status text may appear multiple times; use `.first()`
- `UseDefaultApp()` means no Chrome sidebar — navigate directly to `/<app-id>?shell=false`
- `UseDefaultApp(typeof(App))` with single app — no Chrome, access via `/<app-id>?shell=false`
- For API-dependent tests, use `Promise.race()` with both success and error element waiters

## Widget-Specific Test Helpers

- `safeClick` pattern: try `.click({ timeout: 3000 })`, catch and fall back to `.dispatchEvent("click")` — handles both in-viewport and out-of-viewport elements
- `clickToggleOption` helper: check `[role="checkbox"]` count, then `[role="switch"]`, then fallback `dispatchEvent("click")` — robust across all BoolInput variants
- Sidebar search + `dispatchEvent("click")` pattern works reliably for Chrome tabs navigation

## Widget Library Testing

- Widget library projects have `.samples/` subfolder with the runnable app — tests must use `samplesDir` not `projectRoot` for `dotnet run`
- `[ExternalWidget]` components show "Unknown component type" when JS bundle isn't served — Framework issue
- External widget testing requires TWO frontend rebuilds: host app frontend AND the external widget frontend

## Clipboard Testing Limitations

- Clipboard `writeText` fails in headless Chromium with "Write permission denied" — this is expected and not an app bug
- Use `context.grantPermissions(["clipboard-read", "clipboard-write"])` for clipboard testing
- Verify clipboard content via `page.evaluate(() => navigator.clipboard.readText())`

## File Dialog Testing

- Chromium's `showOpenFilePicker`/`showSaveFilePicker`/`showDirectoryPicker` cannot be intercepted by Playwright — disable via `addInitScript` to fall back to `<input type="file">`
- Folder dialog `<input webkitdirectory>` requires passing directory path to `fileChooser.setFiles()`
- Save dialog fallback uses `<a download>` — capture with `page.waitForEvent('download')`

## Verifying CSS Properties

- Walk the DOM tree from text content upward to find elements with specific computed styles (e.g., `window.getComputedStyle(el).aspectRatio`)
- This is more reliable than data-testid selectors for layout/style verification
- For bounding box measurements: `element.getBoundingClientRect()` via `page.evaluate()`

## PNG Generation for Tests

- To generate valid PNG images for Playwright tests: use `zlib.deflateSync()` on raw pixel data (filter byte 0 + RGB per row), then construct PNG chunks (IHDR, IDAT, IEND) with proper CRC32 checksums
- Invalid/minimal base64 PNGs will cause `InvalidImageContentException`

## Fake Camera Testing

- Chromium fake device flags produce `videoWidth=0` and `videoHeight=0` in headless mode
- Upload-dependent assertions cannot be verified with fake cameras — test UI state transitions instead
- Set config: `permissions: ['camera']` and launch args for fake media stream

## Run History

### 2026-03-17 — UseFileDialog/UseSaveDialog/UseFolderDialog Hooks
- File dialog hooks with browser API detection (File System Access API → fallback input)
- **BUG FOUND**: `FileDialogMode.Upload` (enum value 0) stripped by WidgetSerializer → `mode` undefined on frontend → upload silently skipped. Fixed with `mode = 'Upload'` default in `FileDialogWidget.tsx`
- 6 tests, 3 fix rounds, all passed

### 2026-03-13 — CameraInput Widget
- MUST rebuild frontend when testing commits with new frontend widgets
- Fake camera in headless Chromium produces 0x0 video
- 21 tests, 5 fix rounds, all passed

### 2026-03-13 — Test.CSSGradientTextGenerator
- `CodeBlock` with `UseEffect` renders EMPTY on initial load
- 8 tests, 2 fix rounds, all passed

### 2026-03-13 — Test.LifeInWeeksVisualizer
- `Layout.Grid(3)` renders "3" as text — grid column count bug
- `new Html(gridHtml)` with CSS grid completely invisible
- 7 tests, 1 fix round, all passed

### 2026-03-13 — Test.SimpleCRM
- `decimal` column in DataTable renders as `00000000000000000`
- glide-data-grid cell `.dispatchEvent("click")` passes but does NOT trigger `OnCellClick`
- 9 tests, 3 fix rounds, all passed

### 2026-03-10 — Tempus.AgeCalc
- `state.ToDateInput()` renders as popover trigger, NOT native date input
- `Card.Title("X")` does NOT render as heading
- 5 tests, all passed

### 2026-03-10 — ReversiForge.AI
- Single-app Chrome tabs auto-opens
- `networkidle` hangs on Ivy apps — use `domcontentloaded`
- All passed

### 2026-03-10 — Polyglot.TypeTrainer
- `new Html(...)` invisible in iframe
- `SelectInput` with overlapping prefixes cause strict mode violations
- All passed

### 2026-03-10 — CineStream.Converter
- `AsQueryable().ToDataTable()` — use `toBeAttached()` not `toBeVisible()`
- Multiple `test.describe` blocks cause server re-spawn failures
- All passed

### 2026-03-09 — Parsely.Markflow
- CodeMirror requires clipboard paste, NOT `keyboard.type()` or `keyboard.insertText()`
- 7 tests, all passed

### 2026-03-10 — ByteForge.UrlCraft
- Toggle variant renders as radio buttons with `role="radio"`
- Toast messages produce duplicate text nodes — use `{ exact: true }`
- All passed

### 2026-03-10 — Nexus.PasswordForge2
- Radix slider, NOT native range input
- BoolInput checkbox: `getByRole("checkbox", { name })` and `getByLabel()` fail
- All passed

### 2026-03-10 — Skyline.RunwayCalculator
- Card headers cause strict mode violations — use `getByRole("heading")`
- `state.ToMoneyInput()` renders formatted currency
- All passed

### 2026-03-09 — Chromatica.Palettes2
- `state.ToTextInput()` as `role="textbox"`
- API-dependent tests: `Promise.race()` pattern
- All passed

### 2026-03-10 — Numerix.Statistics
- ValueTuple crash — switched to `|` operator
- `.WithField()` crashes in tuple context
- All passed

### 2026-03-10 — Ivy.TextAnnotate (Widget Library)
- `.samples/` subfolder for runnable app
- All passed

### 2026-03-10 — Calc.Desktop
- Single-app Chrome auto-opens
- Short button names conflict with sidebar
- All passed

### 2026-03-10 — Nexus.DecisionMatrix
- Card `.Title("X")` does NOT render as heading
- `.WithField().Label("X")` does NOT create `<label for="">` association
- All passed

### 2026-03-10 — Nexus.HumanCore
- Chrome tabs start with NO tab open
- Sidebar items may be outside viewport — use `dispatchEvent("click")`
- All passed

### 2026-03-10 — Nexus.CrmPortal
- Complex CRUD app with 4 apps, 15 tests, all passed
- List sort order matters with `OrderByDescending`
- All passed

### 2026-03-10 — Clockwise.MeetingCost
- Primary constructor injection crashes with `MissingMethodException`
- All passed

### 2026-03-10 — PawByte.Tamadog
- Clean pass, sidebar search + `dispatchEvent("click")` pattern
- All passed

### 2026-03-10 — Flowcraft.MermaidStudio
- Markdown with mermaid code blocks renders SVG diagrams
- `UseDownload` renders as `role="link"`
- All passed

### 2026-03-10 — Patternix.RegexLens
- `ToBoolInput().Label("X")` without Variant → `role="checkbox"`
- Dynamic checkbox/switch detection pattern
- All passed

### 2026-03-10 — HoofTrack.StableVault
- `ListItem` click by `filter({ hasText })` fails — use `getByText` + `dispatchEvent`
- `page.goBack()` doesn't restore blade state
- All passed

### 2026-03-10 — Folio.TextMiner
- File upload with `UseUpload` and `setInputFiles()` works
- All passed

### 2026-03-10 — Pinnacle.StockGrid
- YahooFinanceApi v2.3.3 returns 401 — package broken
- Unhandled async exceptions crash server — always try/catch
- All passed

### 2026-03-10 — Archiva.ZipForge
- `FileUpload<byte[]>.ToTable()` with `.Remove(e => e.Content)` throws `KeyNotFoundException`
- 8 tests, 1 fix round, all passed

### 2026-03-10 — Pixelforge.AsciiCraft
- Valid PNG generation requires proper CRC32 checksums
- `beforeAll` timeout needs 120s for recompilation
- 12 tests, 1 fix round, all passed

### 2026-03-11 — Meridian.StockGrid
- Column headers also hidden in virtual rendering — `toBeAttached()`
- YahooFinanceApi confirmed broken again
- 8 tests, 1 fix round, all passed

### 2026-03-11 — StockFlow.Inventory
- `ToAsyncSelectInput` with nullable types: use `Option<int?>` not `Option<int>`
- Complex CRUD with 6 apps, 15 tests, all passed

### 2026-03-11 — Meridian.ClockFace
- Real-time timer updates NOT reliably observable in Playwright
- Clean run: 12 tests, all passed

### 2026-03-11 — NexTask.TodoSync
- `WithConfirm` uses "Ok" as confirm button text
- 12 tests, all passed

### 2026-03-11 — LinguaFlow.Translator
- 100+ language options — need `{ exact: true }` for prefix collisions
- 12 tests, all passed

### 2026-03-11 — GridQuest.Pathfinder
- `UseState<T>(T?)` vs `UseState<T>(Func<T>)` ambiguity
- 14 tests, all passed

### 2026-03-12 — CourtVision.Analytics
- CSV decimal parsing: `int.TryParse("25.0")` returns false
- 10 tests, 2 fix rounds, all passed

### 2026-03-12 — Meridian.ProductVault
- `.ToForm()` renders labels with `*` suffix for `[Required]` fields
- 12 tests, 1 fix round, all passed

### 2026-03-12 — Test.PomodoroTimer
- Timer `Elapsed` events race with disposal — add guard check
- 6 tests, 1 project fix + 4 test fixes, all passed

### 2026-03-12 — Test.DataVisualization
- `DataTableBuilder.Header` with indexer expressions crashes
- `DataTableBuilder` has no `.Remove()` method
- 11 tests, 1 fix round, all passed

### 2026-03-12 — Test.UUIDGenerator2
- Enum names with PascalCase splitting: `V1` → "V 1"
- 13 tests, all passed

### 2026-03-12 — Test.PivotTableBuilder
- Multi-select Toggle variant: `aria-pressed`, NOT `role="radio"`
- 12 tests, all passed

### 2026-03-12 — Test.WebhookTester
- Webhook URLs at `/ivy/webhook/<guid>`
- GET and POST only
- 11 tests, all passed

### 2026-03-12 — Test.UTMParameterBuilder
- `Dictionary.ToDetails()` crashes — use anonymous object
- `Layout.Tabs` renders BOTH tab panels in DOM
- 13 tests, 1 fix round, all passed

### 2026-03-12 — Test.TimezoneWorldClock
- Card `.Title().Content()` renders invisible body
- IANA vs Windows timezone IDs
- 12 tests, 3 fix rounds, all passed

### 2026-03-12 — Test.APIRequestBuilder
- `Layout.Vertical(IEnumerable<T>, otherItem)` renders as debug table
- 13 tests, 1 fix round, all passed

### 2026-03-12 — Test.CountdownTimer
- `UseArgs<T>` reads from WebSocket, not browser URL
- `IHttpContextAccessor.HttpContext` is null in WebSocket context
- 7 tests, 3 fix rounds, all passed

### 2026-03-13 — AspectRatio (WidgetBase property)
- CSS `aspect-ratio` overridden by flex stretch in `Layout.Horizontal()`
- 6 tests, 2 fix rounds, all passed

### 2026-03-13/15 — FirstDayOfWeek (DateTimeInput & DateRangeInput) — FIXED
- `DayOfWeek` enum serializes as string but picker expects numeric
- react-day-picker v9 uses flex layout, NOT `<table>`
- 8 tests, 3 fix rounds, all passed

### 2026-03-13 — Test.ScatterChart
- Frontend TypeScript errors block widget rendering
- 0 tests executed (blocked by frontend issues)

### 2026-03-15 — ExpressionNameHelper RadarChart Fix
- ECharts indicator labels on canvas, NOT in DOM
- Access chart data via React fiber `memoizedProps.option`
- 5 tests, 4 fix rounds, all passed

### 2026-03-16 — FeedbackInputMax
- Star rating SVG locator: use `page.locator('button svg')`
- Decimal formatting locale: use `CultureInfo.InvariantCulture`
- 20 tests, 4 fix rounds, all passed

### 2026-03-16 — Copy Page (Ivy Docs)
- Ivy Docs at `src/Ivy.Docs/` — use directly for docs testing
- PathToAppIdMiddleware rewrites ALL non-static paths
- All passed

### 2026-03-18 — Optimistic Rendering (All Input Widgets)
- Input widget `.TestId()` wraps Field container, not inner element
- Range inputs use tuple state
- Rapid interaction tests work reliably with optimistic rendering

### 2026-03-26 — LineChart Empty Render Bug
- Canvas presence ≠ visual content — charts can render empty
- Case-insensitive dataKey matching now applied to LineChart/BarChart
- 12 tests, all passed

### 2026-03-26 — Markdown OnLinkClick Event
- Fires for ALL URL types (http, https, app://, relative, root-relative, anchor)
- Lambda overload ambiguity: explicitly type parameter
- 20 tests, 1 fix round, all passed

### 2026-03-26 — Markdown Local Image Support (FIXED)
- `.DangerouslyAllowLocalFiles()` preserves file:// URLs
- Browser blocking of file:// is expected behavior
- 18 tests, all passed

### 2026-03-29 — Icons.X in Markdown Inline Code (BUG FOUND)
- react-markdown v10 removed `inline` prop — icon detection never triggers
- Fix: replace `if (inline)` with `if (!className)`
- 11 tests, 2 fix rounds, all passed

### 2026-03-30 — Badge inline-flex Fix in Tables
- Badge CVA base changed from `flex` to `inline-flex`
- `Layout.Horizontal().Gap()` takes `bool`
- 12 tests, 4 fix rounds, all passed

### 2026-03-30 — NumberInput/ColorInput Height Alignment
- ColorInput `.TestId()` not rendered — use `input[type='color']`
- Layout `|` operator is left-associative
- 14 tests, 6 fix rounds, all passed

### 2026-03-30 — DiffView External Widget JSX Runtime Fix
- External widget testing requires TWO frontend rebuilds
- DiffView OnLineClick: use `with { OnLineClick = ... }` expression
- 11 tests, 0 fix rounds, all passed

### 2026-03-31 — Markdown Popover Links
- `[text](## "content")` syntax renders `<span role="button">` with Radix Popover
- Only one popover visible at a time
- 16 tests, 1 fix round, all passed

### 2026-03-31 — Nested Code Block Rendering Fix
- `normalizeNestedFences` preprocessor handles nested backtick fences
- 18 tests, 1 fix round, all passed

### 2026-03-31 — AsyncSelectInput State Update Bug Fix
- Opens Sheet with search input and ListItem options
- OnBlur test: use `.focus()` + `Tab` instead of click
- 17 tests, 1 fix round, all passed
