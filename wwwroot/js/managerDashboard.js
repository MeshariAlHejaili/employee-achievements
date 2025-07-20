function toggleReportFilters() {
    var type = document.getElementById('reportType').value;
    document.getElementById('employeeFilter').style.display = (type === 'byEmployee') ? '' : 'none';
    document.getElementById('dateFilter').style.display = (type === 'byDate') ? '' : 'none';
}
toggleReportFilters();

function approveAchievement(id) {
    if (!confirm('هل أنت متأكد من الموافقة على هذا الإنجاز؟')) return;
    fetch('/Home/ApproveAchievement', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id })
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            document.getElementById('achievement-row-' + id).remove();
            alert(data.message);
        } else {
            alert(data.message);
        }
    });
}

function rejectAchievement(id) {
    if (!confirm('هل أنت متأكد من رفض هذا الإنجاز؟')) return;
    fetch('/Home/RejectAchievement', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id })
    })
    .then(res => res.json())
    .then(data => {
        if (data.success) {
            document.getElementById('achievement-row-' + id).remove();
            alert(data.message);
        } else {
            alert(data.message);
        }
    });
}

document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('reportForm').addEventListener('submit', function (e) {
        e.preventDefault();
        var type = document.getElementById('reportType').value;
        var url = '/Home/GenerateReport?type=' + type;
        if (type === 'byEmployee') {
            var empId = document.getElementById('employeeId').value;
            if (!empId) {
                alert('يرجى اختيار الموظف');
                return;
            }
            url += '&employeeId=' + empId;
        }
        if (type === 'byDate') {
            var start = document.getElementById('startDate').value;
            var end = document.getElementById('endDate').value;
            if (!start || !end) {
                alert('يرجى اختيار تاريخ البداية والنهاية');
                return;
            }
            url += '&startDate=' + start + '&endDate=' + end;
        }
        fetch(url)
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    document.getElementById('reportResult').innerHTML = renderReport(data.report, type);
                } else {
                    document.getElementById('reportResult').innerHTML = '<div class="alert alert-danger">' + data.message + '</div>';
                }
            });
    });
});

function renderReport(report, type) {
    if (type === 'byEmployee') {
        let html = '<h5>تقرير حسب الموظف</h5><ul class="list-group">';
        report.forEach(item => {
            html += `<li class="list-group-item">${item.employee} - عدد الإنجازات: ${item.count}</li>`;
        });
        html += '</ul>';
        return html;
    } else if (type === 'byDate') {
        let html = '<h5>تقرير حسب التاريخ</h5><ul class="list-group">';
        report.forEach(item => {
            html += `<li class="list-group-item">${item.date.split('T')[0]} - عدد الإنجازات: ${item.count}</li>`;
        });
        html += '</ul>';
        return html;
    } else {
        return `<h5>ملخص</h5><div>عدد الإنجازات: ${report.total}</div><div>عدد الموظفين: ${report.employees}</div>`;
    }
} 