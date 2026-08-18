# Day 4 — Collaboration Workflow Assignment

**Project:**  
HelpDesk Lite — Internal Support Ticketing Workspace

**Selected Change:**  
Add Light/Dark Theme Support

## 1. Repo-Ready Change Brief

### Work Item

HDL-THEME-01 — Add Light/Dark Theme Support

### Purpose

HelpDesk Lite should support both Light and Dark themes while preserving the application's existing design, workflows, and functionality. The change should provide a consistent theme experience across authentication and authenticated areas without expanding the scope beyond frontend presentation and preference handling.

### Expected Behavior

- Only Light and Dark are visible theme choices.
- On the first visit, when no saved preference exists, the app uses the browser or operating system preferred color scheme.
- Once the user selects Light or Dark, the preference is stored in `localStorage`.
- The saved choice is restored on future visits.
- The selected theme applies globally.
- The theme toggle is available in the authenticated header and on authentication pages.
- Existing business logic, authentication, routing, roles, and API behavior remain unchanged.

### Likely Affected Files / Modules

- `index.html`
- `src/main.tsx`
- `src/index.css`
- `src/context/theme.ts`
- `src/context/ThemeContext.tsx`
- `src/components/ui/ThemeToggle.tsx`
- `src/components/layout/Header.tsx`
- `src/Pages/Login.tsx`
- `src/Pages/AuthPages.tsx`

### Evidence / Acceptance Evidence

The implementation has been validated with the following evidence:

- `npm run lint` passes.
- `npm run build` passes.
- Light and Dark themes were manually verified visually.
- Theme persistence after refresh was verified.
- First-visit fallback to the system theme was verified.
- Login, Register, and other authentication pages were verified.
- Authenticated application pages were verified.
- Responsive and mobile views were verified.

## 2. Branch Plan

**Planned branch:** `feature/hdl-theme-mode`

This name is specific because it identifies theme-mode work rather than a broad UI change. It is bounded to frontend theme support, traceable to work item HDL-THEME-01 through the `hdl-theme` wording, and avoids implying changes to authentication, ticket workflows, or backend services.

### Out of Scope

The branch must not include:

- Backend changes
- Database migrations
- Authentication logic changes
- Role or permission changes
- Ticket workflow changes
- Unrelated UI redesigns
- Production deployment configuration

## 3. Commit Sequence

The following is the planned collaboration sequence for this assignment, even though the current project implementation may already exist:

1. **`feat(theme): add theme context and preference persistence`**  
   Add the centralized theme types, provider, initial system-preference resolution, and `localStorage` read/write behavior. Wire the provider into the application entry point.

2. **`feat(theme): add reusable light-dark toggle`**  
   Add the shared Light/Dark toggle component and integrate it into the authenticated header. Keep its behavior presentation-focused and reusable by authentication pages.

3. **`style(theme): add global dark theme tokens and component styles`**  
   Define centralized theme variables and dark-mode overrides in the global stylesheet. Update shared component styling so the theme is applied consistently without changing application behavior.

4. **`style(auth): support theme mode across authentication pages`**  
   Add the reusable toggle and theme-aware styling to Login, Register, and related authentication views. Preserve the existing forms, validation, navigation, and authentication logic.

5. **`test(theme): verify theme behavior and responsive states`**  
   Record or add the relevant lint, build, persistence, first-visit fallback, accessibility, authenticated-page, authentication-page, and responsive verification evidence.

## 4. Pull Request Draft

**PR Title:** `feat: add light and dark theme support`

### Purpose

Add global Light and Dark theme support to HelpDesk Lite under HDL-THEME-01 while preserving the current visual identity, business workflows, and application behavior.

### Summary of Changes

- Added centralized theme state and preference persistence.
- Used the browser or operating system color-scheme preference when no saved choice exists.
- Added a reusable toggle exposing only Light and Dark choices.
- Made the toggle available in the authenticated header and authentication pages.
- Added global theme tokens and theme-aware component styles.
- Kept routing, authentication, authorization, ticket workflows, and API behavior unchanged.

### Evidence / Checks

- [x] `npm run lint`
- [x] `npm run build`
- [x] Manual Light/Dark testing
- [x] Theme persistence test after refresh
- [x] First-visit system preference test
- [x] Responsive, authenticated-page, and authentication-page checks

### Reviewer Focus

Please focus review on:

- Whether the theme architecture is centralized appropriately
- Maintainability of CSS variables and design tokens
- Accessibility and contrast in both modes
- Whether any unintended business logic changes were introduced
- Theme persistence and initial preference behavior
- Duplicate styling or unnecessary hard-coded colors

### Known Risks

- A component could still contain a hard-coded light-only color.
- Contrast may differ across browsers, displays, or devices.
- `localStorage` preference behavior must remain backward compatible with previously saved values.

### Intentionally Left Out

- No backend changes
- No database changes
- No API changes
- No user-selectable "System" theme option
- No full visual redesign

## 5. Code Review Response Plan

### Scenario A — Question

**Reviewer:** Why are we storing the theme in `localStorage` instead of the backend?

**Response:** The preference is UI-only and scoped to the current browser or device, so it does not require server persistence for this bounded change. Using `localStorage` also avoids introducing API, database, and account-preference changes outside HDL-THEME-01. We can consider cross-device preference synchronization as a separate work item if it becomes a product requirement.

### Scenario B — Suggestion

**Reviewer:** Could we move repeated dark colors into centralized CSS variables?

**Response:** Agreed. I will replace the repeated values with semantic theme variables in `src/index.css` and update the affected selectors to consume those tokens. This will reduce duplication and make future contrast or palette changes easier to review and maintain.

### Scenario C — Required Change

**Reviewer:** The theme toggle does not have an accessible label.

**Response:** Agreed; this is required. I will add an explicit accessible name that communicates the control's purpose and current or resulting mode, then rerun keyboard and screen-reader-oriented accessibility verification in both themes.

### Scenario D — Hard-Coded Color

**Reviewer:** This component still has a hard-coded light background in Dark mode.

**Response:** Thank you for catching that. I will replace the hard-coded background with the appropriate semantic theme token, check its foreground contrast, and inspect the surrounding component states in both Light and Dark modes for similar omissions.

## 6. Merge and Release Checklist

### Before Merge

- [ ] Work item scope matches HDL-THEME-01
- [ ] PR contains only intended theme-related changes
- [ ] No backend or database changes
- [ ] `npm run lint` passes
- [ ] `npm run build` passes
- [ ] Light mode manually verified
- [ ] Dark mode manually verified
- [ ] Theme persistence verified
- [ ] First-visit system preference verified
- [ ] Auth pages checked
- [ ] Authenticated application pages checked
- [ ] Desktop and mobile views checked
- [ ] Keyboard accessibility checked
- [ ] Reviewer feedback resolved
- [ ] No unresolved required-change comments
- [ ] Branch is up to date with target branch

### Before Release / Production

- [ ] Confirm merged commit is the expected version
- [ ] Create production frontend/backend publish output using the normal project process
- [ ] Preserve `App_Data` during WebDeploy
- [ ] Verify the production health endpoint
- [ ] Smoke-test the Light/Dark toggle
- [ ] Verify the saved theme after refresh
- [ ] Verify Register page placeholders and authentication pages
- [ ] Verify login still works
- [ ] Verify demo Support Agent and Manager flows still work
- [ ] Check the browser console for unexpected frontend errors
- [ ] Confirm a rollback path or previous known-good deployment is available

This frontend-only change does not require a database migration.
