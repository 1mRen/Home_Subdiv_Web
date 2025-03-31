document.addEventListener('DOMContentLoaded', function () {
  const messageAlert = document.getElementById('message-alert');
  if (messageAlert) {
    // Create and add the loading bar
    const loadingBarContainer = document.createElement('div');
    loadingBarContainer.className = 'loading-bar-container';

    const loadingBar = document.createElement('div');
    loadingBar.className = 'loading-bar';
    loadingBarContainer.appendChild(loadingBar);
    messageAlert.appendChild(loadingBarContainer);

    // Set the initial width to 100%
    loadingBar.style.width = '100%';

    // Set the timeout duration (in milliseconds)
    const timeoutDuration = 5000;

    // Animate the loading bar
    loadingBar.style.transition = `width ${timeoutDuration}ms linear`;

    // Start the animation after a small delay to ensure it's properly initialized
    setTimeout(() => {
      loadingBar.style.width = '0%';
    }, 50);

    // Auto close after timeoutDuration
    const timeout = setTimeout(function () {
      messageAlert.classList.add('fade-out');
      setTimeout(function () {
        messageAlert.style.display = 'none';
      }, 300);
    }, timeoutDuration);

    // Manual close button functionality
    const closeButton = messageAlert.querySelector('.close-button');
    if (closeButton) {
      closeButton.addEventListener('click', function () {
        clearTimeout(timeout);
        messageAlert.classList.add('fade-out');
        setTimeout(function () {
          messageAlert.style.display = 'none';
        }, 300);
      });
    }
  }
});


// toogle password
document.addEventListener('DOMContentLoaded', function () {
  const toggleButtons = document.querySelectorAll('.password-toggle-btn');

  toggleButtons.forEach(button => {
    button.addEventListener('click', function () {
      const input = this.closest('.password-input-wrapper').querySelector('input');
      const icon = this.querySelector('i');

      // Toggle type between password and text
      if (input.type === 'password') {
        input.type = 'text';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
      } else {
        input.type = 'password';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
      }
    });
  });
});