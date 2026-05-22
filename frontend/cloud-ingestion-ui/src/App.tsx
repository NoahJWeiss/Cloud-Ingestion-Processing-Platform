import { useState } from 'react'
import { UploadPage } from './pages/UploadPage'
import { FilesPage } from './pages/FilesPage'

export default function App() {
  const [tab, setTab] = useState<'upload' | 'files'>('upload')
  const [refreshKey, setRefreshKey] = useState(0)

  function handleUploaded() {
    setRefreshKey(k => k + 1)
    setTab('files')
  }

  return (
    <div style={styles.root}>
      <header style={styles.header}>
        <h1 style={styles.title}>Cloud Ingestion Platform</h1>
        <nav style={styles.nav}>
          <button
            style={{ ...styles.navBtn, ...(tab === 'upload' ? styles.navBtnActive : {}) }}
            onClick={() => setTab('upload')}
          >
            Upload
          </button>
          <button
            style={{ ...styles.navBtn, ...(tab === 'files' ? styles.navBtnActive : {}) }}
            onClick={() => { setTab('files'); setRefreshKey(k => k + 1) }}
          >
            Files
          </button>
        </nav>
      </header>

      <main style={styles.main}>
        {tab === 'upload' && <UploadPage onUploaded={handleUploaded} />}
        {tab === 'files' && <FilesPage refreshKey={refreshKey} />}
      </main>
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  root: { minHeight: '100vh', display: 'flex', flexDirection: 'column' },
  header: {
    background: '#1e293b', color: '#fff', padding: '0 24px',
    display: 'flex', alignItems: 'center', gap: 32, height: 56,
  },
  title: { fontSize: 16, fontWeight: 700, letterSpacing: '-.01em' },
  nav: { display: 'flex', gap: 4 },
  navBtn: {
    padding: '6px 14px', background: 'transparent', border: 'none',
    color: '#94a3b8', borderRadius: 4, fontWeight: 500, fontSize: 14,
  },
  navBtnActive: { background: '#334155', color: '#fff' },
  main: { flex: 1, padding: 24, maxWidth: 900, width: '100%', margin: '0 auto', display: 'flex', flexDirection: 'column', gap: 20 },
}
