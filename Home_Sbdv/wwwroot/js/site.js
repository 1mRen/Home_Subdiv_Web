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

// Notification Dropdown Logic
$(document).ready(function() {
    function loadNotifications() {
        $('#notificationList').html('<div class="text-center py-3">Loading...</div>');
        $.get('/Notification/GetNotifications', function(data) {
            var notifications = data.notifications;
            var notificationList = $('#notificationList');
            notificationList.empty();

            if (!notifications || notifications.length === 0) {
                notificationList.append('<div class="text-center py-3">No notifications</div>');
                return;
            }

            notifications.forEach(function(notification) {
                var notificationHtml = `
                    <div class="notification-item p-2 border-bottom ${notification.isRead ? '' : 'bg-light'}" data-id="${notification.id}">
                        <div class="d-flex justify-content-between align-items-start">
                            <div>
                                <p class="mb-1">${notification.message}</p>
                                <small class="text-muted">${new Date(notification.createdAt).toLocaleString()}</small>
                            </div>
                            <button class="btn btn-sm btn-link mark-read" data-id="${notification.id}">
                                <i class="bi bi-check2"></i>
                            </button>
                        </div>
                    </div>
                `;
                notificationList.append(notificationHtml);
            });
        });
    }

    function updateUnreadCount() {
        $.get('/Notification/GetUnreadCount', function(count) {
            $('#notificationBadge').text(count);
            if (count > 0) {
                $('#notificationBadge').show();
            } else {
                $('#notificationBadge').hide();
            }
        });
    }

    // Load notifications when the dropdown is shown
    $('#notificationDropdown').on('show.bs.dropdown', function () {
        loadNotifications();
        updateUnreadCount();
    });

    // Mark single notification as read
    $(document).on('click', '.mark-read', function() {
        var notificationId = $(this).data('id');
        $.post('/Notification/MarkAsRead', { id: notificationId }, function() {
            loadNotifications();
            updateUnreadCount();
        });
    });

    // Mark all notifications as read
    $('#markAllRead').click(function() {
        $.post('/Notification/MarkAllAsRead', function() {
            loadNotifications();
            updateUnreadCount();
        });
    });

    // Optionally, keep the interval refresh if you want
    setInterval(function() {
        loadNotifications();
        updateUnreadCount();
    }, 30000);
});





