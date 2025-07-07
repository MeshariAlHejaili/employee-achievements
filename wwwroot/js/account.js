// Account JavaScript Functionality
document.addEventListener('DOMContentLoaded', function () {
    // Card click functionality
    document.querySelectorAll('.card').forEach(function (card) {
        card.addEventListener('click', function () {
            card.classList.toggle('border-success');
            card.classList.toggle('shadow-lg');
        });
    });
    
    // Edit button functionality
    document.querySelectorAll('.edit-btn').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            e.stopPropagation();
            alert('تعديل الإنجاز (قريباً)');
        });
    });
    
    // Delete button functionality
    document.querySelectorAll('.delete-btn').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            e.stopPropagation();
            if (confirm('هل أنت متأكد من حذف هذا الإنجاز؟')) {
                alert('تم حذف الإنجاز (قريباً)');
            }
        });
    });
    
    // Achievement card interactions
    document.querySelectorAll('.achievement-card').forEach(function(card) {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-5px)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });
}); 