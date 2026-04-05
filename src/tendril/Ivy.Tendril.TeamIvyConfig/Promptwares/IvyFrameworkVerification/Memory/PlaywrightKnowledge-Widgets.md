# Widget-Specific Patterns

## Ivy UI Components Overview

- `Layout.Center()`, `Layout.Vertical()`, `Layout.Horizontal()` — layout containers
- `Text.H2()`, `Text.P()`, `Text.Label()`, `Text.InlineCode()` — text elements
- `Slider` — range input, configured with `.Min()`, `.Max()`, `.Step()`
- `Switch` — toggle input, configured with `.Label()`
- `Button` — click handler, configured with `.Icon()`, `.Variant()`, `.Disabled()`
- `Badge` — status label with variant (Destructive, Warning, Success, Info, Secondary)
- `Card` — container card component. Note: Card supports pipe `|` for children but Text elements (H3, P) added directly as Card children may not render; use `Layout.Vertical()` inside Card for mixed content
- `Json` — displays formatted JSON with syntax highlighting and collapsible tree. Constructors: `Json(JsonNode)`, `Json(object)` (auto-serializes), `Json(string)`. Props: `Expanded` (null=collapsed, N=depth, -1=fully expanded). Default is collapsed — use `{ Expanded = -1 }` for testing to make content visible
- `Xml` — displays formatted XML with syntax highlighting and collapsible tree. Constructors: `Xml(XObject)`, `Xml(string)`. Props: `Expanded` (null=collapsed, 0=collapsed, N=depth, -1=fully expanded). Same behavior as Json widget. `.TestId()` does NOT render `data-testid` in the DOM — use text-based or heading-relative locators instead
- `Toast` — notification via `IClientProvider.Toast(message, type)`
- State bindings: `state.ToSliderInput()`, `state.ToSwitchInput()`

## DataTable

### DataTable Testing Patterns

- DataTable uses Glide Data Grid which renders as `<canvas>` elements, NOT HTML `<table>`
- Canvas locator: `page.locator('[data-testid="data-grid-canvas"]')` or `page.locator('canvas').first()`
- **Click target**: Use `page.locator('.dvn-scroller').first().click({ position: { x, y } })` — the `dvn-scroller` overlay div intercepts pointer events over the canvas
- Wait for table: `await page.waitForSelector('canvas', { timeout: 15000 })`; then wait 1s for render
- Stale process cleanup: Always kill previous `DataTableAutoWidth.exe` processes before running tests — they lock DLLs and cause build failures
- Use `--no-build` in test spawn after pre-building: `spawn('dotnet', ['run', '--no-build', '--', '--port', port])`

### DataTable Gotchas

- **DataTable uses virtualized grid rendering** — cells are `<td role="gridcell">` elements that Playwright considers "hidden" even when data is present. Use `page.locator('[role="gridcell"]').first()` with `.toBeAttached()` instead of `.toBeVisible()` to verify data loaded.
- **`.Remove()` on positional records: crash fixed but data alignment broken** — Commit `e08a55d6` fixes the crash (fills removed params with defaults in constructor). However, the DataTable serializes ALL property values (including defaults for removed fields) but maps them by index to only the visible column headers, causing a systematic data shift.
- **Decimal columns display as `0000000000000000`** — `decimal` values in DataTable grid render incorrectly (framework bug). Root cause: `useRowData.ts` reads raw Arrow Decimal128 values via `column.get(i)` without applying `10^scale` division.
- **Reading DataTable cell values in Playwright**: Since DataTable renders on `<canvas>`, DOM text assertions don't work. Use React fiber introspection: find a `canvas` element, walk `__reactFiber` tree upward to find a component with `getCellContent` prop, then call `getCellContent([col, row])` to get `{ kind, data, displayData }`. Column 0 = first visible column.
- **DataTable shows ALL model properties, not just those with `.Header()` calls**: The `.Header()` method only renames a column label and optionally sets a `ValueAccessor`. All properties of the model type appear as columns in the grid.
- **One ValueAccessor per root property**: Two `.Header()` calls targeting different sub-properties of the same root property share one column — only the first `ValueAccessor` is stored.
- **`DataTableBuilder` does NOT have a `.Remove()` method** — `builder.Remove(expressions)` resolves to `CollectionExtensions.Remove<TKey,TValue>` and fails to compile
- **Column headers with virtualized rendering**: `role="columnheader"` elements are "hidden" (glide-data-grid virtual rendering) — use `toBeAttached()` not `toBeVisible()` for column headers too
- **glide-data-grid cell clicking**: gridcell elements are hidden behind the canvas overlay. `.click()` and `.click({ force: true })` both fail. Use `.dispatchEvent("click")` to bypass visibility — but note this does NOT trigger Ivy's `OnCellClick` handler (the canvas intercepts real mouse events).

