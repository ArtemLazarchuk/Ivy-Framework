# Review: FileInput Clipboard Paste Support

- [ ] Take a screenshot (Win+Shift+S) and paste into a FileInput with Ctrl+V — verify it uploads
- [ ] Test paste with `Accept` filter set (e.g., `image/*`) — verify non-matching pastes are rejected with toast
- [ ] Test paste when `MaxFiles` limit is already reached — verify toast error
- [ ] Test paste on a disabled FileInput — verify nothing happens
- [ ] Test paste with `Multiple=false` — verify only the first file is uploaded
- [ ] Verify drag-and-drop still works correctly alongside paste
- [ ] Test in Firefox (clipboard API behavior can differ)
