const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://127.0.0.1:5080'

export type FinanceSummary = {
  revenue: number
  expenses: number
  profit: number
  customerPayments: number
  supplierPayments: number
  cashFlow: number
  receivables: number
  payables: number
}

type AssistantResponse = {
  intent: string
  answer: string
  sources: string[]
  requiresRealtimeData: boolean
}

export async function getFinanceSummary(signal?: AbortSignal): Promise<FinanceSummary> {
  const response = await fetch(`${API_BASE_URL}/api/v1/finance/summary`, { signal })
  if (!response.ok) throw new Error('Không thể tải tổng quan tài chính.')
  return response.json() as Promise<FinanceSummary>
}

export async function askBusinessAssistant(question: string, facts?: Record<string, unknown>): Promise<AssistantResponse> {
  const response = await fetch(`${API_BASE_URL}/api/v1/ai/assistant`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ question, facts }),
  })
  if (!response.ok) throw new Error('Trợ lý AI chưa thể trả lời lúc này.')
  return response.json() as Promise<AssistantResponse>
}

export async function checkServerHealth(signal?: AbortSignal): Promise<boolean> {
  try {
    const response = await fetch(`${API_BASE_URL}/health`, { signal })
    return response.ok
  } catch {
    return false
  }
}