## SelectInput

### SelectInput Multi-Select vs Single-Select

- **Single-select** (Select variant): Uses Radix Select — trigger is `button[role="combobox"]`, options are `[role="option"]`
- **Multi-select** (Select variant): Uses CMDK (Command Menu) in a Popover — trigger is a div with input, options are `[cmdk-item]`
- To open multi-select: `page.getByPlaceholder('placeholder text').click()`
- To select option in multi-select: `page.locator('[cmdk-item]').filter({ hasText: 'Option' }).click()`
- Multi-select popover stays open after selecting — close with `page.keyboard.press('Escape')`
- Visual group separators appear between option groups in the dropdown (Select variant only)

### SelectInput Slider Variant

- `state.ToSelectInput(options).Slider()` renders a discrete range slider using Radix UI `<Slider>` with `role="slider"`
- Use `page.getByRole('slider')` to locate; keyboard interaction with ArrowRight/ArrowLeft (step), Home/End (min/max)
- Does NOT support `selectMany` — logs a warning and falls back to single-select

### SelectInput Trigger Display

- **SelectInput trigger displays all option labels**: `SelectInput<T>` with many options renders ALL option display names concatenated in the trigger button (combobox). This makes `getByText("OptionName")` ambiguous — it matches both the trigger and the dropdown item. Use `page.evaluate` + coordinate clicking.
- **SelectInput dropdown structure**: Trigger is `<button role="combobox">`. Dropdown panel is `<div role="listbox">`. Items inside may have NO `role="option"` — they are plain `<div>` elements with text content.
- `state.ToSelectInput(options)` WITHOUT `.Variant(SelectInputVariants.Toggle)` renders as a Radix dropdown (not native `<select>`) — click the trigger text to open, then `getByText("Option", { exact: true }).first().click()` to select.
- **Searchable SelectInput dropdown**: `getByRole("combobox").first().click()` opens the dropdown, `keyboard.type("search")` filters, `getByRole("option").first().click()` selects.

### Multi-Select Toggle Variant

- `IState<string[]>` with `.Variant(SelectInputVariant.Toggle)` renders as Radix toggle buttons with `aria-pressed` attribute, NOT `role="radio"` or `role="checkbox"`. Use `page.getByText("Option", { exact: true }).first().click()`.
- When Toggle row field options overlap with dropdown option names, `getByText("Quantity").first()` matches the TOGGLE button, not the dropdown option. Use `getByRole("option", { name: "Quantity", exact: true })` to select dropdown items when a popover/dropdown is open.

### AsyncSelectInput

- AsyncSelectInput opens a **Sheet** (`role="dialog"`) with search input and ListItem options
- The trigger button contains placeholder text as `<span>` — use `getByRole('button', { name: 'placeholder text' })` to click the trigger
- Inside the sheet: `sheet.getByPlaceholder('Search')` for the search input, `sheet.getByText('Option', { exact: true }).first()` for options
- After selecting, the sheet auto-closes and state feedback updates — wait 1500ms after click
- **OnBlur test**: Use `.focus()` + `keyboard.press('Tab')` to trigger blur without opening the sheet
- `.Invalid("message")` renders as an icon/visual indicator on the button, NOT as visible text

## Dialog and Sheet (Critical)

- Ivy dialogs (`.ToDialog()`) and confirmation dialogs (`.WithConfirm()`) render as `<div role="dialog">`, NOT HTML `<dialog>` elements
- **NEVER** use `page.locator("dialog")` — it won't match. Always use `page.getByRole("dialog", { name: "Dialog Title" })` or `page.locator("[role='dialog']")`
- Ivy sheets (`.ToSheet()`) also render with `role="dialog"` — same locator pattern applies
- Form fields inside dialogs use labels like "Title *" (with asterisk for required) — `getByLabel("Title")` may not match. Use `dialog.getByRole("textbox").nth(N)` to target fields by position
- Edit sheets use "Save" button (not "Submit") for form submission
- Create dialogs reuse the entity action name as the submit button text (e.g., "Create")

### WithConfirm Dialog

