import { useNavigate } from 'react-router'
import { Button } from '../components/ui/Button'
import { homeForRole } from '../components/layout/RoleRoute'
import { useSession } from '../context/SessionContext'
export default function NotFound(){const navigate=useNavigate();const{currentUser}=useSession();return <main className="route-state"><div className="empty-state"><span className="eyebrow">404</span><h1>Page not found</h1><p>The page you requested does not exist.</p><Button onClick={()=>navigate(currentUser?homeForRole(currentUser.role):'/login')}>Return to HelpDesk Lite</Button></div></main>}
