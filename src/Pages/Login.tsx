import { Eye, EyeOff, LockKeyhole, Mail } from 'lucide-react'
import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router'
import { homeForRole } from '../components/layout/RoleRoute'
import { Button } from '../components/ui/Button'
import { ThemeToggle } from '../components/ui/ThemeToggle'
import { useSession } from '../context/SessionContext'

function SupportIllustration() {
  return <aside className="login-visual" aria-label="HelpDesk support illustration">
    <div className="support-badge" aria-label="24/7 support"><strong>24/7</strong><span>Support</span></div>
    <div className="message-bubble message-bubble-left" aria-hidden="true"><span/><span/><span/></div>
    <div className="message-bubble message-bubble-right" aria-hidden="true"><span/><span/></div>
    <svg className="support-illustration" viewBox="0 0 560 500" role="img" aria-labelledby="support-illustration-title">
      <title id="support-illustration-title">Support agent helping from a laptop</title>
      <path className="illustration-leaf" d="M80 374c-38-55-26-111 31-144 11 65 1 113-31 144Z"/>
      <path className="illustration-leaf secondary" d="M458 374c38-55 26-111-31-144-11 65-1 113 31 144Z"/>
      <path className="illustration-stem" d="M82 378c2-60 12-104 29-135M456 378c-2-60-12-104-29-135"/>
      <ellipse className="illustration-shadow" cx="282" cy="427" rx="188" ry="23"/>
      <path className="agent-body" d="M194 401c8-94 42-142 89-142 52 0 86 50 94 142Z"/>
      <path className="agent-shirt" d="M245 276l37 42 39-42 24 125H220Z"/>
      <circle className="agent-face" cx="283" cy="200" r="65"/>
      <path className="agent-hair" d="M219 204c0-56 27-88 70-88 39 0 66 29 66 72-16-7-29-20-39-38-19 27-52 44-97 54Z"/>
      <path className="headset-band" d="M222 210c-2-58 22-92 63-92 42 0 68 36 66 93"/>
      <rect className="headset-ear" x="210" y="202" width="24" height="48" rx="12"/>
      <rect className="headset-ear" x="340" y="202" width="24" height="48" rx="12"/>
      <path className="headset-mic" d="M351 239c-1 27-17 38-40 38"/>
      <circle className="headset-mic-dot" cx="309" cy="277" r="6"/>
      <circle className="agent-eye" cx="262" cy="205" r="4"/>
      <circle className="agent-eye" cx="307" cy="205" r="4"/>
      <path className="agent-smile" d="M267 232c10 10 24 10 34 0"/>
      <path className="agent-arm" d="M222 315c-29 16-43 44-49 83M345 315c30 16 43 44 49 83"/>
      <path className="laptop-screen" d="M183 324h198l-23 101H205Z"/>
      <path className="laptop-base" d="M169 425h226c-8 16-23 23-45 23H214c-22 0-37-7-45-23Z"/>
      <circle className="laptop-mark" cx="282" cy="377" r="16"/>
      <path className="laptop-mark-line" d="M274 377h16M282 369v16"/>
    </svg>
    <div className="visual-caption"><strong>Support when your team needs it.</strong><span>Clear requests. Faster resolutions.</span></div>
  </aside>
}

export default function Login() {
  const [showPassword, setShowPassword] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [remember, setRemember] = useState(false)
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const navigate = useNavigate()
  const { login, demoLogin } = useSession()
  const [demoSubmitting, setDemoSubmitting] = useState('')

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setError('')
    setSubmitting(true)
    try {
      const user = await login(email, password, remember)
      navigate(homeForRole(user.role))
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Unable to sign in.')
    } finally {
      setSubmitting(false)
    }
  }

  return <main className="login-page">
    <div className="login-shell">
      <section className="login-form-side">
        <ThemeToggle className="login-theme-toggle"/>
        <form className="login-card" onSubmit={submit}>
          <header className="login-heading">
            <h1>HelpDesk <span>Lite</span></h1>
            <p>Your issues. Our priority.</p>
            <span className="login-accent" aria-hidden="true"/>
          </header>
          <div className="login-fields">
            {error&&<div className="form-error" role="alert">{error}</div>}
            <label htmlFor="login-email">Email address</label>
            <div className="input-with-icon">
              <Mail size={19} aria-hidden="true"/>
              <input id="login-email" type="email" required value={email} onChange={event=>setEmail(event.target.value)} placeholder="Enter your email" autoComplete="email"/>
            </div>
            <label htmlFor="login-password">Password</label>
            <div className="input-with-icon">
              <LockKeyhole size={19} aria-hidden="true"/>
              <input id="login-password" type={showPassword?'text':'password'} required value={password} onChange={event=>setPassword(event.target.value)} placeholder="Enter your password" autoComplete="current-password"/>
              <button type="button" onClick={()=>setShowPassword(!showPassword)} aria-label={showPassword?'Hide password':'Show password'}>{showPassword?<EyeOff size={19}/>:<Eye size={19}/>}</button>
            </div>
            <div className="login-options"><label className="check" htmlFor="remember-me"><input id="remember-me" type="checkbox" checked={remember} onChange={event=>setRemember(event.target.checked)}/><span>Remember me</span></label><Link to="/forgot-password">Forgot password?</Link></div>
            <Button type="submit" disabled={submitting}>{submitting?'Signing in…':'Sign In'}</Button>
            <p className="auth-link">Don't have an account? <Link to="/register">Create Account</Link></p>
            <section className="quick-demo"><h2>Quick Demo Access</h2><p>Explore privileged HelpDesk Lite roles.</p>{([['SupportAgent','Manage, assign, update, and resolve tickets.'],['Manager','View operational ticket activity.']] as const).map(([role,description])=><button type="button" key={role} disabled={!!demoSubmitting} onClick={async()=>{setError('');setDemoSubmitting(role);try{const user=await demoLogin(role);navigate(homeForRole(user.role))}catch(reason){setError(reason instanceof Error?reason.message:'Unable to start demo.')}finally{setDemoSubmitting('')}}}><span><strong>{role==='SupportAgent'?'Support Agent':role}</strong><small>{description}</small></span><b>{demoSubmitting===role?'Opening…':'Try'}</b></button>)}</section>
          </div>
        </form>
      </section>
      <SupportIllustration/>
    </div>
  </main>
}
