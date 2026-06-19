// public.js — theme toggle, mobile nav, stat counters, theses client-side filter.
(function () {
  "use strict";

  document.addEventListener("click", function (e) {
    if (e.target.closest(".theme-toggle")) {
      var cur = document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light";
      var next = cur === "dark" ? "light" : "dark";
      document.documentElement.setAttribute("data-theme", next);
      try { localStorage.setItem("theme", next); } catch (e2) {}
      return;
    }
    if (e.target.closest(".nav-burger")) {
      document.querySelector(".nav-menu")?.classList.toggle("open");
      return;
    }
    var tab = e.target.closest(".filter-tab");
    if (tab) {
      var cat = tab.getAttribute("data-cat");
      document.querySelectorAll(".filter-tab").forEach(function (b) { b.classList.remove("active"); });
      tab.classList.add("active");
      document.querySelectorAll("[data-thesis-row]").forEach(function (row) {
        row.style.display = (cat === "all" || row.getAttribute("data-cat") === cat) ? "" : "none";
      });
    }
  });

  // Animated counters
  function animate(node) {
    var target = parseInt(node.getAttribute("data-target"), 10) || 0;
    var suffix = node.getAttribute("data-suffix") || "";
    var start = null, dur = 1400;
    function step(ts) {
      if (start === null) start = ts;
      var p = Math.min((ts - start) / dur, 1);
      node.textContent = Math.round((1 - Math.pow(1 - p, 3)) * target) + suffix;
      if (p < 1) requestAnimationFrame(step);
    }
    requestAnimationFrame(step);
  }
  var nums = document.querySelectorAll(".stat-num[data-target]");
  if (nums.length && "IntersectionObserver" in window) {
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (en) { if (en.isIntersecting) { animate(en.target); io.unobserve(en.target); } });
    }, { threshold: 0.4 });
    nums.forEach(function (n) { io.observe(n); });
  } else {
    nums.forEach(function (n) { n.textContent = (n.getAttribute("data-target") || "") + (n.getAttribute("data-suffix") || ""); });
  }
})();
