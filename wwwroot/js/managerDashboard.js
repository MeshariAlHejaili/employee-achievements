function toggleReportFilters() {
    var type = document.getElementById('reportType').value;
    document.getElementById('employeeFilter').style.display = (type === 'byEmployee') ? '' : 'none';
    document.getElementById('dateFilter').style.display = (type === 'byDate') ? '' : 'none';
}
toggleReportFilters();

function approveAchievement(id) {
    if (!confirm('هل أنت متأكد من الموافقة على هذا الإنجاز؟')) return;
    fetch('/Manager/ApproveAchievement', { 
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
    fetch('/Manager/RejectAchievement', {
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
        var type = document.getElementById('reportType').value;
        if (type === 'summary') {
            // Let the form submit normally for PDF download
            this.method = 'GET';
            this.action = '/Manager/GenerateReport';
            return;
        }
        // Otherwise, handle via AJAX
        e.preventDefault();
        var url = '/Manager/GenerateReport?type=' + type;
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

function showAchievementPhotos(achievementId) {
    fetch(`/Achievement/GetAchievementPhotos?achievementId=${achievementId}`)
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                showPhotoModal(data.photos);
            } else {
                alert(data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('حدث خطأ أثناء جلب الصور');
        });
}

function showPhotoModal(photos) {
    // Create modal if it doesn't exist
    let modal = document.getElementById('photoModal');
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'photoModal';
        modal.className = 'modal fade';
        modal.setAttribute('tabindex', '-1');
        modal.innerHTML = `
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">صور الإنجاز</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div id="photoCarousel" class="carousel slide" data-bs-ride="carousel">
                            <div class="carousel-inner" id="photoCarouselInner">
                            </div>
                            <button class="carousel-control-prev" type="button" data-bs-target="#photoCarousel" data-bs-slide="prev">
                                <span class="carousel-control-prev-icon"></span>
                            </button>
                            <button class="carousel-control-next" type="button" data-bs-target="#photoCarousel" data-bs-slide="next">
                                <span class="carousel-control-next-icon"></span>
                            </button>
                        </div>
                        <div class="mt-3">
                            <div class="row" id="photoThumbnails">
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    }

    // Populate carousel
    const carouselInner = modal.querySelector('#photoCarouselInner');
    const thumbnailsContainer = modal.querySelector('#photoThumbnails');
    
    carouselInner.innerHTML = '';
    thumbnailsContainer.innerHTML = '';

    photos.forEach((photo, index) => {
        // Add carousel item
        const carouselItem = document.createElement('div');
        carouselItem.className = `carousel-item ${index === 0 ? 'active' : ''}`;
        carouselItem.innerHTML = `
            <img src="${photo.filePath}" class="d-block w-100" alt="Achievement Photo" style="max-height: 400px; object-fit: contain;">
            <div class="carousel-caption d-none d-md-block">
                <p>${photo.originalFileName}</p>
            </div>
        `;
        carouselInner.appendChild(carouselItem);

        // Add thumbnail
        const thumbnailCol = document.createElement('div');
        thumbnailCol.className = 'col-2 mb-2';
        thumbnailCol.innerHTML = `
            <img src="${photo.thumbnailPath}" class="img-thumbnail" alt="Thumbnail" 
                 style="cursor: pointer; height: 60px; object-fit: cover;" 
                 onclick="goToSlide(${index})">
        `;
        thumbnailsContainer.appendChild(thumbnailCol);
    });

    // Show modal
    const bootstrapModal = new bootstrap.Modal(modal);
    bootstrapModal.show();
}

function goToSlide(index) {
    const carousel = document.querySelector('#photoCarousel');
    const bsCarousel = new bootstrap.Carousel(carousel);
    bsCarousel.to(index);
} 