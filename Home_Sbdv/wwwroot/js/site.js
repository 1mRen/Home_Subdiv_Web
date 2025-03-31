// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', () => {
  const modeToggleBtn = document.getElementById('modeToggleBtn');
  const modeIcon = document.getElementById('modeIcon');
  const THEME_KEY = 'theme';

  // Ensure localStorage is supported
  if (!window.localStorage) {
    console.warn('LocalStorage is not supported in this browser.');
    return;
  }

  // Check stored preference
  let savedMode = localStorage.getItem(THEME_KEY);

  // If no stored preference, default to light mode
  if (!savedMode) {
    localStorage.setItem(THEME_KEY, 'light');
    savedMode = 'light';
  }

  // Apply the theme
  if (savedMode === 'night') {
    document.body.classList.add('night-mode');
    modeIcon.classList.replace('bi-sun-fill', 'bi-moon-fill');
  } else {
    document.body.classList.remove('night-mode');
    modeIcon.classList.replace('bi-moon-fill', 'bi-sun-fill');
  }

  // Toggle mode on click
  modeToggleBtn.addEventListener('click', () => {
    document.body.style.transition = 'background-color 0.5s ease, color 0.5s ease';
    document.body.classList.toggle('night-mode');

    if (document.body.classList.contains('night-mode')) {
      modeIcon.classList.replace('bi-sun-fill', 'bi-moon-fill');
      localStorage.setItem(THEME_KEY, 'night');
    } else {
      modeIcon.classList.replace('bi-moon-fill', 'bi-sun-fill');
      localStorage.setItem(THEME_KEY, 'light');
    }

    modeToggleBtn.classList.add('rotate-animation');
    setTimeout(() => {
      modeToggleBtn.classList.remove('rotate-animation');
    }, 500);
  });
});





