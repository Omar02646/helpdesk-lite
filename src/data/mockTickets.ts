import type { Ticket } from '../types/ticket'
const history = (owner = 'Ahmed Hassan') => [
  { id: 'h1', title: 'Ticket created', detail: 'Request submitted by Omar Mohamed', createdAt: 'Aug 14, 2026 · 9:10 AM' },
  { id: 'h2', title: `Assigned to ${owner}`, detail: 'Ownership updated by Support Desk', createdAt: 'Aug 14, 2026 · 9:35 AM' },
  { id: 'h3', title: 'Status changed to In Progress', detail: 'Work has started on this request', createdAt: 'Aug 14, 2026 · 10:02 AM' },
]
export const initialTickets: Ticket[] = [
  { id:'1', ticketNumber:'HDL-1001', title:'Laptop will not start', description:'My laptop powers on but the screen stays black. I have tried restarting and connecting an external monitor.', category:'IT Support', status:'In Progress', priority:'High', createdBy:'Omar Mohamed', assignedTo:'Ahmed Hassan', createdAt:'Aug 14, 2026', updatedAt:'Aug 14, 2026', comments:[{id:'c1',author:'Ahmed Hassan',body:'I’m checking the device diagnostics and will share an update shortly.',createdAt:'Aug 14, 2026 · 10:18 AM'}], history:history() },
  { id:'2', ticketNumber:'HDL-1002', title:'VPN access problem', description:'I cannot connect to the company VPN from my laptop while working remotely.', category:'Network', status:'Open', priority:'Medium', createdBy:'Omar Mohamed', assignedTo:null, createdAt:'Aug 14, 2026', updatedAt:'Aug 14, 2026', comments:[], history:[history()[0]] },
  { id:'3', ticketNumber:'HDL-1003', title:'Email not syncing', description:'Outlook has not received new messages since yesterday afternoon.', category:'Email', status:'Resolved', priority:'Low', createdBy:'Omar Mohamed', assignedTo:'Sara Ali', createdAt:'Aug 13, 2026', updatedAt:'Aug 14, 2026', comments:[], history:[...history('Sara Ali'),{id:'h4',title:'Ticket resolved',detail:'Mailbox profile was refreshed',createdAt:'Aug 14, 2026 · 8:30 AM'}] },
  { id:'4', ticketNumber:'HDL-1004', title:'New starter account access', description:'The new finance analyst needs access to the shared drive.', category:'Access & Accounts', status:'Open', priority:'High', createdBy:'Laila Mostafa', assignedTo:null, createdAt:'Aug 12, 2026', updatedAt:'Aug 12, 2026', comments:[], history:[history()[0]] },
  { id:'5', ticketNumber:'HDL-1005', title:'Printer unavailable on floor 3', description:'The shared printer appears offline for the whole team.', category:'IT Support', status:'In Review', priority:'Medium', createdBy:'Karim Samir', assignedTo:'Mona Adel', createdAt:'Aug 11, 2026', updatedAt:'Aug 14, 2026', comments:[], history:history('Mona Adel') },
]
