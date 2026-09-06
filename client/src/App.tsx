import { useEffect, useState } from 'react'
import { askBusinessAssistant, checkServerHealth, getFinanceSummary, type FinanceSummary } from './services/api'
import './App.css'

const navigation = [
  { label: 'Tổng quan', key: 'Overview', group: 'Không gian làm việc' },
  { label: 'Bán hàng', key: 'Sales', group: 'Vận hành' },
  { label: 'Tồn kho', key: 'Inventory', group: 'Vận hành' },
  { label: 'Mua hàng', key: 'Purchasing', group: 'Vận hành' },
  { label: 'Tài chính', key: 'Finance', group: 'Vận hành' },
  { label: 'Digital Twin', key: 'Digital Twin', group: 'Trí tuệ' },
  { label: 'Mô phỏng', key: 'Simulation', group: 'Trí tuệ' },
  { label: 'AI Center', key: 'AI Center', group: 'Trí tuệ' },
  { label: 'Báo cáo', key: 'Reports', group: 'Không gian làm việc' },
]

const revenueData = [
  { month: 'Apr', value: 46 },
  { month: 'May', value: 58 },
  { month: 'Jun', value: 54 },
  { month: 'Jul', value: 68 },
  { month: 'Aug', value: 74 },
  { month: 'Sep', value: 88 },
]

const kpis = [
  { label: 'Revenue', value: '$2.42M', delta: '+7.2%', tone: 'green' },
  { label: 'Profit', value: '$684K', delta: '+4.8%', tone: 'green' },
  { label: 'Orders', value: '3,421', delta: '+12.4%', tone: 'blue' },
  { label: 'Inventory', value: '$812K', delta: '-2.1%', tone: 'amber' },
]

const moduleRows: Record<string, { columns: string[]; rows: string[][] }> = {
  Sales: {
    columns: ['Đơn hàng', 'Khách hàng', 'Giá trị', 'Trạng thái'],
    rows: [['SO-1048', 'Northstar Retail', '$18,240', 'Đang xử lý'], ['SO-1047', 'Minh Anh Co.', '$9,840', 'Đã xác nhận'], ['SO-1046', 'Vega Market', '$6,120', 'Hoàn tất']],
  },
  Inventory: {
    columns: ['Sản phẩm', 'Kho', 'Khả dụng', 'Tín hiệu'],
    rows: [['Product A', 'Kho miền Bắc', '124 đơn vị', 'Sắp nhập thêm'], ['Product B', 'Kho trung tâm', '840 đơn vị', 'Ổn định'], ['Product C', 'Kho miền Nam', '38 đơn vị', 'Tồn thấp']],
  },
  Purchasing: {
    columns: ['Đơn mua', 'Nhà cung cấp', 'Giá trị', 'Trạng thái'],
    rows: [['PO-208', 'Supplier B', '$24,500', 'Đã nhận một phần'], ['PO-207', 'Supplier X', '$12,800', 'Đã đặt hàng'], ['PO-206', 'Supplier A', '$8,420', 'Đã nhận đủ']],
  },
  Finance: {
    columns: ['Khoản mục', 'Tham chiếu', 'Số tiền', 'Loại'],
    rows: [['Doanh thu', 'SO-1048', '$18,240', 'Khoản thu'], ['Chi phí', 'PO-208', '$24,500', 'Khoản chi'], ['Khách thanh toán', 'PAY-884', '$12,400', 'Tiền vào']],
  },
  Reports: {
    columns: ['Báo cáo', 'Kỳ báo cáo', 'Cập nhật', 'Thao tác'],
    rows: [['Hiệu suất bán hàng', 'Tháng 9/2026', '2 phút trước', 'Mở'], ['Tuổi tồn kho', 'Tháng 9/2026', '18 phút trước', 'Mở'], ['Tổng quan tài chính', 'Tháng 9/2026', '1 giờ trước', 'Mở']],
  },
}

