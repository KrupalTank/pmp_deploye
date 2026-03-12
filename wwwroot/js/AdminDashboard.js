/* AdminDashboard.js - Operational Logic for Placement Portal */

let packageChart;
let companyChart;

// 1. Generic Chart Creation Functions
function createBarChart(canvasId, labels, data, labelText, color = 'rgba(99,102,241,0.6)') {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    if (canvas._chartInstance) canvas._chartInstance.destroy();

    canvas._chartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: labelText,
                data,
                backgroundColor: color,
                borderRadius: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true } },
            plugins: { legend: { display: false } }
        }
    });
}

// 2. Dual Axis Chart (Count + Percentage)
function createDualChart(canvasId, labels, countData, percentageData, countLabel) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    if (canvas._chartInstance) canvas._chartInstance.destroy();

    canvas._chartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [
                {
                    label: countLabel,
                    data: countData,
                    backgroundColor: 'rgba(99,102,241,0.6)',
                    yAxisID: 'y',
                    borderRadius: 6
                },
                {
                    label: 'Placement %',
                    data: percentageData,
                    type: 'line', // Line overlay for percentage
                    borderColor: '#f43f5e',
                    backgroundColor: '#f43f5e',
                    yAxisID: 'y1',
                    tension: 0.3,
                    pointRadius: 4
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    type: 'linear',
                    position: 'left',
                    beginAtZero: true,
                    title: { display: true, text: 'Students Placed' },
                    ticks: { stepSize: 1, callback: v => Number.isInteger(v) ? v : '' }
                },
                y1: {
                    type: 'linear',
                    position: 'right',
                    title: { display: true, text: 'Percentage (%)' },
                    min: 0,
                    max: 100,
                    grid: { drawOnChartArea: false } // Keeps the grid clean
                }
            },
            plugins: {
                legend: { display: true, position: 'top' }
            }
        }
    });
}

// 3. Specialized AJAX Loaders
async function loadYearBranchChart(year) {
    const canvas = document.getElementById('chartCurrentYearBranch');
    const yearNoData = document.getElementById('yearNoData');
    const currentYearLabel = document.getElementById('currentYearLabel');

    if (!year) {
        if (canvas._chartInstance) canvas._chartInstance.destroy();
        yearNoData.style.display = 'none';
        currentYearLabel.textContent = '';
        return;
    }

    try {
        const res = await fetch(`/admin/getplacementsbyyear/${year}`);
        const json = await res.json();

        const labels = json.labels || [];
        const counts = json.counts || [];
        const percentages = json.percentages || []; // Capture new data
        const total = counts.reduce((s, v) => s + (Number(v) || 0), 0);

        if (!labels.length || total === 0) {
            if (canvas._chartInstance) canvas._chartInstance.destroy();
            yearNoData.style.display = 'inline';
            currentYearLabel.textContent = year;
            return;
        }

        yearNoData.style.display = 'none';
        currentYearLabel.textContent = year;

        // Corrected: Use createDualChart to show the percentage line
        createDualChart('chartCurrentYearBranch', labels, counts, percentages, 'Placements in ' + year);
    } catch (err) {
        console.error('Failed to load year data', err);
    }
}

async function loadBranchYearChart(bid) {
    const canvas = document.getElementById('chartBranchYear');
    const branchNoData = document.getElementById('branchNoData');
    if (!bid) {
        if (canvas._chartInstance) canvas._chartInstance.destroy();
        branchNoData.style.display = 'none';
        return;
    }

    try {
        const res = await fetch(`/admin/getplacementsbybranch/${bid}`);
        const json = await res.json();
        const labels = json.labels || [];
        const counts = json.counts || [];
        const percentages = json.percentages || []; // Correctly fetched from updated controller
        const total = counts.reduce((s, v) => s + (Number(v) || 0), 0);

        if (!labels.length || total === 0) {
            if (canvas._chartInstance) canvas._chartInstance.destroy();
            branchNoData.style.display = 'inline';
            return;
        }

        branchNoData.style.display = 'none';
        createDualChart('chartBranchYear', labels, counts, percentages, 'Placements');
    } catch (err) {
        console.error('Error loading branch chart:', err);
    }
}

