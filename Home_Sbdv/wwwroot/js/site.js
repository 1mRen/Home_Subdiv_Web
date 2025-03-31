// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


const modeToggleBtn = document.getElementById('modeToggleBtn');
const modeIcon = document.getElementById('modeIcon');
const THEME_KEY = 'theme';

// Initial theme setup
const savedMode = localStorage.getItem(THEME_KEY);
if (savedMode === 'night') {
  document.body.classList.add('night-mode');
  modeIcon.classList.replace('bi-sun', 'bi-moon-fill');
} else {
  modeIcon.classList.replace('bi-moon', 'bi-sun-fill');
}


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

  // Optional: Add a small animation to the button
  modeToggleBtn.classList.add('rotate-animation');
  setTimeout(() => {
    modeToggleBtn.classList.remove('rotate-animation');
  }, 500);
});




