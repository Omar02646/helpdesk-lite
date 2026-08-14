import type { Ticket } from '../types/ticket'
import { api } from './client'
import { mapTicket } from './ticketsApi'
type RawSummary={open:number;unassigned:number;inProgress:number;resolved:number;workload:{userId:string;name:string;activeTickets:number}[];recentTickets:Parameters<typeof mapTicket>[0][];needsAttention:Parameters<typeof mapTicket>[0][]}
export type ManagerSummary={open:number;unassigned:number;inProgress:number;resolved:number;workload:{userId:string;name:string;activeTickets:number}[];recentTickets:Ticket[];needsAttention:Ticket[]}
export async function getManagerSummary():Promise<ManagerSummary>{const data=await api<RawSummary>('/api/manager/summary');return{...data,recentTickets:data.recentTickets.map(mapTicket),needsAttention:data.needsAttention.map(mapTicket)}}
