// Modal for the safe
/*const Modal_AddSafe = document.querySelector("header #modal.add-safe");*/
const Modal_CloseSafe = document.querySelector("header #modal.close-safe");
const Add_Safe = document.querySelector("header .header_content button.button");
const closeIcon_Safe = document.querySelector(
  "header #modal.close-safe .close_modal i"
);
const closeButton_Safe = document.querySelector(
  "header #modal.close-safe .Buttons .closeSafe"
);

//const checkForSafe = sessionStorage.getItem("Safe");

//if (!checkForSafe) {
//  sessionStorage.setItem("Safe", true);
//  Modal_AddSafe && Modal_AddSafe.classList.add("open");
//}

Add_Safe &&
  Add_Safe.addEventListener("click", () => {
    Modal_CloseSafe && Modal_CloseSafe.classList.add("open");
  });

closeIcon_Safe &&
  closeIcon_Safe.addEventListener("click", () => {
    Modal_CloseSafe && Modal_CloseSafe.classList.remove("open");
  });

closeButton_Safe &&
  closeButton_Safe.addEventListener("click", () => {
    Modal_CloseSafe && Modal_CloseSafe.classList.remove("open");
  });

// Modal for the customer
const Modal_Customer = document.querySelector(".Checkout #modal ");
const Add_Customer = document.querySelector(
  ".addCustomer"
);

const closeIcon_Customer = document.querySelector(
  ".Checkout #modal .close_modal i"
);

const closeButton_Customer = document.querySelector(
  ".Checkout #modal .Buttons #closeModel"
);

Add_Customer &&
  Add_Customer.addEventListener("click", () => {
      Modal_Customer && Modal_Customer.classList.add("open");
      document.querySelector("#CustomerRegisterVM_Phone").value = "";
      document.querySelector("#CustomerRegisterVM_Name").value = "";
      document.querySelector("#CustomerRegisterVM_AnotherPhone").value = "";
      document.querySelector("#CustomerRegisterVM_Address").value = "";
  });

closeIcon_Customer &&
  closeIcon_Customer.addEventListener("click", () => {
    Modal_Customer && Modal_Customer.classList.remove("open");
  });
closeButton_Customer &&
  closeButton_Customer.addEventListener("click", () => {
    Modal_Customer && Modal_Customer.classList.remove("open");
  });

// Modal for the Table
const Modal_Table = document.querySelector(".tableModel ");
const Add_Table = document.querySelector(
    ".chooseTable"
);

const closeIcon_Table = document.querySelector(
    ".tableModel .close_modal i"
);

const closeButton_Table = document.querySelector(
    ".tableModel .Buttons button[type=button]"
);

Add_Table &&
    Add_Table.addEventListener("click", () => {
        Modal_Table && Modal_Table.classList.add("open");
        const checkboxes = document.querySelectorAll('.Tables_content input[type="checkbox"]');
        checkboxes.forEach((checkbox) => {
            checkbox.checked = false;
        });
    });

closeIcon_Table &&
    closeIcon_Table.addEventListener("click", () => {
        Modal_Table && Modal_Table.classList.remove("open");
    });
closeButton_Table &&
    closeButton_Table.addEventListener("click", () => {
        Modal_Table && Modal_Table.classList.remove("open");
    });

// Add a new delivery
const Add_Delivery = document.querySelector(".search button");
const Modal_Delivery = document.querySelector("#modal");
const closeIcon_Delivery = document.querySelector("#modal .close_modal");
const closeButton_Delivery = document.querySelector(
  "#modal .Buttons button[type=button]"
);

Add_Delivery &&
  Add_Delivery.addEventListener("click", () => {
    Modal_Delivery && Modal_Delivery.classList.add("open");
  });

closeIcon_Delivery &&
  closeIcon_Delivery.addEventListener("click", () => {
    Modal_Delivery && Modal_Delivery.classList.remove("open");
  });

closeButton_Delivery &&
  closeButton_Delivery.addEventListener("click", () => {
    Modal_Delivery && Modal_Delivery.classList.remove("open");
  });

// choose delivery in checkout
const button_delivery = document.querySelector(
  ".Checkout .List button#delivery"
);
const company_delivery = document.querySelector(".Checkout .company-delivery");

button_delivery &&
  button_delivery.addEventListener("click", () => {
    company_delivery && company_delivery.classList.toggle("open");
  });

// Modal the reactionary => مرتجع المبيعات
const Modal_AddReactionary = document.querySelector(
  "header #modal.add-reactionary"
);
const Add_reactionary = document.querySelector(
  "header .header_content button#reactionary"
);

Add_reactionary &&
  Add_reactionary.addEventListener("click", () => {
    Modal_AddReactionary && Modal_AddReactionary.classList.add("open");
  });

const headen = document.querySelectorAll(
  "header #modal.add-reactionary .reactionary-button"
);

const buttons = document.querySelectorAll(
  "header #modal.add-reactionary .reactionary-button button"
);

const reactionary_button = document.querySelectorAll(
  "header #modal.add-reactionary .reactionary-button"
);

for (let i = 0; i < buttons.length; i++) {
  buttons[i].addEventListener("click", (e) => {
    let parentElement = e.target.parentElement;
    parentElement.classList.toggle("active");

    let desc = parentElement.nextElementSibling;

    if (desc.style.maxHeight) {
      desc.style.maxHeight = null;
      desc.style.overflow = "hidden";
    } else {
      desc.style.overflow = "auto";
      desc.style.maxHeight = desc.scrollHeight + "px";
    }
  });
}

// Add new item in invoice
const add_newItem = document.querySelector(
  "header #modal.add-reactionary #new-reactionary"
);

add_newItem &&
  add_newItem.addEventListener("click", (e) => {
    if (e.target.innerHTML === "الغاء") {
      e.target.innerHTML = "اضافة";
      e.target.style = "background: var(--color-primary)";
      document.querySelector(
        "header #modal.add-reactionary .form-reactionary"
      ).style = `display: none`;
      document.querySelector(
        "header #modal.add-reactionary .reactionary_wapper"
      ).style = `display: flex`;
    } else {
      e.target.innerHTML = "الغاء";
      e.target.style = "background: brown";
      document.querySelector(
        "header #modal.add-reactionary .form-reactionary"
      ).style = `display: flex`;
      document.querySelector(
        "header #modal.add-reactionary .reactionary_wapper"
      ).style = `display: none`;
    }
  });
