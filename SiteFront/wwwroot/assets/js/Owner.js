const modal = document.querySelector("#modal");
const btn_add = document.querySelector(".search button");
const icon_close = document.querySelector("#modal .close_modal i");
const button_close = document.querySelector(
  "#modal .Buttons button[type=button]"
);

btn_add.addEventListener("click", (e) => {
  modal.classList.add("open");
});

button_close.addEventListener("click", () => {
  modal.classList.remove("open");
});

icon_close.addEventListener("click", () => {
  modal.classList.remove("open");
});
