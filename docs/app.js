// SPDX-License-Identifier: Apache-2.0
const header = document.querySelector('[data-header]');
const progress = document.querySelector('.scroll-progress');
const navLinks = Array.from(document.querySelectorAll('.nav-links a'));
const revealItems = Array.from(document.querySelectorAll('.reveal'));

const updateChrome = () => {
  const top = window.scrollY || 0;
  const max = Math.max(1, document.documentElement.scrollHeight - window.innerHeight);
  if (header) header.classList.toggle('is-compact', top > 22);
  if (progress) progress.style.width = `${Math.min(100, (top / max) * 100)}%`;
};

const revealObserver = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.classList.add('is-visible');
      revealObserver.unobserve(entry.target);
    }
  });
}, { threshold: 0.14 });

revealItems.forEach((item) => revealObserver.observe(item));

const sections = navLinks
  .map((link) => document.querySelector(link.getAttribute('href')))
  .filter(Boolean);

const navObserver = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (!entry.isIntersecting) return;
    navLinks.forEach((link) => {
      link.classList.toggle('is-active', link.getAttribute('href') === `#${entry.target.id}`);
    });
  });
}, { rootMargin: '-32% 0px -58% 0px', threshold: 0.01 });

sections.forEach((section) => navObserver.observe(section));
window.addEventListener('scroll', updateChrome, { passive: true });
window.addEventListener('resize', updateChrome, { passive: true });
updateChrome();