function ModuleView({ page, assistantAnswer, isAsking, onAsk }: { page: string; assistantAnswer: string; isAsking: boolean; onAsk: (question: string) => void }) {
  const [question, setQuestion] = useState('')
  if (page === 'Digital Twin') {
    return <section className="module-grid"><article className="panel twin-state"><div className="panel-heading"><div><span className="panel-kicker">Snapshot hiện tại · v12</span><h2>Trạng thái doanh nghiệp</h2></div><span className="healthy-label state-badge">Đang hoạt động</span></div><div className="state-metrics"><div><span>Doanh thu</span><strong>$2.42M</strong></div><div><span>Lợi nhuận</span><strong>$684K</strong></div><div><span>Dòng tiền</span><strong>+$124K</strong></div><div><span>Đơn hàng</span><strong>3,421</strong></div></div></article><article className="panel timeline-panel"><div className="panel-heading"><div><span className="panel-kicker">Lịch sử phiên bản</span><h2>Dòng thời gian</h2></div><button className="text-button" type="button">So sánh snapshot →</button></div><div className="timeline"><div><b>Hôm nay</b><span>Snapshot v12 · Đã đồng bộ toàn hệ thống</span></div><div><b>05/09</b><span>Snapshot v11 · Doanh thu vượt $2.4M</span></div><div><b>01/09</b><span>Snapshot v10 · Phát hiện cảnh báo tồn kho</span></div></div></article></section>
  }

  if (page === 'Simulation') {
    return <section className="module-grid"><article className="panel scenario-panel"><div className="panel-heading"><div><span className="panel-kicker">Phòng mô phỏng What-if</span><h2>Tham số kịch bản</h2></div><span className="alert-count">Bản nháp</span></div><div className="scenario-fields"><label>Giá sản phẩm <input type="range" min="-10" max="20" defaultValue="5" /><span>+5%</span></label><label>Nhu cầu <input type="range" min="-20" max="30" defaultValue="10" /><span>+10%</span></label><label>Chi phí marketing <input type="range" min="-20" max="30" defaultValue="15" /><span>+15%</span></label></div><button className="primary-button" type="button">Chạy mô phỏng</button></article><article className="panel result-panel"><div className="panel-heading"><div><span className="panel-kicker">Kết quả mô phỏng</span><h2>Kịch bản A</h2></div><span className="healthy-label state-badge">Rủi ro thấp</span></div><div className="state-metrics"><div><span>Doanh thu</span><strong>+8.2%</strong></div><div><span>Lợi nhuận</span><strong>+11.4%</strong></div><div><span>Tồn kho</span><strong>-4.5%</strong></div><div><span>Rủi ro</span><strong>Thấp</strong></div></div></article></section>
  }

  if (page === 'AI Center') {
    return <section className="module-grid"><article className="panel ai-chat-panel"><div className="panel-heading"><div><span className="panel-kicker">Trợ lý doanh nghiệp</span><h2>Hỏi dữ liệu kinh doanh</h2></div><span className="healthy-label state-badge">Đã kết nối facts</span></div><div className="chat-bubble">Hôm nay tôi cần chú ý điều gì?</div><div className="chat-answer"><strong>{assistantAnswer || 'Trợ lý đang chờ câu hỏi'}</strong><p>Dữ liệu realtime được lấy từ business tool, không suy đoán khi thiếu nguồn.</p></div><form className="chat-input" onSubmit={(event) => { event.preventDefault(); if (question.trim()) onAsk(question.trim()) }}><input aria-label="Hỏi trợ lý doanh nghiệp" value={question} onChange={(event) => setQuestion(event.target.value)} placeholder="Ví dụ: Doanh thu tháng này thế nào?" /><button className="primary-button" disabled={isAsking} type="submit">{isAsking ? 'Đang hỏi...' : 'Hỏi AI'}</button></form></article><article className="panel recommendation-panel"><div className="panel-heading"><div><span className="panel-kicker">Đề xuất hành động</span><h2>Việc nên làm tiếp theo</h2></div></div><div className="insight-list"><div className="insight-item"><span className="insight-marker red" /><div><strong>Kiểm tra bổ sung Product A</strong><p>Ưu tiên cao · Dự báo nhu cầu</p></div></div><div className="insight-item"><span className="insight-marker amber" /><div><strong>Liên hệ Supplier B</strong><p>Ưu tiên vừa · Bất thường lead time</p></div></div></div></article></section>
  }

  const data = moduleRows[page] ?? moduleRows.Reports
    return <section className="panel table-panel"><div className="panel-heading"><div><span className="panel-kicker">Không gian {navigation.find((item) => item.key === page)?.label?.toLowerCase() ?? page.toLowerCase()}</span><h2>Hoạt động gần đây</h2></div><button className="primary-button" type="button">Tạo mới</button></div><div className="data-table" role="table"><div className="table-row table-header">{data.columns.map((column) => <span key={column}>{column}</span>)}</div>{data.rows.map((row) => <div className="table-row" key={row[0]}>{row.map((cell, index) => <span className={index === row.length - 1 ? 'table-status' : ''} key={`${row[0]}-${cell}`}>{cell}</span>)}</div>)}</div></section>
}

