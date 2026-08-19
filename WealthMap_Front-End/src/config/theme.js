/**
 * Four states, not two.
 *
 * "System" is a real choice and the default one: someone whose laptop switches
 * to dark at sunset expects the app to follow. A two-state toggle cannot express
 * that — it would freeze the app in whichever mode the system happened to be in
 * the first time it loaded.
 */
export const THEMES = [
  { value: 'light', labelKey: 'theme.light', icon: 'sun' },
  { value: 'dark', labelKey: 'theme.dark', icon: 'moon' },
  /* Two dark themes, deliberately. Dark is the neutral one and navy is the
     brand at low light; neither is the better answer, they suit different
     rooms. */
  { value: 'navy', labelKey: 'theme.navy', icon: 'boat' },
  { value: 'system', labelKey: 'theme.system', icon: 'monitor' }
]

export const DEFAULT_THEME = 'system'

export const THEME_STORAGE_KEY = 'wm_theme'

export const isSupportedTheme = (value) => THEMES.some((theme) => theme.value === value)

/**
 * The stored choice, or the default.
 *
 * Read before Vue mounts so the first paint is already the right colour — see
 * applyTheme in main.js. A theme that arrives a frame late is a white flash on
 * every load, which is the single most noticeable way to get dark mode wrong.
 */
export function readStoredTheme() {
  try {
    const stored = localStorage.getItem(THEME_STORAGE_KEY)
    return isSupportedTheme(stored) ? stored : DEFAULT_THEME
  } catch {
    // Private browsing, or storage disabled. The default still works.
    return DEFAULT_THEME
  }
}

/**
 * Writes the choice onto the root element, where the CSS reads it.
 *
 * "System" deliberately removes the attribute rather than setting a value: the
 * stylesheet treats "no attribute" as "follow prefers-color-scheme", so the OS
 * stays in charge and changes to it apply without the app doing anything.
 */
export function applyTheme(theme) {
  const root = document.documentElement

  if (theme === 'system') root.removeAttribute('data-theme')
  else root.setAttribute('data-theme', theme)
}

/** What the browser is actually showing, once "system" has been resolved. */
export function resolvedTheme(theme) {
  if (theme !== 'system') return theme

  return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

/**
 * Whether the resolved theme is a dark one.
 *
 * Asked as "not light" rather than "is dark", because there are two dark themes
 * and a third would break any list of them. This decides which artwork the icon
 * and wordmark show, so getting it wrong puts navy lettering on a navy page.
 */
export const isDarkTheme = (theme) => resolvedTheme(theme) !== 'light'

/** The page colour a theme paints, for the browser's own chrome. */
export const themeColour = (theme) => {
  switch (resolvedTheme(theme)) {
    case 'navy': return '#0E141D'
    case 'dark': return '#171614'
    default: return '#F3F2EE'
  }
}
