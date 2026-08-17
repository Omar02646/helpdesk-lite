import { useMemo, useState, type ReactNode } from 'react'
import { ThemeContext, type Theme } from './theme'

const STORAGE_KEY = 'helpdesk-lite-theme'

function getInitialTheme(): Theme {
  const current = document.documentElement.dataset.theme
  if (current === 'light' || current === 'dark') return current
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export function ThemeProvider({children}:{children:ReactNode}) {
  const [theme, setThemeState] = useState<Theme>(getInitialTheme)
  const value = useMemo(() => ({
    theme,
    setTheme: (nextTheme: Theme) => {
      document.documentElement.dataset.theme = nextTheme
      document.documentElement.style.colorScheme = nextTheme
      localStorage.setItem(STORAGE_KEY, nextTheme)
      setThemeState(nextTheme)
    },
  }), [theme])
  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}