function App() {
  const [activePage, setActivePage] = useState('Overview')
  const [period, setPeriod] = useState('30 ngày gần đây')
  const [lastSynced, setLastSynced] = useState('2 min ago')
  const [finance, setFinance] = useState<FinanceSummary | null>(null)
  const [serverOnline, setServerOnline] = useState(false)
  const [isLoading, setIsLoading] = useState(true)
  const [assistantAnswer, setAssistantAnswer] = useState('')
  const [isAsking, setIsAsking] = useState(false)

  useEffect(() => {
    const controller = new AbortController()
    Promise.all([getFinanceSummary(controller.signal), checkServerHealth(controller.signal)])
      .then(([summary, online]) => {
        setFinance(summary)
        setServerOnline(online)
      })
      .catch(() => setServerOnline(false))
      .finally(() => setIsLoading(false))
    return () => controller.abort()
  }, [])

  const refreshData = async () => {
    setIsLoading(true)
    try {
      const [summary, online] = await Promise.all([getFinanceSummary(), checkServerHealth()])
      setFinance(summary)
      setServerOnline(online)
      setLastSynced('Vừa xong')
    } catch {
      setServerOnline(false)
      setLastSynced('Không kết nối được')
    } finally {
      setIsLoading(false)
    }
  }

  const askAssistant = async (question: string) => {
    setIsAsking(true)
    try {
      const response = await askBusinessAssistant(question, {
        revenue: finance?.revenue,
        profit: finance?.profit,
        asOfDate: new Date().toISOString().slice(0, 10),
      })
      setAssistantAnswer(response.answer)
    } catch (error) {
      setAssistantAnswer(error instanceof Error ? error.message : 'Không thể kết nối trợ lý AI.')
    } finally {
      setIsAsking(false)
    }
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand-lockup">
          <div className="brand-mark" aria-hidden="true">BT</div>
          <div><strong>BUSINESS TWIN</strong><span>Decision platform</span></div>
        </div>
        <nav className="main-nav" aria-label="Main navigation">
          {['Không gian làm việc', 'Vận hành', 'Trí tuệ'].map((group) => (
            <div className="nav-group" key={group}>
              <span className="nav-group-label">{group}</span>
              {navigation.filter((item) => item.group === group).map((item) => (
                <button className={`nav-item ${activePage === item.key ? 'active' : ''}`} key={item.key} onClick={() => setActivePage(item.key)} type="button">
                  <span className="nav-dot" aria-hidden="true" />{item.label}
                </button>
              ))}
            </div>
          ))}
        </nav>
        <div className="sidebar-footer">
          <button className="nav-item" type="button" onClick={() => setActivePage('Settings')}><span className="nav-dot" aria-hidden="true" />Cài đặt</button>
          <div className="user-chip"><div className="avatar">NL</div><div><strong>Nguyen Long</strong><span>Administrator</span></div><span className="chevron" aria-hidden="true">›</span></div>
        </div>
      </aside>

      <main className="main-content">
        <header className="topbar">
          <div className="breadcrumb"><span>Không gian làm việc</span><b>/</b><strong>{navigation.find((item) => item.key === activePage)?.label ?? activePage}</strong></div>
          <div className="topbar-actions">
            <span className="sync-status"><span className={`status-dot ${serverOnline ? '' : 'offline'}`} /> {serverOnline ? `Server đang hoạt động · ${lastSynced}` : 'Server chưa kết nối'}</span>
            <button className="icon-button" aria-label="Refresh business data" onClick={refreshData} type="button">↻</button>
            <button className="profile-button" type="button"><span className="avatar small">NL</span><span>Nguyễn Long</span><span className="chevron">⌄</span></button>
          </div>
        </header>

        <div className="page-content">
          <section className="page-heading">
            <div><p className="eyebrow">Thứ hai, ngày 6 tháng 9, 2026</p><h1>{activePage === 'Overview' ? 'Chào buổi sáng, Nguyễn' : navigation.find((item) => item.key === activePage)?.label ?? activePage}</h1><p className="heading-copy">Tóm tắt tình hình hoạt động của doanh nghiệp hôm nay.</p></div>
            <div className="heading-controls">
              <label className="select-control"><span className="sr-only">Khoảng thời gian báo cáo</span><select value={period} onChange={(event) => setPeriod(event.target.value)}><option>30 ngày gần đây</option><option>90 ngày gần đây</option><option>Năm nay</option></select><span aria-hidden="true">⌄</span></label>
              <button className="primary-button" type="button" disabled={isLoading} onClick={refreshData}>{isLoading ? 'Đang tải...' : 'Làm mới dữ liệu'}</button>
            </div>
          </section>

          {activePage === 'Overview' ? <>
          <section className="kpi-grid" aria-label="Các chỉ số kinh doanh">
            {kpis.map((kpi) => {
              const value = kpi.label === 'Revenue' && finance ? `$${(finance.revenue / 1_000_000).toFixed(2)}M` : kpi.label === 'Profit' && finance ? `$${(finance.profit / 1_000).toFixed(0)}K` : kpi.value
              const label = kpi.label === 'Revenue' ? 'Doanh thu' : kpi.label === 'Profit' ? 'Lợi nhuận' : kpi.label === 'Orders' ? 'Đơn hàng' : 'Tồn kho'
              return <article className="kpi-card" key={kpi.label}><div className="kpi-topline"><span>{label}</span><span className={`metric-icon ${kpi.tone}`} aria-hidden="true" /></div><strong>{value}</strong><div className="kpi-bottom"><span className={`delta ${kpi.tone}`}>{kpi.delta}</span><span>so với kỳ trước</span></div></article>
            })}
          </section>

          <section className="dashboard-grid">
            <article className="panel revenue-panel">
              <div className="panel-heading"><div><span className="panel-kicker">Hiệu suất</span><h2>Tổng quan doanh thu</h2></div><button className="text-button" type="button">Xem báo cáo <span aria-hidden="true">→</span></button></div>
              <div className="chart-summary"><strong>$2.42M</strong><span className="delta green">+7.2%</span><span>so với kỳ trước</span></div>
              <div className="bar-chart" aria-label="Doanh thu tăng từ tháng 4 đến tháng 9" role="img">{revenueData.map((point) => <div className="bar-column" key={point.month}><div className="bar-track"><div className="bar" style={{ height: `${point.value}%` }} /></div><span>{point.month}</span></div>)}</div>
            </article>

            <article className="panel health-panel">
              <div className="panel-heading"><div><span className="panel-kicker">Digital Twin</span><h2>Sức khỏe doanh nghiệp</h2></div><button className="more-button" aria-label="Thêm tùy chọn sức khỏe doanh nghiệp" type="button">•••</button></div>
              <div className="health-score"><div className="score-ring"><strong>86</strong><span>/ 100</span></div><div><strong className="healthy-label">Ổn định</strong><p>Hoạt động đang cao hơn ngưỡng mục tiêu.</p></div></div>
              <div className="health-list"><div><span>Bán hàng</span><b className="healthy-label">Ổn định</b></div><div><span>Tồn kho</span><b className="warning-label">Cần chú ý</b></div><div><span>Tài chính</span><b className="healthy-label">Ổn định</b></div></div>
            </article>
          </section>

          <section className="dashboard-grid lower-grid">
            <article className="panel insights-panel">
              <div className="panel-heading"><div><span className="panel-kicker">Trí tuệ dữ liệu</span><h2>Phân tích từ AI</h2></div><button className="text-button" type="button" onClick={() => setActivePage('AI Center')}>Mở AI Center <span aria-hidden="true">→</span></button></div>
              <div className="insight-list"><div className="insight-item"><span className="insight-marker blue" /><div><strong>Dự báo doanh thu đang tăng</strong><p>Dự kiến tăng 7.2% trong 30 ngày tới.</p></div><span className="insight-arrow">→</span></div><div className="insight-item"><span className="insight-marker amber" /><div><strong>Product A có thể hết hàng sau 8 ngày</strong><p>Nhu cầu đang vượt kế hoạch bổ sung hiện tại.</p></div><span className="insight-arrow">→</span></div><div className="insight-item"><span className="insight-marker red" /><div><strong>Hiệu suất Supplier B giảm</strong><p>Thời gian giao hàng trung bình tăng 12% kỳ này.</p></div><span className="insight-arrow">→</span></div></div>
            </article>
            <article className="panel alerts-panel">
              <div className="panel-heading"><div><span className="panel-kicker">Cần xử lý</span><h2>Cảnh báo hiện tại</h2></div><span className="alert-count">3 đang mở</span></div>
              <div className="alert-list"><div className="alert-row"><span className="alert-symbol red" aria-hidden="true">!</span><div><strong>Thiếu tồn kho</strong><span>Product A · Ưu tiên cao</span></div><button aria-label="Mở cảnh báo thiếu tồn kho" type="button">›</button></div><div className="alert-row"><span className="alert-symbol amber" aria-hidden="true">!</span><div><strong>Nhà cung cấp giao trễ</strong><span>Supplier B · Trễ 2 ngày</span></div><button aria-label="Mở cảnh báo giao trễ" type="button">›</button></div><div className="alert-row"><span className="alert-symbol blue" aria-hidden="true">i</span><div><strong>Bất thường doanh thu</strong><span>Cần xem xu hướng tháng 9</span></div><button aria-label="Mở cảnh báo doanh thu" type="button">›</button></div></div>
            </article>
          </section>
          </> : <ModuleView page={activePage} assistantAnswer={assistantAnswer} isAsking={isAsking} onAsk={askAssistant} />}
        </div>
      </main>
    </div>
  )
}

export default App
