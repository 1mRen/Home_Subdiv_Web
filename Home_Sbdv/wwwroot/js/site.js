// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


const modeToggleBtn = document.getElementById('modeToggleBtn');
const modeIcon = document.getElementById('modeIcon');

// Check for saved mode preference
const savedMode = localStorage.getItem('theme');
if (savedMode === 'night') {
  document.body.classList.add('night-mode');
  modeIcon.classList.replace('bi-sun', 'bi-moon');
}

modeToggleBtn.addEventListener('click', () => {
  document.body.classList.toggle('night-mode');

  if (document.body.classList.contains('night-mode')) {
    modeIcon.classList.replace('bi-sun', 'bi-moon');
    localStorage.setItem('theme', 'night');
  } else {
    modeIcon.classList.replace('bi-moon', 'bi-sun');
    localStorage.setItem('theme', 'light');
  }
});

