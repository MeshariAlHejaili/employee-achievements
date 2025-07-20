export function copyToClipboard(email, password, event) {
    // Create a temporary input element
    const tempInput = document.createElement('input');
    tempInput.value = `Email: ${email}\nPassword: ${password}`;
    document.body.appendChild(tempInput);
    
    // Select and copy the text
    tempInput.select();
    document.execCommand('copy');
    
    // Remove the temporary element
    document.body.removeChild(tempInput);
    
    // Show feedback
    const button = event.target;
    const originalText = button.textContent;
    button.textContent = 'تم النسخ!';
    button.classList.add('copied');
    
    setTimeout(() => {
        button.textContent = originalText;
        button.classList.remove('copied');
    }, 2000);
} 