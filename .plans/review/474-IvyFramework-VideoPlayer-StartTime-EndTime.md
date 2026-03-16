## VideoPlayer StartTime/EndTime - Review Checklist

- [ ] Play a video with `.StartTime(5)` — verify it begins at 5 seconds, not at 0
- [ ] Play a video with `.StartTime(2).EndTime(6)` — verify it plays only the 2s–6s segment and pauses at 6s
- [ ] Play a YouTube embed with `.StartTime(30).EndTime(60)` — verify YouTube player starts at 30s and stops at 60s
- [ ] Verify that videos without StartTime/EndTime are unaffected (no regressions)
- [ ] Check that seeking past EndTime still triggers the pause behavior
