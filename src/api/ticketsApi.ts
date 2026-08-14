import type { Ticket, TicketStatus } from '../types/ticket'
import type { User } from '../types/user'
import { api } from './client'
type ApiTicket={id:number;ticketNumber:string;title:string;description:string;category:string;status:'Open'|'InProgress'|'InReview'|'Resolved';priority:'High'|'Medium'|'Low';createdBy:string;assignedTo:string|null;assignedToUserId:string|null;createdAt:string;updatedAt:string;resolvedAt:string|null;comments:{id:number;author:string;body:string;createdAt:string}[];history:{id:number;title:string;detail:string;createdAt:string}[]}
const statusMap:Record<ApiTicket['status'],TicketStatus>={Open:'Open',InProgress:'In Progress',InReview:'In Review',Resolved:'Resolved'}
const apiStatus:Record<TicketStatus,ApiTicket['status']>={Open:'Open','In Progress':'InProgress','In Review':'InReview',Resolved:'Resolved'}
const date=(value:string)=>new Intl.DateTimeFormat('en-US',{month:'short',day:'numeric',year:'numeric'}).format(new Date(value))
const dateTime=(value:string)=>new Intl.DateTimeFormat('en-US',{month:'short',day:'numeric',year:'numeric',hour:'numeric',minute:'2-digit'}).format(new Date(value))
export const mapTicket=(ticket:ApiTicket):Ticket=>({id:String(ticket.id),ticketNumber:ticket.ticketNumber,title:ticket.title,description:ticket.description,category:ticket.category,status:statusMap[ticket.status],priority:ticket.priority,createdBy:ticket.createdBy,assignedTo:ticket.assignedTo,assignedToUserId:ticket.assignedToUserId,createdAt:date(ticket.createdAt),updatedAt:date(ticket.updatedAt),resolvedAt:ticket.resolvedAt?date(ticket.resolvedAt):null,comments:ticket.comments.map(item=>({...item,id:String(item.id),createdAt:dateTime(item.createdAt)})),history:ticket.history.map(item=>({...item,id:String(item.id),createdAt:dateTime(item.createdAt)}))})
const query=(values:Record<string,string|undefined>)=>{const params=new URLSearchParams();Object.entries(values).forEach(([key,value])=>{if(value)params.set(key,value)});const text=params.toString();return text?`?${text}`:''}
export const ticketsApi={
  my:async(filters:Record<string,string|undefined>={})=>(await api<ApiTicket[]>(`/api/tickets/my${query(filters)}`)).map(mapTicket),
  all:async(filters:Record<string,string|undefined>={})=>(await api<ApiTicket[]>(`/api/tickets${query(filters)}`)).map(mapTicket),
  queue:async(filters:Record<string,string|undefined>={})=>(await api<ApiTicket[]>(`/api/support/queue${query(filters)}`)).map(mapTicket),
  get:async(id:string)=>mapTicket(await api<ApiTicket>(`/api/tickets/${id}`)),
  create:async(data:{title:string;category:string;description:string})=>mapTicket(await api<ApiTicket>('/api/tickets',{method:'POST',body:JSON.stringify(data)})),
  assign:(id:string,userId:string|null)=>api<void>(`/api/tickets/${id}/assignee`,{method:'PATCH',body:JSON.stringify({userId})}),
  status:(id:string,status:TicketStatus)=>api<void>(`/api/tickets/${id}/status`,{method:'PATCH',body:JSON.stringify({status:apiStatus[status]})}),
  comment:(id:string,body:string)=>api<void>(`/api/tickets/${id}/comments`,{method:'POST',body:JSON.stringify({body})}),
  agents:async()=>(await api<(Omit<User,'role'>&{role:'SupportAgent'})[]>('/api/users/support-agents')).map(user=>({...user,role:'Support Agent' as const})),
}
