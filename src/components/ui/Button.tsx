import type { ButtonHTMLAttributes } from 'react'
export function Button({ className='', children, ...props }: ButtonHTMLAttributes<HTMLButtonElement>) { return <button className={`button ${className}`} {...props}>{children}</button> }
