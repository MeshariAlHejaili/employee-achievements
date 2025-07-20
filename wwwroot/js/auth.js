import { isValidEmail } from './components/isValidEmail.js';
import { copyToClipboard } from './components/copyToClipboard.js';

// Authentication JavaScript Functionality
window.copyToClipboard = function(email, password) {
    copyToClipboard(email, password, event);
};

// Form validation for login
document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.querySelector('form[asp-action="Login"]');
    if (loginForm) {
        loginForm.addEventListener('submit', function(e) {
            const email = document.getElementById('email');
            const password = document.getElementById('password');
            let isValid = true;
            
            // Reset previous errors
            email.classList.remove('is-invalid');
            password.classList.remove('is-invalid');
            
            // Validate email
            if (!email.value.trim()) {
                email.classList.add('is-invalid');
                isValid = false;
            } else if (!isValidEmail(email.value)) {
                email.classList.add('is-invalid');
                isValid = false;
            }
            
            // Validate password
            if (!password.value.trim()) {
                password.classList.add('is-invalid');
                isValid = false;
            }
            
            if (!isValid) {
                e.preventDefault();
                alert('يرجى التأكد من صحة البيانات المدخلة');
            }
        });
    }
    
    // Real-time validation
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(function(input) {
        input.addEventListener('blur', function() {
            if (this.type === 'email' && this.value.trim()) {
                if (!isValidEmail(this.value)) {
                    this.classList.add('is-invalid');
                } else {
                    this.classList.remove('is-invalid');
                }
            } else if (this.hasAttribute('required') && !this.value.trim()) {
                this.classList.add('is-invalid');
            } else {
                this.classList.remove('is-invalid');
            }
        });
        
        input.addEventListener('input', function() {
            if (this.value.trim()) {
                this.classList.remove('is-invalid');
            }
        });
    });
}); 