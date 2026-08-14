// oxlint-disable react/only-export-components -- provider and hook form one module
import { createContext, useContext, type ReactNode } from 'react'
import { ticketsApi } from '../api/ticketsApi'
import type { Ticket, TicketStatus } from '../types/ticket'
type Value={createTicket:(data:Pick<Ticket,'title'|'category'|'description'>)=>Promise<Ticket>}
const Context=createContext<Value|null>(null)
export function TicketProvider({children}:{children:ReactNode}){const createTicket=(data:Pick<Ticket,'title'|'category'|'description'>)=>ticketsApi.create(data);return <Context.Provider value={{createTicket}}>{children}</Context.Provider>}
export function useTickets(){const value=useContext(Context);if(!value)throw new Error('useTickets must be inside TicketProvider');return value}
export const ticketStatuses:TicketStatus[]=['Open','In Progress','In Review','Resolved']
