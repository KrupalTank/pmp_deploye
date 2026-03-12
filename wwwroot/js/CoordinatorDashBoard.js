document.addEventListener('DOMContentLoaded', function () {
    const yearFilter = document.getElementById('yearFilter');
    const placementTable = document.getElementById('placementTable');
    const placementBody = document.getElementById('placementBody');
    const noDataMessage = document.getElementById('noDataMessage');

    yearFilter.addEventListener('change', async function () {
        const yid = this.value;

        if (!yid) {
            placementTable.style.display = 'none';
            noDataMessage.style.display = 'block';
            noDataMessage.innerHTML = '<p class="mb-0">Please select a year to view the official placement ledger.</p>';
            return;
        }

        try {
            const response = await fetch(`/Coordinator/GetBranchPlacements?yid=${yid}`);
            const data = await response.json();

            // Clear previous results
            placementBody.innerHTML = '';

            if (data.length === 0) {
                placementTable.style.display = 'none';
                noDataMessage.style.display = 'block';
                noDataMessage.innerHTML = '<p class="mb-0 text-danger font-weight-bold">No records found for this year.</p>';
                return;
            }

            // Populate Table
            data.forEach(student => {
                const row = `
                    <tr>
                        <td class="ps-4 fw-semibold">${student.sname}</td>
                        <td class="text-muted small">${student.contact}</td>
                        <td><span class="badge bg-primary-subtle text-primary border border-primary">${student.company}</span></td>
                        <td class="text-center fw-bold">${student.package}</td>
                    </tr>
                `;
                placementBody.insertAdjacentHTML('beforeend', row);
            });

            noDataMessage.style.display = 'none';
            placementTable.style.display = 'table';

        } catch (error) {
            console.error("Error fetching branch data:", error);
            alert("Failed to load placement ledger. Please try again.");
        }
    });
});

function downloadCustomReport() {
    const yid = document.getElementById('reportYearId').value;
    const lookback = document.getElementById('lookbackYears').value;
    const includeStudents = document.getElementById('chkStudents').checked;
    const includeResources = document.getElementById('chkResources').checked;
    const includeRounds = document.getElementById('chkRounds').checked; // New 

    if (!yid) {
        alert("Please select a year.");
        return;
    }

    // Updated URL with includeRounds parameter 
    const url = `/Coordinator/GenerateBranchReport?yearId=${yid}&placedStudents=${includeStudents}&resources=${includeResources}&includeRounds=${includeRounds}&experienceYears=${lookback}`;

    window.location.href = url;
}