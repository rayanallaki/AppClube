(() => {
  'use strict';
  const toggle = document.getElementById('menu-toggle');
  const closeMenu = () => {
    document.body.classList.remove('menu-open');
    if (toggle) toggle.setAttribute('aria-expanded', 'false');
  };
  if (toggle) toggle.addEventListener('click', () => {
    const open = document.body.classList.toggle('menu-open');
    toggle.setAttribute('aria-expanded', String(open));
  });
  document.addEventListener('click', event => {
    if (!document.body.classList.contains('menu-open')) return;
    if (!event.target.closest('.sidebar') && !event.target.closest('#menu-toggle')) closeMenu();
  });
  document.addEventListener('keydown', event => {
    if (event.key === 'Escape') closeMenu();
  });
  document.addEventListener('submit', event => {
    const form = event.target;
    if (!(form instanceof HTMLFormElement)) return;
    const message = event.submitter?.dataset.confirm || form.dataset.confirm;
    if (message && !window.confirm(message)) event.preventDefault();
  });
  document.querySelectorAll('[data-table-search]').forEach(input => {
    input.addEventListener('input', () => {
      const table = document.getElementById(input.dataset.tableSearch);
      if (!table) return;
      const query = input.value.trim().toLocaleLowerCase('pt-BR');
      table.querySelectorAll('tbody tr').forEach(row => {
        row.hidden = !row.textContent.toLocaleLowerCase('pt-BR').includes(query);
      });
    });
  });
})();
