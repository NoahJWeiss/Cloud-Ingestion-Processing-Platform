import type { FileRecord } from '../api'

const colors: Record<FileRecord['status'], string> = {
  Pending:    '#888',
  Processing: '#d97706',
  Completed:  '#16a34a',
  Failed:     '#dc2626',
}

export function StatusBadge({ status }: { status: FileRecord['status'] }) {
  return (
    <span style={{
      display: 'inline-block',
      padding: '2px 8px',
      borderRadius: 4,
      fontSize: 12,
      fontWeight: 600,
      color: '#fff',
      background: colors[status] ?? '#888',
    }}>
      {status}
    </span>
  )
}
