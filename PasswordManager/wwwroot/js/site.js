/* ========================================
   TOGGLE PASSWORD VISIBILITY
   ======================================== */

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
    	width="24"
	    height="24"
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

// Toggle Sidebar Function
function toggleSidebar() {
    const currentState = localStorage.getItem('sidebarCollapsed') === 'true';
    const newState = !currentState;

    localStorage.setItem('sidebarCollapsed', newState);

    document.documentElement.style.setProperty('--sidebar-state', newState ? 'collapsed' : 'expanded');
}

// Delete Folder Function
function deleteFolder(event, folderId, folderName) {
    event.preventDefault();
    event.stopPropagation();

    if (!confirm(`Are you sure you want to delete the folder "${folderName}"?\n\nAll items in this folder will be moved to "No Folder".`)) {
        return;
    }

    // Create form and submit
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Vault/DeleteFolder';

    // Add anti-forgery token
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) {
        const token = document.createElement('input');
        token.type = 'hidden';
        token.name = '__RequestVerificationToken';
        token.value = tokenInput.value;
        form.appendChild(token);
    }

    // Add folder ID
    const idInput = document.createElement('input');
    idInput.type = 'hidden';
    idInput.name = 'folderId';
    idInput.value = folderId;
    form.appendChild(idInput);

    document.body.appendChild(form);
    form.submit();
}

/* ========================================
   SIDEBAR STATE - NO LONGER NEEDED
   ======================================== */
// Sidebar state is now handled by inline script in _VaultSidebar.cshtml
// and CSS variables. No FOUC possible anymore!

document.addEventListener('DOMContentLoaded', function () {
    initializeFoldersScroll();
    initializeTooltipPositioning();
    checkFoldersOverflow();
});

/* ========================================
   FOLDERS SCROLL MANAGEMENT
   ======================================== */
function initializeFoldersScroll() {
    const foldersScrollable = document.querySelector('.folders-scrollable');

    if (foldersScrollable) {
        const scrollEl = document.getElementById('foldersScroll') || foldersScrollable;
        const key = 'foldersScrollTop';

        const saved = localStorage.getItem(key);
        if (saved !== null) {
            scrollEl.scrollTop = parseInt(saved, 10);
        }

        scrollEl.addEventListener('scroll', () => {
            localStorage.setItem(key, scrollEl.scrollTop);
        });

        // Highlight active folder on scroll
        const activeFolderItem = foldersScrollable.querySelector('.folder-item.active');
        if (activeFolderItem && saved === null) {
            const scrollTop = activeFolderItem.offsetTop - foldersScrollable.offsetTop - 20;
            foldersScrollable.scrollTop = scrollTop;
        }
    }
}

/* ========================================
   FOLDERS OVERFLOW DETECTION
   ======================================== */
function checkFoldersOverflow() {
    const foldersScrollable = document.querySelector('.folders-scrollable');

    if (!foldersScrollable) return;

    const hasOverflow = foldersScrollable.scrollHeight > foldersScrollable.clientHeight;

    if (hasOverflow) {
        foldersScrollable.classList.add('has-overflow');
    } else {
        foldersScrollable.classList.remove('has-overflow');
    }
}

window.addEventListener('resize', checkFoldersOverflow);

/* ========================================
   TOOLTIP POSITIONING
   ======================================== */
function initializeTooltipPositioning() {
    const folderWrappers = document.querySelectorAll('.folder-item-wrapper');

    folderWrappers.forEach(wrapper => {
        const tooltip = wrapper.querySelector('.folder-tooltip');

        if (!tooltip) return;

        wrapper.addEventListener('mouseenter', function () {
            updateTooltipPosition(wrapper, tooltip);
        });
    });
}

function updateTooltipPosition(wrapper, tooltip) {
    const rect = wrapper.getBoundingClientRect();

    // Get the currently visible sidebar (either expanded or collapsed)
    const sidebarExpanded = document.getElementById('sidebar-expanded');
    const sidebarCollapsed = document.getElementById('sidebar-collapsed');
    const sidebar = sidebarExpanded && sidebarExpanded.offsetParent !== null
        ? sidebarExpanded
        : sidebarCollapsed;

    if (!sidebar) return;

    const sidebarRect = sidebar.getBoundingClientRect();

    const left = sidebarRect.right + 12;
    const top = rect.top;

    tooltip.style.left = left + 'px';
    tooltip.style.top = top + 'px';

    setTimeout(() => {
        const tooltipRect = tooltip.getBoundingClientRect();

        if (tooltipRect.bottom > window.innerHeight) {
            const newTop = window.innerHeight - tooltipRect.height - 12;
            tooltip.style.top = Math.max(12, newTop) + 'px';
        }

        if (tooltipRect.right > window.innerWidth) {
            tooltip.style.left = (sidebarRect.left - tooltipRect.width - 12) + 'px';

            tooltip.style.setProperty('--arrow-side', 'right');
        }
    }, 10);
}

