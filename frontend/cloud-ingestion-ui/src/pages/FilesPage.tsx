import { useEffect, useState } from 'react'
import { listFiles, getFile, type FileRecord } from '../api'
import { StatusBadge } from '../components/StatusBadge'

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString()
}

function MetadataPanel({ record }: { record: FileRecord }) {
  if (!record.metadataJson) return null
  let parsed: Record<string, unknown> = {}
  try { parsed = JSON.parse(record.metadataJson) } catch { return null }
  return (
    <div style={styles.metaPanel}>
      <strong>Extracted Metadata</strong>
      <pre style={styles.pre}>{JSON.stringify(parsed, null, 2)}</pre>
    </div>
  )
}

export function FilesPage({ refreshKey }: { refreshKey: number }) {
  const [files, setFiles] = useState<FileRecord[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selected, setSelected] = useState<FileRecord | null>(null)

  useEffect(() => {
    setLoading(true)
    listFiles()
      .then(setFiles)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false))
  }, [refreshKey])

  async function handleSelect(id: string) {
    if (selected?.id === id) { setSelected(null); return }
    try {
      const record = await getFile(id)
      setSelected(record)
    } catch {
      // ignore
    }
  }

  if (loading) return <p style={{ color: '#888' }}>Loading…</p>
  if (error) return <p style={{ color: '#dc2626' }}>{error}</p>
  if (files.length === 0) return <p style={{ color: '#888' }}>No files uploaded yet.</p>

  return (
    <div style={styles.card}>
      <h2 style={styles.heading}>Uploaded Files</h2>
      <table style={styles.table}>
        <thead>
          <tr>
            <th style={styles.th}>File</th>
            <th style={styles.th}>Size</th>
            <th style={styles.th}>Status</th>
            <th style={styles.th}>Uploaded</th>
          </tr>
        </thead>
        <tbody>
          {files.map(f => (
            <>
              <tr
                key={f.id}
                style={{ ...styles.row, background: selected?.id === f.id ? '#eff6ff' : undefined }}
                onClick={() => handleSelect(f.id)}
              >
                <td style={styles.td}>{f.originalFileName}</td>
                <td style={styles.td}>{formatBytes(f.sizeBytes)}</td>
                <td style={styles.td}><StatusBadge status={f.status} /></td>
                <td style={styles.td}>{formatDate(f.uploadedAt)}</td>
              </tr>
              {selected?.id === f.id && (
                <tr key={`${f.id}-detail`}>
                  <td colSpan={4} style={styles.detailCell}>
                    <div style={styles.detail}>
                      <p><strong>ID:</strong> <code>{selected.id}</code></p>
                      <p><strong>Content type:</strong> {selected.contentType}</p>
                      {selected.processedAt && <p><strong>Processed:</strong> {formatDate(selected.processedAt)}</p>}
                      {selected.failureReason && <p style={{ color: '#dc2626' }}><strong>Failure:</strong> {selected.failureReason}</p>}
                      <MetadataPanel record={selected} />
                    </div>
                  </td>
                </tr>
              )}
            </>
          ))}
        </tbody>
      </table>
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  card: { background: '#fff', borderRadius: 8, padding: 24, boxShadow: '0 1px 4px rgba(0,0,0,.1)' },
  heading: { marginBottom: 16, fontSize: 18 },
  table: { width: '100%', borderCollapse: 'collapse' },
  th: { textAlign: 'left', padding: '8px 12px', borderBottom: '2px solid #e5e7eb', fontSize: 13, color: '#555', fontWeight: 600 },
  td: { padding: '10px 12px', borderBottom: '1px solid #f3f4f6', fontSize: 14 },
  row: { cursor: 'pointer', transition: 'background .1s' },
  detailCell: { padding: 0 },
  detail: { padding: '12px 16px', background: '#f8fafc', borderBottom: '1px solid #e5e7eb', fontSize: 13, display: 'flex', flexDirection: 'column', gap: 4 },
  metaPanel: { marginTop: 8 },
  pre: { marginTop: 4, padding: 8, background: '#1e293b', color: '#e2e8f0', borderRadius: 4, fontSize: 12, overflowX: 'auto' },
}
