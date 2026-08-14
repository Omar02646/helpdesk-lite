import type { Priority, TicketStatus } from '../../types/ticket'
const statusClass: Record<TicketStatus,string> = { Open:'badge-amber','In Progress':'badge-blue','In Review':'badge-purple',Resolved:'badge-green' }
const priorityClass: Record<Priority,string> = { High:'priority-high',Medium:'priority-medium',Low:'priority-low' }
export function TicketStatusBadge({status}:{status:TicketStatus}) { return <span className={`badge ${statusClass[status]}`}><span className="status-dot" />{status}</span> }
export function PriorityBadge({priority}:{priority:Priority}) { return <span className={`priority ${priorityClass[priority]}`}>{priority}</span> }
