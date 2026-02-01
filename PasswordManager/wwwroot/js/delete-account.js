document.addEventListener('DOMContentLoaded', function () {
    const deleteBtn = document.getElementById('deleteBtn');
    const deleteBtnText = document.getElementById('deleteBtnText');
    const countdownElement = document.getElementById('countdown');
    const countdownNotice = document.getElementById('countdownNotice');

    let timeLeft = 5;

    const countdown = setInterval(function () {
        timeLeft--;

        if (countdownElement && deleteBtnText) {
            countdownElement.textContent = timeLeft;
            deleteBtnText.textContent = `Delete Account (${timeLeft})`;
        }

        if (timeLeft <= 0) {
            clearInterval(countdown);

            if (deleteBtn) {
                deleteBtn.disabled = false;
            }

            if (deleteBtnText) {
                deleteBtnText.innerHTML = 'Delete Account';
            }

            if (countdownNotice) {
                countdownNotice.classList.add('hidden');
            }
        }
    }, 1000);
});