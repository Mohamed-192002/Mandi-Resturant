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