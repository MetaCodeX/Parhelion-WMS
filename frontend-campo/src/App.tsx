import { AnimatedGrid } from './components/AnimatedGrid'

function App() {
  return (
    <div className="min-h-screen bg-sand flex flex-col items-center justify-center p-6 relative overflow-hidden">
      {/* Animated Grid Background */}
      <AnimatedGrid />

      {/* Main Content */}
      <main className="text-center max-w-md content-layer">
        <h1 className="font-logo text-4xl md:text-5xl text-black leading-tight">
          Parhelion
        </h1>
        <p className="font-logo text-2xl md:text-3xl text-oxide mb-4">
          Logistics
        </p>
        <p className="font-heading text-base md:text-lg text-gray-600 mb-6">
          App de Campo
        </p>
        
        <p className="text-gray-500 text-sm mb-6">En desarrollo</p>

        {/* GitHub Button */}
        <a 
          href="https://github.com/MetaCodeX/Parhelion-Logistics" 
          target="_blank" 
          rel="noopener noreferrer"
          className="btn btn-primary inline-flex items-center gap-2 w-full justify-center"
        >
          <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
            <path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z"/>
          </svg>
          Ver en GitHub
        </a>

        {/* Interactive Components Demo */}
        <div className="mt-8 flex flex-col gap-3">
          <button className="btn btn-oxide w-full">Acción</button>
          <button className="btn btn-black w-full">Confirmar</button>
        </div>
      </main>

      {/* Footer */}
      <footer className="absolute bottom-0 left-0 right-0 p-4 text-center content-layer">
        <p className="text-gray-500 text-xs">
          Parhelion Logistics - MetaCodeX Portfolio
        </p>
        <p className="text-gray-400 text-[10px] mt-1">
          UI: <a href="https://github.com/ekmas/neobrutalism-components" target="_blank" rel="noopener noreferrer" className="text-oxide hover:underline">neobrutalism-components</a>
        </p>
      </footer>
    </div>
  )
}

export default App
