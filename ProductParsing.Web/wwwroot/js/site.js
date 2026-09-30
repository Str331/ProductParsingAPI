document.addEventListener('submit', (event) => {
  const form = event.target;
  if (form instanceof HTMLFormElement && form.dataset.confirm && !window.confirm(form.dataset.confirm)) {
    event.preventDefault();
  }
}, true);

document.addEventListener('submit', (event) => {
  const form = event.target;
  if (event.defaultPrevented || !(form instanceof HTMLFormElement) || !form.hasAttribute('data-busy-on-submit') || !form.checkValidity()) {
    return;
  }
  const button = form.querySelector('button[type="submit"]');
  if (button) {
    button.disabled = true;
    button.querySelector('.spinner-border')?.classList.remove('d-none');
  }
});

document.addEventListener('click', (event) => {
  const toggle = event.target instanceof Element ? event.target.closest('[data-description-toggle]') : null;
  if (!toggle) {
    return;
  }
  const description = document.getElementById(toggle.getAttribute('aria-controls'));
  const expanded = description.classList.toggle('is-expanded');
  toggle.setAttribute('aria-expanded', String(expanded));
  toggle.textContent = expanded ? 'Згорнути' : 'Показати повністю';
});

document.addEventListener('DOMContentLoaded', () => {
  document.querySelectorAll('[data-description-toggle]').forEach((toggle) => {
    const description = document.getElementById(toggle.getAttribute('aria-controls'));
    if (description && description.scrollHeight <= description.clientHeight + 1) {
      toggle.hidden = true;
    }
  });
});
