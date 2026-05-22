import { useState, useRef } from 'react'
import { uploadFile, type UploadResult } from '../api'

const ACCEPTED = '.png,.jpg,.jpeg,.pdf,.mp4,.stl'

export function UploadPage({ onUploaded }: { onUploaded: () => void }) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [uploading, setUploading] = useState(false)
  const [result, setResult] = useState<UploadResult | null>(null)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    const file = inputRef.current?.files?.[0]
    if (!file) return

    setUploading(true)
    setError(null)
    setResult(null)

    try {
      const res = await uploadFile(file)
      setResult(res)
      onUploaded()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Upload failed')
    } finally {
      setUploading(false)
    }
  }

  return (
    <div style={styles.card}>
      <h2 style={styles.heading}>Upload File</h2>
      <p style={styles.hint}>Supported: PNG, JPG, PDF, MP4, STL (max 512 MB)</p>

      <form onSubmit={handleSubmit} style={styles.form}>
        <input
          ref={inputRef}
          type="file"
          accept={ACCEPTED}
          required
          style={styles.fileInput}
        />
        <button type="submit" disabled={uploading} style={styles.button}>
          {uploading ? 'Uploading…' : 'Upload'}
        </button>
      </form>

      {error && <p style={styles.error}>{error}</p>}

      {result && (
        <div style={styles.success}>
          <strong>Uploaded</strong>
          <p>{result.originalFileName} — {(result.sizeBytes / 1024).toFixed(1)} KB</p>
          <p>Status: {result.status}</p>
          <p style={{ fontSize: 12, color: '#888' }}>ID: {result.fileId}</p>
        </div>
      )}
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  card: { background: '#fff', borderRadius: 8, padding: 24, boxShadow: '0 1px 4px rgba(0,0,0,.1)' },
  heading: { marginBottom: 4, fontSize: 18 },
  hint: { color: '#888', fontSize: 13, marginBottom: 16 },
  form: { display: 'flex', gap: 12, alignItems: 'center', flexWrap: 'wrap' },
  fileInput: { flex: 1 },
  button: { padding: '8px 20px', background: '#2563eb', color: '#fff', border: 'none', borderRadius: 6, fontWeight: 600 },
  error: { marginTop: 12, color: '#dc2626' },
  success: { marginTop: 16, padding: 12, background: '#f0fdf4', borderRadius: 6, borderLeft: '4px solid #16a34a' },
}
