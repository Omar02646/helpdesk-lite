// oxlint-disable react/only-export-components -- route helpers are intentionally colocated
import type { ReactNode } from 'react'
import { Navigate } from 'react-router'
import { useSession } from '../../context/SessionContext'
import type { UserRole } from '../../types/user'
export const homeForRole=(role:UserRole)=>role==='Employee'?'/tickets':role==='Support Agent'?'/support':'/manager'
export function ProtectedRoute({children}:{children:ReactNode}){const{currentUser,loading}=useSession();if(loading)return <div className="route-state">Loading workspace…</div>;return currentUser?children:<Navigate to="/login" replace/>}
export function RoleRoute({allow,children}:{allow:UserRole[];children:ReactNode}){const{currentUser}=useSession();if(!currentUser)return <Navigate to="/login" replace/>;return allow.includes(currentUser.role)?children:<Navigate to={homeForRole(currentUser.role)} replace/>}
export function RoleHome(){const{currentUser,loading}=useSession();if(loading)return <div className="route-state">Loading workspace…</div>;return <Navigate to={currentUser?homeForRole(currentUser.role):'/login'} replace/>}
