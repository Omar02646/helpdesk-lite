import type { User } from '../types/user'
export const users: User[] = [
  { id: 'u1', name: 'Omar Mohamed', role: 'Employee', initials: 'OM' },
  { id: 'u2', name: 'Ahmed Hassan', role: 'Support Agent', initials: 'AH' },
  { id: 'u3', name: 'Sara Ali', role: 'Support Agent', initials: 'SA' },
  { id: 'u4', name: 'Mona Adel', role: 'Support Agent', initials: 'MA' },
  { id: 'u5', name: 'Manager User', role: 'Manager', initials: 'MU' },
]
export const agents = users.filter((user) => user.role === 'Support Agent')
