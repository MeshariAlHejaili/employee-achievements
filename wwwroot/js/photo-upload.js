// Photo Upload JavaScript
class PhotoUploadManager {
    constructor() {
        this.maxFiles = 4;
        this.maxFileSize = 2 * 1024 * 1024; // 2MB
        this.allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/bmp'];
        this.uploadArea = document.getElementById('photoUploadArea');
        this.fileInput = document.getElementById('photoInput');
        this.previewContainer = document.getElementById('photoPreviewContainer');
        this.selectedFiles = new Map();
        
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.setupDragAndDrop();
    }

    setupEventListeners() {
        // File input change
        this.fileInput.addEventListener('change', (e) => {
            this.handleFileSelection(e.target.files);
        });

        // Click to upload
        this.uploadArea.addEventListener('click', () => {
            this.fileInput.click();
        });

        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            this.uploadArea.addEventListener(eventName, (e) => {
                e.preventDefault();
                e.stopPropagation();
            });
        });
    }

    setupDragAndDrop() {
        // Drag enter
        this.uploadArea.addEventListener('dragenter', (e) => {
            this.uploadArea.classList.add('dragover');
        });

        // Drag over
        this.uploadArea.addEventListener('dragover', (e) => {
            this.uploadArea.classList.add('dragover');
        });

        // Drag leave
        this.uploadArea.addEventListener('dragleave', (e) => {
            if (!this.uploadArea.contains(e.relatedTarget)) {
                this.uploadArea.classList.remove('dragover');
            }
        });

        // Drop
        this.uploadArea.addEventListener('drop', (e) => {
            this.uploadArea.classList.remove('dragover');
            const files = Array.from(e.dataTransfer.files);
            this.handleFileSelection(files);
        });
    }

    handleFileSelection(files) {
        const validFiles = Array.from(files).filter(file => this.validateFile(file));
        
        if (this.selectedFiles.size + validFiles.length > this.maxFiles) {
            this.showError(`يمكنك رفع ${this.maxFiles} صور كحد أقصى`);
            return;
        }

        validFiles.forEach(file => {
            this.addFile(file);
        });
    }

    validateFile(file) {
        // Check file type
        if (!this.allowedTypes.includes(file.type)) {
            this.showError(`نوع الملف غير مدعوم: ${file.name}`);
            return false;
        }

        // Check file size
        if (file.size > this.maxFileSize) {
            this.showError(`حجم الملف كبير جداً: ${file.name}`);
            return false;
        }

        // Check if file already exists
        if (this.selectedFiles.has(file.name)) {
            this.showError(`الملف موجود مسبقاً: ${file.name}`);
            return false;
        }

        return true;
    }

    addFile(file) {
        const fileId = this.generateFileId();
        this.selectedFiles.set(fileId, file);

        const reader = new FileReader();
        reader.onload = (e) => {
            this.createPreviewItem(fileId, file, e.target.result);
        };
        reader.readAsDataURL(file);

        // Update file input
        this.updateFileInput();
    }

    createPreviewItem(fileId, file, dataUrl) {
        const previewItem = document.createElement('div');
        previewItem.className = 'photo-preview-item';
        previewItem.dataset.fileId = fileId;

        const fileSize = this.formatFileSize(file.size);
        const fileName = file.name.length > 20 ? file.name.substring(0, 17) + '...' : file.name;

        previewItem.innerHTML = `
            <img src="${dataUrl}" alt="${file.name}" />
            <div class="photo-preview-overlay">
                <button type="button" onclick="photoUploadManager.removeFile('${fileId}')">
                    <i class="bi bi-x"></i>
                </button>
            </div>
            <div class="photo-preview-info">
                <div class="file-name">${fileName}</div>
                <div class="file-size">${fileSize}</div>
            </div>
        `;

        this.previewContainer.appendChild(previewItem);
    }

    removeFile(fileId) {
        const previewItem = this.previewContainer.querySelector(`[data-file-id="${fileId}"]`);
        if (previewItem) {
            previewItem.remove();
        }

        this.selectedFiles.delete(fileId);
        this.updateFileInput();
        this.clearError();
    }

    updateFileInput() {
        // Create a new FileList-like object
        const dt = new DataTransfer();
        this.selectedFiles.forEach(file => {
            dt.items.add(file);
        });
        this.fileInput.files = dt.files;
    }

    generateFileId() {
        return 'file_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    showError(message) {
        this.clearError();
        const errorDiv = document.createElement('div');
        errorDiv.className = 'photo-error-message';
        errorDiv.textContent = message;
        this.uploadArea.parentNode.appendChild(errorDiv);
        
        this.uploadArea.classList.add('error');
        
        // Auto-remove error after 5 seconds
        setTimeout(() => {
            this.clearError();
        }, 5000);
    }

    clearError() {
        const existingError = this.uploadArea.parentNode.querySelector('.photo-error-message');
        if (existingError) {
            existingError.remove();
        }
        this.uploadArea.classList.remove('error');
    }

    getSelectedFiles() {
        return Array.from(this.selectedFiles.values());
    }

    clearAll() {
        this.selectedFiles.clear();
        this.previewContainer.innerHTML = '';
        this.updateFileInput();
        this.clearError();
    }
}

