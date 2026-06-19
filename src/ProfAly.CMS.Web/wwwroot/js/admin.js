// admin.js — theme toggle + responsive sidebar for the admin shell.
(function () {
  "use strict";

  // Theme is applied pre-paint by an inline script in the layout <head>.
  function toggleTheme() {
    var current = document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light";
    var next = current === "dark" ? "light" : "dark";
    document.documentElement.setAttribute("data-theme", next);
    try { localStorage.setItem("theme", next); } catch (e) { /* ignore */ }
  }

  document.addEventListener("click", function (e) {
    if (e.target.closest(".theme-toggle")) { toggleTheme(); return; }
    if (e.target.closest(".nav-burger")) { document.querySelector(".admin-shell")?.classList.toggle("sidebar-open"); return; }
    if (e.target.closest(".sidebar-backdrop")) { document.querySelector(".admin-shell")?.classList.remove("sidebar-open"); }
  });
})();
