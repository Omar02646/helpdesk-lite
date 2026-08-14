// oxlint-disable react/only-export-components -- provider and hook form one module
import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { authApi } from '../api/authApi'
import { ApiError } from '../api/client'
import type { User } from '../types/user'
type Session={currentUser:User|null;loading:boolean;login:(email:string,password:string,remember:boolean)=>Promise<User>;logout:()=>Promise<void>}
const Context=createContext<Session|null>(null)
export function SessionProvider({children}:{children:ReactNode}){const[currentUser,setCurrentUser]=useState<User|null>(null);const[loading,setLoading]=useState(true);useEffect(()=>{authApi.me().then(setCurrentUser).catch(error=>{if(!(error instanceof ApiError&&error.status===401))console.error(error)}).finally(()=>setLoading(false))},[]);const login=async(email:string,password:string,remember:boolean)=>{const user=await authApi.login(email,password,remember);setCurrentUser(user);return user};const logout=async()=>{await authApi.logout();setCurrentUser(null)};return <Context.Provider value={{currentUser,loading,login,logout}}>{children}</Context.Provider>}
export function useSession(){const value=useContext(Context);if(!value)throw new Error('useSession must be used inside SessionProvider');return value}
