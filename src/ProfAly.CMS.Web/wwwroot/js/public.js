// public.js — theme toggle, mobile nav, stat counters, theses client-side filter.
(function () {
  "use strict";

  var burger = document.querySelector(".nav-burger");
  var menu = document.querySelector(".nav-menu");
  var siteNav = document.querySelector(".site-nav");
  var navInner = document.querySelector(".nav-inner");

  function setMenu(open) {
    if (!menu || !burger) return;
    menu.classList.toggle("open", open);
    burger.setAttribute("aria-expanded", open ? "true" : "false");
  }

  // Responsive header: above the CSS breakpoint the nav is a single horizontal row.
  // If the localized labels are too wide to fit that row (e.g. the longer French
  // strings), collapse to the same hamburger drawer instead of letting items overlap
  // the brand or language switcher. Language-agnostic: it measures the actual fit, so
  // it adapts automatically to the selected language and text length.
  var desktopNav = window.matchMedia("(min-width: 1440px)");
  function syncNavFit() {
    if (!siteNav || !navInner) return;
    // Measure the natural single-line fit with our own class removed, then decide.
    var wasCollapsed = siteNav.classList.contains("nav-collapsed");
    siteNav.classList.remove("nav-collapsed");
    // The nav sits in the centre grid column; when its labels are too wide the whole
    // header row overflows its container. scrollWidth > clientWidth (by >1px) means the
    // three sections cannot share one line. Only meaningful on the desktop layout.
    var overflowing = desktopNav.matches && navInner.scrollWidth - navInner.clientWidth > 1;
    siteNav.classList.toggle("nav-collapsed", overflowing);
    // Leaving the collapsed state? Make sure the drawer isn't left open.
    if (wasCollapsed && !overflowing) setMenu(false);
  }
  syncNavFit();
  window.addEventListener("resize", syncNavFit, { passive: true });
  window.addEventListener("load", syncNavFit); // re-check once web fonts have settled

  document.addEventListener("click", function (e) {
    if (e.target.closest(".theme-toggle")) {
      var cur = document.documentElement.getAttribute("data-theme") === "dark" ? "dark" : "light";
      var next = cur === "dark" ? "light" : "dark";
      document.documentElement.setAttribute("data-theme", next);
      try { localStorage.setItem("theme", next); } catch (e2) {}
      return;
    }
    if (e.target.closest(".nav-burger")) {
      setMenu(!(menu && menu.classList.contains("open")));
      return;
    }
    // Digital Resources dropdown: toggle on the button (desktop click + keyboard).
    var ddToggle = e.target.closest(".nav-dropdown-toggle");
    if (ddToggle) {
      var dd = ddToggle.closest(".nav-dropdown");
      var willOpen = dd && !dd.classList.contains("open");
      document.querySelectorAll(".nav-dropdown.open").forEach(function (d) {
        d.classList.remove("open");
        var t = d.querySelector(".nav-dropdown-toggle");
        if (t) t.setAttribute("aria-expanded", "false");
      });
      if (dd) {
        dd.classList.toggle("open", willOpen);
        ddToggle.setAttribute("aria-expanded", willOpen ? "true" : "false");
      }
      return;
    }
    // Click outside an open dropdown closes it.
    if (!e.target.closest(".nav-dropdown")) {
      document.querySelectorAll(".nav-dropdown.open").forEach(function (d) {
        d.classList.remove("open");
        var t = d.querySelector(".nav-dropdown-toggle");
        if (t) t.setAttribute("aria-expanded", "false");
      });
    }
    // Close the mobile menu when a link inside it is followed, or when clicking outside.
    if (menu && menu.classList.contains("open")) {
      if (e.target.closest(".nav-menu a")) { setMenu(false); }
      else if (!e.target.closest(".nav-menu") && !e.target.closest(".nav-burger")) { setMenu(false); }
    }
    if (e.target.closest(".back-to-top")) {
      window.scrollTo({ top: 0, behavior: "smooth" });
      return;
    }
    // Event video facade: replace the thumbnail with the embedded YouTube player.
    var facade = e.target.closest(".video-facade");
    if (facade && facade.getAttribute("data-embed")) {
      var frame = document.createElement("iframe");
      var sep = facade.getAttribute("data-embed").indexOf("?") === -1 ? "?" : "&";
      frame.src = facade.getAttribute("data-embed") + sep + "autoplay=1";
      frame.title = facade.getAttribute("aria-label") || "";
      frame.setAttribute("allow", "accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share");
      frame.setAttribute("referrerpolicy", "strict-origin-when-cross-origin");
      frame.setAttribute("allowfullscreen", "");
      facade.innerHTML = "";
      facade.appendChild(frame);
      facade.classList.remove("video-facade");
      facade.removeAttribute("role");
      facade.removeAttribute("tabindex");
      facade.removeAttribute("aria-label");
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

  // Close mobile menu / open dropdowns on Escape (return focus to the relevant toggle).
  document.addEventListener("keydown", function (e) {
    // Activate the video facade with Enter/Space (it exposes role="button").
    if ((e.key === "Enter" || e.key === " ") && e.target.classList && e.target.classList.contains("video-facade")) {
      e.preventDefault();
      e.target.click();
      return;
    }
    if (e.key !== "Escape") return;
    var openDd = document.querySelector(".nav-dropdown.open");
    if (openDd) {
      openDd.classList.remove("open");
      var t = openDd.querySelector(".nav-dropdown-toggle");
      if (t) { t.setAttribute("aria-expanded", "false"); t.focus(); }
      return;
    }
    if (menu && menu.classList.contains("open")) {
      setMenu(false);
      if (burger) burger.focus();
    }
  });

  // Header elevation on scroll + back-to-top visibility (shown after scrolling down).
  var nav = document.querySelector(".site-nav");
  var toTop = document.querySelector(".back-to-top");
  if (nav || toTop) {
    var onScroll = function () {
      var y = window.scrollY;
      if (nav) nav.classList.toggle("scrolled", y > 8);
      if (toTop) toTop.classList.toggle("show", y > 500);
    };
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
  }

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
