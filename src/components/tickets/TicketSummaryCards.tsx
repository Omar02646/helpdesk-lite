import { CheckCircle2, CircleDot, Clock3, Tickets } from 'lucide-react'
import type { Ticket } from '../../types/ticket'
import { MetricCard } from '../dashboard/MetricCard'
export function TicketSummaryCards({tickets}:{tickets:Ticket[]}) { return <div className="metrics"><MetricCard label="All Tickets" value={tickets.length} icon={Tickets}/><MetricCard label="Open" value={tickets.filter(t=>t.status==='Open').length} icon={CircleDot} tone="amber"/><MetricCard label="In Progress" value={tickets.filter(t=>t.status==='In Progress').length} icon={Clock3}/><MetricCard label="Resolved" value={tickets.filter(t=>t.status==='Resolved').length} icon={CheckCircle2} tone="green"/></div> }
