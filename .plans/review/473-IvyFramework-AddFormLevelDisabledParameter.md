# Review: Add Form-Level Disabled Parameter (#2346)

- [ ] Test `.Disabled()` on a form - all fields and submit button should be disabled
- [ ] Test `.Disabled(false)` - form should be enabled (no fields disabled)
- [ ] Test per-field `.Disabled(true, m => m.Field1)` - only that field should be disabled
- [ ] Test combination: form enabled, single field disabled - only that field disabled
- [ ] Test combination: form disabled overrides per-field state - all fields disabled
- [ ] Verify existing forms without `.Disabled()` still work normally (fields default to enabled)