- `button.WithConfirm("message", "title")` renders a custom Dialog with "Cancel" (outline) and "Ok" (primary) buttons
- The confirm button text is always **"Ok"** — use `getByRole("button", { name: "Ok" })` to click it
- The dialog title and message are customizable, but button labels are hardcoded in `WithConfirmView`

## Form Patterns (.ToForm / .ToDialog / .ToSheet)

- `.ToForm()` with `[Required]` fields renders labels as "FieldName *" (with asterisk suffix) — `getByText("Code", { exact: true })` won't match "Code *". Use input element locators instead
- `.ToDialog(isOpen, title, submitTitle)` renders a dialog with custom title and submit button text
- `.ToSheet(isOpen, title)` renders a slide-in sheet with Save/Cancel buttons
- `.WithField().Required()` adds `*` suffix to field labels
- `.WithField().Label("X")` on NumberInput does NOT create an HTML `<label for="">` association — `getByLabel("X")` fails. Use `page.locator("input").first()` or index-based locators instead

## NumberInput / NumberRangeInput / MoneyInput

- **NumberInput** (`state.ToNumberInput()`) renders as a regular text `<input>` in the DOM, NOT `<input type="number">`. `page.locator('input[type="number"]')` will find nothing. Use `page.locator('input[value="200"]')` to locate by current value, or find inputs relative to their label text.
- **NumberRangeInput** (`state.ToNumberRangeInput()`) renders as a dual-handle slider with `role="slider"` attributes (2 sliders per range input for lower/upper bounds). Supports currency formatting, percent formatting, prefix/suffix, and keyboard navigation.
- `state.ToMoneyInput().Currency("USD").Precision(0)` renders as a text input with formatted currency (e.g., "$850,000") — values are displayed with `$` prefix and comma separators
- `state.ToNumberInput().Variant(NumberInputVariants.Slider)` renders similarly to `ToSliderInput()` — a Radix slider with `role="slider"`.

## BoolInput (Checkbox / Switch)

- `state.ToBoolInput().Variant(BoolInputVariants.Checkbox)` renders as `<button role="checkbox">` (Radix UI), NOT a native `<input type="checkbox">` — `getByRole("checkbox", { name: /.../ })` and `getByLabel()` do NOT work for locating these; use `page.locator('[role="checkbox"]').nth(N)` by index order instead
- Clicking the checkbox label text via `getByText("Label").click()` does NOT toggle the checkbox — must click the `<button>` element directly
- `state.ToBoolInput().Label("X")` WITHOUT explicit `.Variant()` renders as `role="checkbox"` (NOT `role="switch"`) — clicking the label text does NOT toggle the checkbox

## Slider

- `state.ToSliderInput()` renders as a Radix UI slider with `role="slider"`, NOT a native `<input type="range">` — use `page.getByRole("slider")` and keyboard interaction (ArrowRight/ArrowLeft to increment/decrement by step, Home/End for min/max)

## TextInput / TextareaInput

- `state.ToTextareaInput()` renders as a standard `<textarea>` element, locatable via `page.locator("textarea").first()`; disabled output textarea is the `.nth(1)`
- `state.ToTextInput()` renders as a standard `role="textbox"` element
- `new TextInput(value, onChange).Placeholder("X")` renders with `placeholder="X"` attribute accessible via `getByRole("textbox", { name: "X" })`
- `.Multiline().Rows(6)` on TextInput renders as `<textarea>` element

## DateInput / DateRangeInput

### DateInput (ToDateInput)

- `state.ToDateInput()` renders as a `<button data-slot="calendar">` trigger that opens a Radix Popover with a react-day-picker Calendar — NOT a native `<input type="date">`
- **Calendar navigation**: Month input: `input[placeholder="M"]`, Year input: `input[placeholder="YYYY"]` — fill and press Enter
- **Day selection**: Day buttons inside `div[data-slot="calendar"]` (calendar root, NOT the trigger button). Use `page.locator('div[data-slot="calendar"]').locator("button").filter({ hasText: /^15$/ }).first().click()`
- The popover content renders via Radix Portal at document root
- After selecting a day, the popover auto-closes
- `nullable` dates show a clear (X) button when a value is set

### DateRangeInput (ToDateRangeInput)

