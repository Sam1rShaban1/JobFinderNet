import { ClerkProvider } from '@clerk/react'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import './index.css'
import { dark } from '@clerk/themes'
import { ThemeProvider, useTheme } from './context/ThemeContext'

// Clerk renders its own UI (UserButton popover, account modal, sign-in/up) in
// its own DOM, so it can't read our CSS variables. We hand it the dark base
// theme PLUS explicit color variables. Recent Clerk renamed several variables
// (colorText -> colorForeground, colorInputBackground -> colorInput, etc.), so
// we provide both the new and legacy names — Clerk ignores any it doesn't know.
const clerkDarkAppearance = {
  baseTheme: dark,
  variables: {
    colorBackground: '#1a1a1f',
    colorPrimary: '#6b9bff',
    colorNeutral: '#ffffff',
    // new variable names
    colorForeground: '#f4f4f5',
    colorMutedForeground: '#b8b8c2',
    colorMuted: '#26262e',
    colorInput: '#26262e',
    colorInputForeground: '#f4f4f5',
    colorBorder: '#33333d',
    // legacy variable names (older Clerk)
    colorText: '#f4f4f5',
    colorTextSecondary: '#b8b8c2',
    colorInputBackground: '#26262e',
    colorInputText: '#f4f4f5',
  },
}

function ClerkWithTheme() {
  const { theme } = useTheme()
  return (
    // key={theme} forces Clerk to fully re-initialize when the theme flips, so
    // its popover/modal reliably adopt the new appearance (it otherwise caches
    // the appearance from its first mount and ignores later changes).
    <ClerkProvider
      key={theme}
      afterSignOutUrl="/"
      afterSignInUrl="/jobs"
      appearance={theme === 'dark' ? clerkDarkAppearance : undefined}
    >
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </ClerkProvider>
  )
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ThemeProvider>
      <ClerkWithTheme />
    </ThemeProvider>
  </StrictMode>,
)
