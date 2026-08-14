export type TicketStatus = 'Open' | 'In Progress' | 'In Review' | 'Resolved'
export type Priority = 'High' | 'Medium' | 'Low'
export type Comment = { id: string; author: string; body: string; createdAt: string }
export type HistoryItem = { id: string; title: string; detail: string; createdAt: string }
export type Ticket = {
  id: string; ticketNumber: string; title: string; description: string; category: string
  status: TicketStatus; priority: Priority; createdBy: string; assignedTo: string | null
  assignedToUserId?: string | null; createdAt: string; updatedAt: string; resolvedAt?: string | null; comments: Comment[]; history: HistoryItem[]
}
