# Review: DateTimeInput Min/Max/Step Constraints

## Visual Verification Checklist

- [ ] **Date input with Min/Max**: Open the DateTimeInput sample app, scroll to "Min/Max/Step Constraints" section. Click the "Min/Max Date" input and verify dates before today are grayed out/disabled and dates beyond +90 days are also disabled.
- [ ] **DateTime with business hours**: Click the "Business Hours" input. Verify the calendar disables dates before today's 9 AM and after today's 5 PM boundary dates.
- [ ] **Time input with Step**: Click the "Time Slots" input. Verify that stepping through times only allows 15-minute increments (:00, :15, :30, :45).
- [ ] **TimeSpan serialization**: Verify that `TimeSpan.FromMinutes(15)` correctly serializes to the frontend and is parsed as 900 seconds for the HTML time input step attribute. Check browser dev tools network tab for the serialized prop value.
- [ ] **Nullable + constraints**: Verify that nullable date inputs with min/max still allow clearing the value.
- [ ] **Month/Year variants**: Test that Month and Year pickers disable options outside min/max range when constraints are applied.
