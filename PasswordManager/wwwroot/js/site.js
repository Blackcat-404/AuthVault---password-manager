/* ========================================
   WELCOME PAGE SCRIPTS
   ======================================== */

// Smooth scroll to anchor links
document.addEventListener('DOMContentLoaded', function () {
    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href === '#') return;

            e.preventDefault();
            const target = document.querySelector(href);
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Intersection Observer for fade-in animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    // Observe feature cards
    document.querySelectorAll('.feature-card').forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(30px)';
        card.style.transition = `all 0.6s ease ${index * 0.1}s`;
        observer.observe(card);
    });
});

/* ========================================
   AUTH PAGE SCRIPTS (Login/Register)
   ======================================== */

// Toggle password visibility
function togglePassword(inputId, button) {
    const input = document.getElementById(inputId);

    if (!input) return;

    const isHidden = input.type === 'password';

    input.type = isHidden ? 'text' : 'password';

    button.innerHTML = isHidden
        ? getEyeIcon()
        : getEyeOffIcon();
}

function getEyeIcon() {
    return `
    <svg xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        stroke-linecap="round"
        stroke-linejoin="round">
        <path d="M2.062 12.348a1 1 0 0 1 0-.696 10.75 10.75 0 0 1 19.876 0 1 1 0 0 1 0 .696 10.75 10.75 0 0 1-19.876 0"/>
    <circle cx="12" cy="12" r="3"/>
    </svg>`;
}

function getEyeOffIcon() {
    return `
    <svg xmlns="http://www.w3.org/2000/svg"
	    width="24"
	    height="24"
	    viewBox="0 0 24 24"
	    fill="none"
	    stroke="currentColor"
	    stroke-width="2"
	    stroke-linecap="round"
	    stroke-linejoin="round">
	    <path d="m15 18-.722-3.25"/>
	    <path d="M2 8a10.645 10.645 0 0 0 20 0"/>
	    <path d="m20 15-1.726-2.05"/>
	    <path d="m4 15 1.726-2.05"/>
	    <path d="m9 18 .722-3.25"/>
    </svg>`;
}

/* ========================================
   VAULT PAGE SCRIPTS (Dashboard)
   ======================================== */

// Toggle sidebar collapse
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('collapsed');
    localStorage.setItem('sidebar', sidebar.classList.contains('collapsed'));
}


// Password Generator Modal
function openGenerator() {
    const modal = document.getElementById('generator-modal');
    if (modal) {
        modal.classList.add('show');
        generatePassword();
    }
}

function closeGenerator() {
    const modal = document.getElementById('generator-modal');
    if (modal) {
        modal.classList.remove('show');
    }
}

// Update password length display
function updateLength(value) {
    const lengthValue = document.getElementById('length-value');
    if (lengthValue) {
        lengthValue.textContent = value;
        generatePassword();
    }
}

// Generate password
function generatePassword() {
    const lengthSlider = document.getElementById('length-slider');
    const uppercaseCheckbox = document.getElementById('uppercase');
    const lowercaseCheckbox = document.getElementById('lowercase');
    const numbersCheckbox = document.getElementById('numbers');
    const symbolsCheckbox = document.getElementById('symbols');
    const display = document.getElementById('generated-password');

    if (!lengthSlider || !display) return;

    const length = parseInt(lengthSlider.value);
    const useUppercase = uppercaseCheckbox ? uppercaseCheckbox.checked : true;
    const useLowercase = lowercaseCheckbox ? lowercaseCheckbox.checked : true;
    const useNumbers = numbersCheckbox ? numbersCheckbox.checked : true;
    const useSymbols = symbolsCheckbox ? symbolsCheckbox.checked : true;

    let charset = '';
    if (useUppercase) charset += 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    if (useLowercase) charset += 'abcdefghijklmnopqrstuvwxyz';
    if (useNumbers) charset += '0123456789';
    if (useSymbols) charset += '!@#$%^&*()_+-=[]{}|;:,.<>?';

    // If no options selected, use lowercase by default
    if (charset === '') {
        charset = 'abcdefghijklmnopqrstuvwxyz';
    }

    let password = '';
    for (let i = 0; i < length; i++) {
        password += charset.charAt(Math.floor(Math.random() * charset.length));
    }

    display.textContent = password;
}

// Close modal when clicking outside
document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('generator-modal');
    if (modal) {
        modal.addEventListener('click', function (e) {
            if (e.target === this) {
                closeGenerator();
            }
        });
    }
});

// Copy to clipboard function (for future use)
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        // Show success notification
        console.log('Copied to clipboard!');
        // TODO: Add visual feedback
    }).catch(err => {
        console.error('Failed to copy:', err);
    });
}