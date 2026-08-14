import type { LucideIcon } from 'lucide-react'
export function MetricCard({label,value,icon:Icon,tone='blue'}:{label:string;value:number|string;icon:LucideIcon;tone?:string}) { return <article className="metric-card"><div className={`metric-icon ${tone}`}><Icon size={20}/></div><div><p>{label}</p><strong>{value}</strong></div></article> }
