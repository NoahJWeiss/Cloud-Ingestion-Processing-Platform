export interface FileRecord {
  id: string
  originalFileName: string
  contentType: string
  sizeBytes: number
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed'
  uploadedAt: string
  processedAt: string | null
  metadataJson: string | null
  failureReason: string | null
}

export interface UploadResult {
  fileId: string
  status: string
  originalFileName: string
  sizeBytes: number
}

const BASE = '/api'

export async function uploadFile(file: File): Promise<UploadResult> {
  const form = new FormData()
  form.append('file', file)
  const res = await fetch(`${BASE}/files/upload`, { method: 'POST', body: form })
  if (!res.ok) {
    const body = await res.json().catch(() => ({}))
    throw new Error(body.error ?? `Upload failed (${res.status})`)
  }
  return res.json()
}

export async function listFiles(): Promise<FileRecord[]> {
  const res = await fetch(`${BASE}/files`)
  if (!res.ok) throw new Error(`Failed to fetch files (${res.status})`)
  return res.json()
}

export async function getFile(id: string): Promise<FileRecord> {
  const res = await fetch(`${BASE}/files/${id}`)
  if (!res.ok) throw new Error(`File not found (${res.status})`)
  return res.json()
}
