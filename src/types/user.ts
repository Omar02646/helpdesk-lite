export type UserRole = 'Employee' | 'Support Agent' | 'Manager'
export type User = { id: string; name: string; role: UserRole; initials: string; email?: string; firstName?: string; lastName?: string; isDemo: boolean }