- `state.ToDateRangeInput()` renders as a `<button data-slot="calendar">` trigger with a two-month calendar popover and preset shortcuts
- The `data-testid` attribute is placed directly on the trigger button itself, NOT on a wrapper div
- **Date selection in range mode**: After clicking the start date, react-day-picker disables dates before the start. Use sidebar presets for reliable testing
- Supports `Placeholder`, `StartPlaceholder`, `EndPlaceholder`
- Range inputs use tuple state: `UseState<(DateOnly, DateOnly)>()`
- `UseState<(DateOnly, DateOnly)?>()` (nullable tuple) is incompatible — use non-nullable

### Calendar Widget Patterns

- react-day-picker v9 uses flex layout, NOT `<table>` elements. Use `.rdp-weekdays .rdp-weekday` for weekday headers, `.rdp-day button` for day buttons

## CodeInput / CodeBlock

- `new CodeBlock(state.Value, language)` where `state` is populated via `UseEffect` will render with EMPTY `<code>` element on initial page load — the UseEffect hasn't fired yet. Trigger a state change first, then wait 500-1000ms.
- CodeMirror-based code inputs (from `state.ToCodeInput()`) use `.cm-content[contenteditable='true']` as the editable locator
- **IMPORTANT**: Do NOT use `keyboard.type()` or `keyboard.insertText()` for CodeMirror editors — use clipboard paste instead: `page.evaluate(async (t) => { await navigator.clipboard.writeText(t); }, text)` then `page.keyboard.press("Control+V")`. Requires `permissions: ["clipboard-read", "clipboard-write"]`.
- `CodeInput<TString>` requires a type parameter — use `.ToCodeInput()` extension method or `Markdown` widget as alternative
- `.ShowCopyButton()` and code editors add `aria-label="Copy to clipboard"` icon buttons — always use `{ exact: true }` for explicit Copy buttons
- **CodeBlock syntax tokens use inline styles, not CSS classes** — `react-syntax-highlighter` (Prism mode) applies styles via inline `style` attributes. Do NOT use class-based selectors.

## Card Component

- `new Card(content).Title("X")` renders the title as a `<span>`, NOT as a heading element — use `getByText("X", { exact: true })` to locate card titles, not `getByRole("heading")`
- `new Card(content, header: Text.H3("X"))` renders a heading — but `.Title("X")` does not
- Card title text can cause strict mode violations — always use `{ exact: true }`
- `Card.Default(content)` does NOT exist. Use `new Card().Content(widget)` or `new Card().Header("title").Content(widget)` pattern instead.
- **Card `.Title().Content()` renders invisible card body** — workaround: use `Layout.Vertical()` with manual styling instead of Card

## VideoPlayer Widget

- `<track>` elements inside `<video>` are NOT accessible via Playwright locators — use `page.evaluate()` to query `document.querySelectorAll('video')[N].querySelectorAll('track')`
- Non-CORS video sources will fail when subtitles are present — use CORS-compatible sources

## Kanban Widget

- Kanban column headers include card counts, e.g., "Todo (2)" — use regex `/^Todo/` or `/Todo.*\(\d+\)/`
- The `group:` parameter (not `path:`) is used in the `[App]` attribute for nav grouping

## Calendar Widget

- Calendar renders using react-big-calendar with toolbar (Today, <, >, Month/Week/Day/Agenda)
- Events display on correct dates with color support (string colors like "Blue", "Green", "Red", "Purple")
- All-day events span across multiple date cells
- `OnEventClick` handler receives the event ID
- `.ShowToolbar(false)` hides the navigation toolbar

## ECharts (Bar/Line/Area/Pie/Radar Charts)

- Ivy charts use **echarts-for-react** which renders as **SVG** (not canvas) by default — `page.locator('canvas')` will NOT find chart elements
- Locate ECharts instances via: `page.locator('[_echarts_instance_]')`
- The `onChartReady` callback sets `data-chart-rendered="true"` on the container
- For screenshot-based verification, use longer waits (3s+)
- **Screenshot path in test specs**: `projectRoot` resolves to `sample/` dir; screenshots go to `path.resolve(projectRoot, '..', 'screenshots')`
- **Canvas presence ≠ visual content**: Charts can have valid canvas elements but render completely empty
- **ECharts indicator labels are rendered on canvas, NOT in DOM text**: Use React fiber to extract option data
- **Accessing ECharts option data via React fiber**: Walk up from `<canvas>` to find `memoizedProps.option.radar`
- **`__ec_instance__` and `_ec_` are NOT reliable** — use fiber tree instead
- **Radar option.radar can be object or array**: Always normalize: `Array.isArray(data) ? data : [data]`
- **Case sensitivity pattern**: When charts show axes/labels but no data lines, suspect dataKey case mismatch

