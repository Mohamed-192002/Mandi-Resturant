const Drawer = document.querySelector("#Drawer");
const close_Drawer = document.querySelector("#Drawer .close i");
const btn_Drawer = document.querySelector("header .fa-solid.fa-list.icon");

btn_Drawer &&
  btn_Drawer.addEventListener("click", () => {
    Drawer && Drawer.classList.toggle("open");
  });

close_Drawer &&
  close_Drawer.addEventListener("click", () => {
    Drawer && Drawer.classList.remove("open");
  });

// Toggle Sidebar in mobile
const Sidebar = document.querySelector(".Sidebar");
const Menu_Sidebar = document.querySelector(
  ".fa-solid.fa-bars-staggered.menu-slider"
);
const close_Sidebar = document.querySelector(
  ".Sidebar .Sidebar_content .close_sidebar"
);

Menu_Sidebar &&
  Menu_Sidebar.addEventListener("click", (e) => {
    Sidebar.classList.toggle("toggle");
    Sidebar && Sidebar.classList.add("open");
  });

close_Sidebar &&
  close_Sidebar.addEventListener("click", () => {
    Sidebar && Sidebar.classList.remove("open");
  });

document.addEventListener("keydown", (e) => {
  if (e.key === "Escape") {
    Sidebar && Sidebar.classList.remove("open");
  }
});

// Loading Pages

window.onload = () => {
  const loader = document.querySelector(".loader");

  loader && loader.classList.add("loader-hidden");

  loader &&
    loader.addEventListener("transitionend", () => {
      loader && document.body.removeChild(loader);
    });
};

// Global Search Functionality
// Global Search Functionality
document.addEventListener('keyup', function (e) {
  if (e.target && e.target.id === 'searchInput') {
    const value = e.target.value.toLowerCase();
    const rows = document.querySelectorAll("table tbody tr");
    rows.forEach(row => {
      const text = row.textContent.toLowerCase();
      row.style.display = text.indexOf(value) > -1 ? "" : "none";
    });
  }
});
