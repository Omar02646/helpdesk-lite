import type { User, UserRole } from '../types/user'
import { api } from './client'
type ApiUser={id:string;name:string;email:string;role:'Employee'|'SupportAgent'|'Manager';initials:string}
const map=(user:ApiUser):User=>({...user,role:(user.role==='SupportAgent'?'Support Agent':user.role) as UserRole})
export const authApi={login:async(email:string,password:string,rememberMe:boolean)=>map(await api<ApiUser>('/api/auth/login',{method:'POST',body:JSON.stringify({email,password,rememberMe})})),me:async()=>map(await api<ApiUser>('/api/auth/me')),logout:()=>api<void>('/api/auth/logout',{method:'POST'})}