## Camera/Media Testing

- Chromium's `--use-fake-device-for-media-stream` produces `videoWidth=0` and `videoHeight=0` in headless mode — upload-dependent assertions cannot be verified
- CameraInput widget's state machine (idle -> active -> captured) CAN be tested
- Set Playwright config: `permissions: ['camera']` and appropriate `launchOptions.args`

## Upload/FileUpload Patterns

- `UseUpload(MemoryStreamUploadHandler.Create(photoState), defaultContentType: "image/png")` creates an upload context
- `FileUpload<byte[]>` has: FileName, Length, Content, Status (Pending/Loading/Aborted/Failed/Finished)
- `FileUploadStatus` values: Pending, Aborted, Loading, Failed, Finished (NO `InProgress`)
- `ToFileInput(upload).Placeholder(...)` renders a dashed dropzone with hidden `input[type="file"]`
- `page.locator('input[type="file"]').setInputFiles({name, mimeType, buffer})` works for Playwright testing
- `FileUpload<byte[]>.ToTable()` with `.Remove(e => e.Content)` throws `KeyNotFoundException` — remove the `.Remove()` call

## File Dialog Testing (UseFileDialog/UseSaveDialog/UseFolderDialog)

### Disabling File System Access API for Playwright
```typescript
await page.addInitScript(() => {
  Object.defineProperty(window, 'showOpenFilePicker', { value: undefined, configurable: true, writable: true });
  Object.defineProperty(window, 'showSaveFilePicker', { value: undefined, configurable: true, writable: true });
  Object.defineProperty(window, 'showDirectoryPicker', { value: undefined, configurable: true, writable: true });
  delete (window as any).showOpenFilePicker;
  delete (window as any).showSaveFilePicker;
  delete (window as any).showDirectoryPicker;
});
```

### Folder Dialog with webkitdirectory
Playwright's `fileChooser.setFiles()` for `<input webkitdirectory>` requires passing a **directory path**, not individual file paths.

### Save Dialog Fallback
In headless mode with File System Access API disabled, the save dialog uses `<a download>` fallback. Use `page.waitForEvent('download')` to capture it.

## Markdown Widget

- **OnLinkClick event fires for ALL URL types** when handler is registered
- **Lambda overload ambiguity**: Must explicitly type parameter: `(string url) => { }`
- **Popover links** (`[text](## "content")` syntax): Renders as `<span role="button">` with Radix `<Popover>`
- **normalizeNestedFences preprocessor**: Handles nested code blocks per CommonMark spec
- **Icons.X in inline code**: react-markdown v10 removed `inline` prop — use `!className` check instead
- **Local image support**: `.DangerouslyAllowLocalFiles()` preserves file:// URLs; browser blocking is expected behavior

## FeedbackInput (Star Rating)

- **Star rating SVG locator**: Use `page.locator('button svg')` — page has extra SVGs from Ivy branding
- **AllowHalf creates extra SVG elements**: Don't assert exact `button svg` counts
- **Decimal formatting locale**: Use `CultureInfo.InvariantCulture` for values matched by Playwright
- `.Invalid("message")` renders as an icon, not visible text

## Details Widget

- `.ToDetails()` on anonymous objects converts PascalCase to spaced labels (e.g., `MonthlyNetBurn` → "Monthly Net Burn")
- **`Dictionary<TKey, TValue>.ToDetails()` crashes** with `TargetParameterCountException` — convert to anonymous object first

## Tabs Layout

- **Ivy `Layout.Tabs` renders BOTH tab panels in DOM** — inactive tab content is hidden but still exists. Locators can match hidden elements from inactive panel.
- Tab switching: `getByRole("tab", { name: "TabName" })` works reliably for clicking tab triggers

## Webhooks

- `UseWebhook` generates URLs at `/ivy/webhook/<guid>`, NOT `/api/webhook/<guid>`
- Only accepts GET and POST requests — PUT/DELETE return non-200
- Each `page.goto()` creates new WebSocket session with fresh state and new webhook URL
- State updates via WebSocket need 1500-3000ms wait for UI reflection

## UseArgs / UseDownload

- **`UseArgs<T>` does NOT work with external URL navigation** — reads from WebSocket `appArgs`, not browser URL
- **`IHttpContextAccessor.HttpContext` is null in WebSocket context** — always use `?.`
- `UseDownload(factory, mimeType, fileName)` returns nullable URL — renders as `role="link"` when non-null