// Photo Gallery JavaScript
class PhotoGallery {
    constructor() {
        this.currentIndex = 0;
        this.photos = [];
        this.modal = null;
        this.init();
    }

    init() {
        this.createModal();
        this.setupEventListeners();
    }

    createModal() {
        this.modal = document.createElement('div');
        this.modal.className = 'photo-gallery-modal';
        this.modal.innerHTML = `
            <div class="photo-gallery-content">
                <button class="photo-gallery-close" onclick="photoGallery.close()">
                    <i class="bi bi-x"></i>
                </button>
                <img src="" alt="Gallery Image" />
                <button class="photo-gallery-nav prev" onclick="photoGallery.prev()">
                    <i class="bi bi-chevron-left"></i>
                </button>
                <button class="photo-gallery-nav next" onclick="photoGallery.next()">
                    <i class="bi bi-chevron-right"></i>
                </button>
                <div class="photo-gallery-counter"></div>
            </div>
        `;
        document.body.appendChild(this.modal);
    }

    setupEventListeners() {
        // Close on escape key
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && this.modal.classList.contains('show')) {
                this.close();
            }
        });

        // Close on background click
        this.modal.addEventListener('click', (e) => {
            if (e.target === this.modal) {
                this.close();
            }
        });

        // Arrow keys navigation
        document.addEventListener('keydown', (e) => {
            if (!this.modal.classList.contains('show')) return;
            
            if (e.key === 'ArrowLeft') {
                this.prev();
            } else if (e.key === 'ArrowRight') {
                this.next();
            }
        });
    }

    open(photos, startIndex = 0) {
        this.photos = photos;
        this.currentIndex = startIndex;
        this.updateDisplay();
        this.modal.classList.add('show');
        document.body.style.overflow = 'hidden';
    }

    close() {
        this.modal.classList.remove('show');
        document.body.style.overflow = '';
        this.photos = [];
        this.currentIndex = 0;
    }

    prev() {
        if (this.photos.length === 0) return;
        this.currentIndex = (this.currentIndex - 1 + this.photos.length) % this.photos.length;
        this.updateDisplay();
    }

    next() {
        if (this.photos.length === 0) return;
        this.currentIndex = (this.currentIndex + 1) % this.photos.length;
        this.updateDisplay();
    }

    updateDisplay() {
        if (this.photos.length === 0) return;

        const img = this.modal.querySelector('img');
        const counter = this.modal.querySelector('.photo-gallery-counter');
        const prevBtn = this.modal.querySelector('.prev');
        const nextBtn = this.modal.querySelector('.next');

        img.src = this.photos[this.currentIndex];
        counter.textContent = `${this.currentIndex + 1} / ${this.photos.length}`;

        // Show/hide navigation buttons
        prevBtn.style.display = this.photos.length > 1 ? 'flex' : 'none';
        nextBtn.style.display = this.photos.length > 1 ? 'flex' : 'none';
    }
}

// Initialize photo upload manager
let photoUploadManager;
let photoGallery;

document.addEventListener('DOMContentLoaded', function() {
    // Initialize photo upload if on add achievement page
    if (document.getElementById('photoUploadArea')) {
        photoUploadManager = new PhotoUploadManager();
    }

    // Initialize photo gallery
    photoGallery = new PhotoGallery();

    // Setup photo gallery triggers
    setupPhotoGalleryTriggers();
});

function setupPhotoGalleryTriggers() {
    // Add click handlers to achievement photos
    document.addEventListener('click', function(e) {
        if (e.target.closest('.achievement-photos-container')) {
            const container = e.target.closest('.achievement-photos-container');
            const photos = Array.from(container.querySelectorAll('img')).map(img => img.src);
            
            if (photos.length > 0) {
                const clickedImg = e.target.closest('img');
                const startIndex = clickedImg ? Array.from(container.querySelectorAll('img')).indexOf(clickedImg) : 0;
                photoGallery.open(photos, startIndex);
            }
        }
    });
}

// Global functions for form submission
function getSelectedPhotos() {
    if (photoUploadManager) {
        return photoUploadManager.getSelectedFiles();
    }
    return [];
}

function clearPhotoUpload() {
    if (photoUploadManager) {
        photoUploadManager.clearAll();
    }
} 