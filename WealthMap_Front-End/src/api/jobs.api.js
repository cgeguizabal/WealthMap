import client from './client'

/**
 * One job per user. Every deduction endpoint returns the *whole* job, so the
 * recomputed net figures arrive with the mutation — no follow-up fetch.
 */
export const jobsApi = {
  list: () => client.get('/jobs'),
  get: (id) => client.get(`/jobs/${id}`),
  create: (payload) => client.post('/jobs', payload),
  update: (id, payload) => client.put(`/jobs/${id}`, payload),
  remove: (id) => client.delete(`/jobs/${id}`),

  addDeduction: (jobId, payload) => client.post(`/jobs/${jobId}/deductions`, payload),
  updateDeduction: (jobId, deductionId, payload) =>
    client.put(`/jobs/${jobId}/deductions/${deductionId}`, payload),
  removeDeduction: (jobId, deductionId) =>
    client.delete(`/jobs/${jobId}/deductions/${deductionId}`)
}

export const DEDUCTION_TYPE = { FIXED: 1, PERCENTAGE: 2 }

export const DEDUCTION_TYPE_OPTIONS = [
  { value: DEDUCTION_TYPE.FIXED, label: 'Fixed amount' },
  { value: DEDUCTION_TYPE.PERCENTAGE, label: 'Percentage of gross' }
]

/**
 * Mirrors the domain rule so the effect of an edit can be shown before saving:
 * net = gross − Σfixed − gross × Σpercentage ÷ 100.
 */
export function computeNet(gross, deductions) {
  const total = Number(gross) || 0

  const fixed = deductions
    .filter((d) => d.type === 'Fixed' || d.type === DEDUCTION_TYPE.FIXED)
    .reduce((sum, d) => sum + Number(d.value), 0)

  const percent = deductions
    .filter((d) => d.type === 'Percentage' || d.type === DEDUCTION_TYPE.PERCENTAGE)
    .reduce((sum, d) => sum + Number(d.value), 0)

  return total - fixed - (total * percent) / 100
}
