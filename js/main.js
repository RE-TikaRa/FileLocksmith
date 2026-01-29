document.addEventListener('DOMContentLoaded', () => {
  const items = Array.from(document.querySelectorAll('[data-reveal]'));
  if (items.length) {
    if (!('IntersectionObserver' in window)) {
      items.forEach((el) => el.classList.add('is-visible'));
    } else {
      const observer = new IntersectionObserver(
        (entries, obs) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting) {
              entry.target.classList.add('is-visible');
              obs.unobserve(entry.target);
            }
          });
        },
        { threshold: 0.18, rootMargin: '0px 0px -10% 0px' }
      );
      items.forEach((el) => observer.observe(el));
    }
  }

  const root = document.documentElement;
  const toggle = document.getElementById('theme-toggle');
  if (!toggle) {
    return;
  }
  const label = toggle.querySelector('.theme-label');
  const media = window.matchMedia('(prefers-color-scheme: dark)');
  const getSystemTheme = () => (media.matches ? 'dark' : 'light');
  const updateButton = (mode, resolved) => {
    toggle.dataset.mode = mode;
    if (label) {
      label.textContent = mode === 'system' ? '自动' : mode === 'dark' ? '深色' : '浅色';
      toggle.setAttribute('aria-label', `主题：${label.textContent}`);
    }
    root.dataset.resolvedTheme = resolved;
  };
  const applyMode = (mode, persist) => {
    if (mode === 'system') {
      root.removeAttribute('data-theme');
      if (persist) {
        localStorage.removeItem('theme-mode');
      }
    } else {
      root.dataset.theme = mode;
      if (persist) {
        localStorage.setItem('theme-mode', mode);
      }
    }
    const resolved = root.dataset.theme || getSystemTheme();
    updateButton(mode, resolved);
  };

  const stored = localStorage.getItem('theme-mode');
  applyMode(stored || 'system', false);

  media.addEventListener('change', () => {
    if (!localStorage.getItem('theme-mode')) {
      applyMode('system', false);
    }
  });

  toggle.addEventListener('click', () => {
    const current = toggle.dataset.mode || 'system';
    const next = current === 'system' ? 'dark' : current === 'dark' ? 'light' : 'system';
    applyMode(next, true);
  });
});
