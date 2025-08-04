// Feed JavaScript Functionality
document.addEventListener('DOMContentLoaded', function () {
    // Like functionality
    document.querySelectorAll('.like-btn').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const achievementId = this.dataset.achievementId;
            const icon = this.querySelector('i');
            const isLiked = this.classList.contains('liked');
            
            // Make AJAX call to toggle like
            fetch('/Feed/ToggleLike', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({ achievementId: achievementId })
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    if (data.isLiked) {
                        this.classList.add('liked');
                        icon.className = 'bi bi-heart-fill';
                    } else {
                        this.classList.remove('liked');
                        icon.className = 'bi bi-heart';
                    }
                    
                    // Update like count
                    const statsElement = this.closest('.achievement-card').querySelector('.achievement-stats span');
                    const currentComments = statsElement.textContent.split('•')[1].trim();
                    statsElement.textContent = `${data.likesCount} إعجاب • ${currentComments}`;
                } else {
                    alert(data.message || 'حدث خطأ أثناء تحديث الإعجاب');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('حدث خطأ أثناء تحديث الإعجاب');
            });
        });
    });

    // Comment toggle functionality
    document.querySelectorAll('.comment-btn').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const achievementId = this.dataset.achievementId;
            const commentsSection = document.getElementById(`comments-${achievementId}`);
            
            if (commentsSection.style.display === 'none') {
                commentsSection.style.display = 'block';
            } else {
                commentsSection.style.display = 'none';
            }
        });
    });

    // Post comment functionality
    document.querySelectorAll('.post-comment-btn').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const commentInput = this.previousElementSibling;
            const commentText = commentInput.value.trim();
            
            if (commentText) {
                const achievementCard = this.closest('.achievement-card');
                const commentsSection = achievementCard.querySelector('.comments-section');
                const achievementId = achievementCard.dataset.achievementId;
                
                // Make AJAX call to add comment
                fetch('/Feed/AddComment', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    },
                    body: JSON.stringify({ 
                        achievementId: achievementId,
                        content: commentText 
                    })
                })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        // Create new comment element
                        const newComment = document.createElement('div');
                        newComment.className = 'comment';
                        newComment.innerHTML = `
                            <div class="comment-pic">${data.comment.user.name.charAt(0)}</div>
                            <div class="comment-content">
                                <div class="comment-header">
                                    <span class="comment-author">${data.comment.user.name}</span>
                                    <span class="comment-date">الآن</span>
                                </div>
                                <div class="comment-text">${data.comment.content}</div>
                            </div>
                        `;
                        
                        // Insert before the add-comment div
                        const addCommentDiv = commentsSection.querySelector('.add-comment');
                        commentsSection.insertBefore(newComment, addCommentDiv);
                        
                        // Clear input
                        commentInput.value = '';
                        
                        // Update comment count
                        const statsElement = achievementCard.querySelector('.achievement-stats span');
                        const currentLikes = statsElement.textContent.split('•')[0].trim();
                        statsElement.textContent = `${currentLikes} • ${data.commentsCount} تعليق`;
                    } else {
                        alert(data.message || 'حدث خطأ أثناء إضافة التعليق');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    alert('حدث خطأ أثناء إضافة التعليق');
                });
            }
        });
    });

    // Search functionality
    const searchInput = document.querySelector('.search-input');
    const searchIdInput = document.querySelector('.search-id-input');
    function filterAchievements() {
        const searchTerm = searchInput ? searchInput.value.toLowerCase() : '';
        const searchId = searchIdInput ? searchIdInput.value.trim() : '';
        const achievementCards = document.querySelectorAll('.achievement-card');
        achievementCards.forEach(function(card) {
            const title = card.querySelector('.achievement-title').textContent.toLowerCase();
            const description = card.querySelector('.achievement-description').textContent.toLowerCase();
            const ownerName = card.querySelector('.user-info h6').textContent.toLowerCase();
            const cardId = card.dataset.achievementId.toString();
            // Text filter
            const matchesText = title.includes(searchTerm) || description.includes(searchTerm) || ownerName.includes(searchTerm);
            // ID filter (if present)
            const matchesId = !searchId || cardId === searchId;
            if (matchesText && matchesId) {
                card.style.display = 'block';
            } else {
                card.style.display = 'none';
            }
        });
    }
    if (searchInput) {
        searchInput.addEventListener('input', filterAchievements);
    }
    if (searchIdInput) {
        searchIdInput.addEventListener('input', filterAchievements);
    }

    // Delete achievement functionality
    window.deleteAchievement = function(achievementId) {
        if (confirm('هل أنت متأكد من حذف هذا الإنجاز؟')) {
            const form = document.createElement('form');
            form.method = 'POST';
            form.action = '/Achievement/Delete';
            
            const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;
            const csrfInput = document.createElement('input');
            csrfInput.type = 'hidden';
            csrfInput.name = '__RequestVerificationToken';
            csrfInput.value = csrfToken;
            
            const idInput = document.createElement('input');
            idInput.type = 'hidden';
            idInput.name = 'id';
            idInput.value = achievementId;
            
            form.appendChild(csrfInput);
            form.appendChild(idInput);
            document.body.appendChild(form);
            form.submit();
        }
    };
}); 