async function loadPackageStats(year = 0) {
    const res = await fetch(`/admin/getpackagestats/${year}`);
    const json = await res.json();
    const ctx = document.getElementById('chartPackageStats').getContext('2d');

    if (packageChart) packageChart.destroy();

    packageChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: json.labels,
            datasets: [
                {
                    label: 'Average Package (LPA)',
                    data: json.average,
                    backgroundColor: 'rgba(99, 102, 241, 0.6)',
                    borderRadius: 5
                },
                {
                    label: 'Highest Package (LPA)',
                    data: json.highest,
                    backgroundColor: 'rgba(244, 63, 94, 0.6)',
                    borderRadius: 5
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: { y: { beginAtZero: true, title: { display: true, text: 'LPA' } } }
        }
    });
}

async function loadCompanyPlacementChart(year = 0) {
    try {
        const res = await fetch(`/admin/getcompanyplacementstats/${year}`);
        const json = await res.json();
        const canvas = document.getElementById('chartCompanyPlacements');
        const wrapper = document.getElementById('companyCanvasWrapper');
        if (!canvas) return;

        const barHeight = 40;
        const minHeight = 380;
        const calculatedHeight = Math.max(minHeight, json.labels.length * barHeight);

        wrapper.style.height = calculatedHeight + "px";
        canvas.style.height = calculatedHeight + "px";
        canvas.height = calculatedHeight;

        if (companyChart) companyChart.destroy();

        const ctx = canvas.getContext('2d');
        companyChart = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: json.labels,
                datasets: [{
                    label: 'Students Placed',
                    data: json.counts,
                    backgroundColor: 'rgba(59, 130, 246, 0.7)',
                    borderRadius: 4
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: { beginAtZero: true, ticks: { stepSize: 1 } },
                    y: { ticks: { autoSkip: false } }
                },
                plugins: { legend: { display: false } }
            }
        });
    } catch (err) {
        console.error('Error loading company chart:', err);
    }
}

async function loadAuditFeed(start = null, end = null) {
    let url = '/admin/getauditlogs';
    if (start && end) url += `?start=${start}&end=${end}`;

    const res = await fetch(url);
    const logs = await res.json();
    const container = document.getElementById('auditFeedContainer');

    container.innerHTML = logs.map(log => `
        <div class="activity-item">
            <div class="activity-icon ${log.action.toLowerCase()}">
                <span>${log.action.substring(0, 1)}</span>
            </div>
            <div class="activity-details">
                <p class="activity-text"><strong>${log.action}:</strong> ${log.detail}</p>
                <p class="activity-text"><strong>Done By:</strong> ${log.doneBy}</p>
                <span class="activity-time">${log.displayTime}</span>
            </div>
        </div>
    `).join('') || '<p class="no-data">No logs found for this range.</p>';
}

// 4. Initialization
function initDashboardEventListeners() {
    document.getElementById('yearSelect')?.addEventListener('change', e => loadYearBranchChart(e.target.value));
    document.getElementById('branchSelect')?.addEventListener('change', e => loadBranchYearChart(e.target.value));
    document.getElementById('packageYearSelect')?.addEventListener('change', e => loadPackageStats(e.target.value));
    document.getElementById('companyYearSelect')?.addEventListener('change', e => loadCompanyPlacementChart(e.target.value));

    document.getElementById('btnFilterAudit')?.addEventListener('click', () => {
        const start = document.getElementById('auditStart').value;
        const end = document.getElementById('auditEnd').value;
        if (start && end) loadAuditFeed(start, end);
    });

    document.getElementById('btnResetAudit')?.addEventListener('click', () => {
        document.getElementById('auditStart').value = '';
        document.getElementById('auditEnd').value = '';
        loadAuditFeed();
    });
}

function downloadDynamicReport() {
    const year = document.getElementById('reportYearSelect').value;
    const options = {
        summary: document.getElementById('chkSummary').checked,
        growth: document.getElementById('chkGrowth').checked,
        branch: document.getElementById('chkBranchEff').checked,
        corp: document.getElementById('chkCompanies').checked,
        details: document.getElementById('chkDetails').checked
    };

    // Construct URL with flags
    const url = `/admin/generateannualreport/${year}?summary=${options.summary}&growth=${options.growth}&branch=${options.branch}&corp=${options.corp}&details=${options.details}`;
    window.location.href = url;
}