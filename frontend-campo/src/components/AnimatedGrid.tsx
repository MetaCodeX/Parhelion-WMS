import { useEffect, useRef } from 'react'

export function AnimatedGrid() {
  const gridRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!gridRef.current) return

    // Generate random direction on mount
    const directions = [
      { x: 150, y: 0 },   // right
      { x: -150, y: 0 },  // left
      { x: 0, y: 150 },   // down
      { x: 0, y: -150 },  // up
      { x: 120, y: 120 },  // diagonal right-down
      { x: -120, y: 120 }, // diagonal left-down
      { x: 120, y: -120 }, // diagonal right-up
      { x: -120, y: -120 } // diagonal left-up
    ]
    
    const randomDir = directions[Math.floor(Math.random() * directions.length)]
    const randomDuration = 20 + Math.random() * 20 // 20-40 seconds
    
    gridRef.current.style.setProperty('--grid-x', `${randomDir.x}px`)
    gridRef.current.style.setProperty('--grid-y', `${randomDir.y}px`)
    gridRef.current.style.setProperty('--grid-duration', `${randomDuration}s`)
  }, [])

  return <div ref={gridRef} className="grid-background"></div>
}