// Update folder count in real-time (optional enhancement)
function updateFolderItemCount(folderId, count) {
    const folderLink = document.querySelector(`a[href*="folderId=${folderId}"]`);
    if (folderLink) {
        const countSpan = folderLink.querySelector('.nav-item-count');
        if (countSpan) {
            countSpan.textContent = count;
        }
    }
}

/* ========================================
   PASSWORD GENERATOR FUNCTIONS
   ======================================== */

function openGenerator(event) {
    if (event) event.preventDefault();
    const modal = document.getElementById('generator-modal');
    if (modal) {
        modal.classList.add('show');
        generatePassword(); // Generate password immediately on open
    }
}

function closeGenerator() {
    const modal = document.getElementById('generator-modal');
    if (modal) {
        modal.classList.remove('show');
    }
}

function updateLength(value) {
    const lengthValue = document.getElementById('length-value');
    if (lengthValue) {
        lengthValue.textContent = value;
    }
    generatePassword();
}

function generatePassword() {
    const length = parseInt(document.getElementById('length-slider').value);
    const useUppercase = document.getElementById('uppercase').checked;
    const useLowercase = document.getElementById('lowercase').checked;
    const useNumbers = document.getElementById('numbers').checked;
    const useSymbols = document.getElementById('symbols').checked;

    // Character sets
    const uppercase = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const lowercase = 'abcdefghijklmnopqrstuvwxyz';
    const numbers = '0123456789';
    const symbols = '!@#$%^&*()_+-=[]{}|;:,.<>?';

    let charset = '';
    let password = '';

    // Build character set
    if (useUppercase) charset += uppercase;
    if (useLowercase) charset += lowercase;
    if (useNumbers) charset += numbers;
    if (useSymbols) charset += symbols;

    // If nothing selected, use lowercase
    if (charset === '') {
        charset = lowercase;
    }

    // Generate password
    for (let i = 0; i < length; i++) {
        const randomIndex = Math.floor(Math.random() * charset.length);
        password += charset[randomIndex];
    }

    // Display password
    const passwordDisplay = document.getElementById('generated-password');
    if (passwordDisplay) {
        passwordDisplay.textContent = password;

        // Appearance animation
        passwordDisplay.style.transform = 'scale(0.95)';
        setTimeout(() => {
            passwordDisplay.style.transform = 'scale(1)';
        }, 100);
    }
}

function copyGeneratedPassword() {
    const passwordDisplay = document.getElementById('generated-password');
    if (!passwordDisplay) return;

    const password = passwordDisplay.textContent;

    if (password === 'Click Generate') {
        showNotification('Please generate a password first!', 'warning');
        return;
    }

    navigator.clipboard.writeText(password).then(() => {
        showNotification('Password copied to clipboard!', 'success');

        // Visual feedback
        const copyBtn = document.querySelector('.copy-password-btn');
        if (copyBtn) {
            copyBtn.style.background = '#27ae60';
            copyBtn.style.color = '#ffffff';

            setTimeout(() => {
                copyBtn.style.background = '';
                copyBtn.style.color = '';
            }, 1000);
        }
    }).catch(err => {
        console.error('Copy failed:', err);
        showNotification('Failed to copy password', 'error');
    });
}

function showNotification(message, type = 'success') {
    // Create notification
    const notification = document.createElement('div');
    notification.className = 'notification';

    // Determine color
    let bgColor = '#27ae60'; // success
    if (type === 'error') bgColor = '#e74c3c';
    if (type === 'warning') bgColor = '#f39c12';

    notification.style.cssText = `
        position: fixed;
        bottom: 24px;
        right: 24px;
        background: ${bgColor};
        color: white;
        padding: 16px 24px;
        border-radius: 10px;
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.3);
        z-index: 10000;
        font-weight: 600;
        animation: slideInRight 0.3s ease;
    `;

    notification.textContent = message;
    document.body.appendChild(notification);

    // Remove after 3 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => {
            document.body.removeChild(notification);
        }, 300);
    }, 3000);
}

// Close modal when clicking outside
document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('generator-modal');
    if (modal) {
        modal.addEventListener('click', function (e) {
            if (e.target === modal) {
                closeGenerator();
            }
        });
    }
});

// Animations for notifications
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(400px);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);