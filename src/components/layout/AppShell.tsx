import { useEffect, useRef, useState } from 'react'
import { Outlet } from 'react-router'
import { Header } from './Header'
import { Sidebar } from './Sidebar'

export function AppShell() {
  const [open, setOpen] = useState(false)
  const previousFocus = useRef<HTMLElement | null>(null)
  useEffect(() => {
    const closeOnEscape = (event: KeyboardEvent) => { if (event.key === 'Escape') setOpen(false) }
    document.addEventListener('keydown', closeOnEscape)
    document.body.style.overflow = open ? 'hidden' : ''
    if (open) {
      previousFocus.current = document.activeElement as HTMLElement
      document.querySelector<HTMLButtonElement>('.close-menu')?.focus()
    } else if (previousFocus.current) {
      previousFocus.current.focus()
      previousFocus.current = null
    }
    return () => {
      document.removeEventListener('keydown', closeOnEscape)
      document.body.style.overflow = ''
    }
  }, [open])
  return <div className="app-shell"><Sidebar open={open} onClose={() => setOpen(false)}/><div className="app-column"><Header onMenu={() => setOpen(true)}/><main className="workspace"><Outlet/></main></div></div>
}
