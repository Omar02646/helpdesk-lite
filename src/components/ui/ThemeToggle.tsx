import { Moon, Sun } from 'lucide-react'
import { useTheme } from '../../context/theme'

export function ThemeToggle({className = ''}:{className?:string}) {
  const { theme, setTheme } = useTheme()
  const nextTheme = theme === 'light' ? 'dark' : 'light'
  const label = `Switch to ${nextTheme} theme`
  return <button className={`theme-toggle ${className}`.trim()} type="button" onClick={() => setTheme(nextTheme)} aria-label={label} title={label}>
    <Sun size={17} aria-hidden="true"/>
    <span aria-hidden="true"/>
    <Moon size={17} aria-hidden="true"/>
  </button>
}
