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

// Add new item in invoice - Refactored into a reusable function
window.initEditBillModalEvents = function () {
    // 1. Bill Expansion Logic (Accordion)
    // defined globally or scoped to a container if passed, defaulting to document
    const buttons = document.querySelectorAll(
        "header #modal.add-reactionary .reactionary-button button"
    );

    // We can use jQuery for easier event delegation and management if available, otherwise vanilla JS
    // Given the project uses jQuery mixed with vanilla, let's stick to the vanilla style ensuring we don't duplicate events
    // But since the previous fix used jQuery for reliability, let's allow this function to use jQuery if available (which it is)
    if (typeof $ !== 'undefined') {
        // jQuery version for robustness with dynamic content

        // 1. Accordion
        $(document).off('click', '.reactionary-button button').on('click', '.reactionary-button button', function (e) {
            e.preventDefault();
            // Stop propagation if needed, but not specified in original
            let parentElement = $(this).closest('.reactionary-button');
            parentElement.toggleClass("active");

            let desc = parentElement.next('.reactionary-decs');

            // Toggle logic
            if (desc.length > 0) {
                let el = desc[0];
                if (el.style.maxHeight) {
                    el.style.maxHeight = null;
                    el.style.overflow = "hidden";
                } else {
                    el.style.overflow = "auto";
                    el.style.maxHeight = el.scrollHeight + "px";
                }
            }
        });

        // 2. Add Button Logic
        $(document).off('click', '#new-reactionary').on('click', '#new-reactionary', function (e) {
            let target = $(this);
            if (target.text() === "الغاء") {
                target.text("اضافة");
                target.css("background", "var(--color-primary)");
                $(".form-reactionary").hide();
                $(".reactionary_wapper").css("display", "flex");
            } else {
                target.text("الغاء");
                target.css("background", "brown");
                $(".form-reactionary").css("display", "flex");
                $(".reactionary_wapper").hide();
            }
        });

        // 3. Print Button Logic
        $(document).off('click', '.reactionary .print').on('click', '.reactionary .print', async function (e) {
            e.preventDefault();
            e.stopPropagation();

            // Show Loader
            $(".loader").fadeIn();

            const $reactionaryButton = $(this).closest(".reactionary-button");
            const saleBillId = $reactionaryButton.find(".saleBillId").val();

            const $reactionaryDecs = $reactionaryButton.next(".reactionary-decs");
            // Collect items from the modal (in case they were edited)
            const items = $reactionaryDecs.find(".item").slice(1).map(function () {
                const $item = $(this);

                const categoryId = $item.find(".categoryId").val();
                const productId = $item.find(".productId").val();
                const pName = $item.find(".productName").text().trim();
                const price = parseFloat($item.find(".price").text().trim()) || 0;
                const amount = parseFloat($item.find(".amount").val().trim()) || 0;
                const discount = parseFloat($item.find(".discount").val().trim()) || 0;
                const totalPrice = parseFloat($item.find(".total").text().trim()) || 0;

                return {
                    categoryId,
                    productId,
                    pName,
                    price,
                    amount,
                    discount,
                    totalPrice,
                };
            }).get();

            const SaleBillRefundVM = {
                id: saleBillId,
                billDetailRegisterVM: items,
            };

            // Check availability using the helper function
            const result = await checkHolesAmount(items);
            if (!result) {
                $(".loader").fadeOut(); // Hide loader if check fails
                return;
            }

            // Update/Save and Print (backend handles printing during update if configured)
            $.ajax({
                url: '/Cashier/BillSafary/UpdateSaleSafary',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(SaleBillRefundVM),
                success: function (response) {
                    if (typeof toastr !== 'undefined') toastr.success("تم تعديل الفاتورة بنجاح");
                    setTimeout(function () {
                        window.location.reload();
                    }, 500);
                    // Make sure loader stays until reload or hide it if we want
                    // $(".loader").fadeOut(); // Usually we want to keep it until reload
                },
                error: function (xhr, status, error) {
                    $(".loader").fadeOut(); // Hide loader on error
                    if (typeof toastr !== 'undefined') toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
                    setTimeout(function () {
                        window.location.reload();
                    }, 500);
                }
            });
        });
        // 9. Close Modal Logic
        $(document).off('click', '#modal.add-reactionary .close_modal, #modal.add-reactionary .close_modal i').on('click', '#modal.add-reactionary .close_modal, #modal.add-reactionary .close_modal i', function () {
            const $modal = $(this).closest('#modal.add-reactionary');
            $modal.removeClass('open');

            // If it's the dynamic modal wrapper in ResturantDelivery/DeliveryBill, hide it too
            const $wrapper = $modal.closest('#editBillModal');
            if ($wrapper.length > 0) {
                $wrapper.hide();
                $wrapper.empty(); // Optional: Clear content so it reloads fresh next time
            }
        });

    } else {
        // Fallback or original vanilla implementation if someone prefers
        // For this task, we strongly prefer the jQuery delegation to handle dynamic content automatically
        // without needing to re-attach events every time (delegation on document).
        // However, to be safe with the specific requirements of the user who asked for "Clean Code",
        // Delegated events on 'document' is the cleanest way to handle dynamic content.
    }
};

// Initialize on load
$(document).ready(function () {
    window.initEditBillModalEvents();
});

// Bill Search Functions
// 4. Form Logic (Delete Item)
$(document).off('click', '.reactionary-decs .delete-btn').on('click', '.reactionary-decs .delete-btn', function () {
    $(this).closest(".item").remove();
});

// 5. Form Logic (Recalculate on Input in List)
$(document).off('input', '.reactionary-decs .amount, .reactionary-decs .discount').on('input', '.reactionary-decs .amount, .reactionary-decs .discount', function () {
    const $item = $(this).closest(".item");
    const $price = $item.find(".price");
    const $amount = $item.find(".amount");
    const $discount = $item.find(".discount");
    const $total = $item.find(".total");

    const updateTotal = () => {
        const price = parseFloat($price.text()) || 0;
        const amount = parseFloat($amount.val()) || 0;
        const discount = parseFloat($discount.val()) || 0;

        const total = amount * price - discount;
        $total.text(total.toFixed(2));
    };

    updateTotal();
});

// 6. Form Logic (Get Product Price)
$(document).off('change', '.form-reactionary form .productId').on('change', '.form-reactionary form .productId', function () {
    var productId = $(this).val();
    var $form = $(this).closest("form");
    var $price = $form.find(".price");
    var $categoryId = $form.find(".categoryId");
    var $total = $form.find(".total");
    $.ajax({
        type: 'GET',
        url: '/Cashier/SaleBill/getProductPrice?productId=' + productId,
        success: function (data) {
            //console.log(data);
            $price.val(data.price);
            $total.val(data.price);
            $categoryId.val(data.categoryId);
        },
        error: function () {
            // Handle error
        }
    });
});

// 7. Form Logic (Recalculate on Input in Form)
$(document).off('input', '.form-reactionary form .amount, .form-reactionary form .discount').on('input', '.form-reactionary form .amount, .form-reactionary form .discount', function () {
    var $form = $(this).closest("form");
    var $price = $form.find(".price");
    var $amount = $form.find(".amount");
    var $discount = $form.find(".discount");
    var $total = $form.find(".total");

    var price = parseFloat($price.val()) || 0;
    var amount = parseFloat($amount.val()) || 0;
    var discount = parseFloat($discount.val()) || 0;
    var total = (price * amount) - discount;

    total = total < 0 ? 0 : total;
    $total.val(total.toFixed(2));
});

// 8. Form Logic (Add Button Click)
$(document).off('click', '.form-reactionary form button').on('click', '.form-reactionary form button', function (e) {
    e.preventDefault(); // Prevent submit
    var $form = $(this).closest("form");
    var $billId = $form.find(".billId").val();
    var $productId = $form.find(".productId").val();
    var $productName = $form.find(".productId option:selected").text();
    var $categoryId = $form.find(".categoryId").val();
    var $price = $form.find(".price").val();
    var $amount = parseFloat($form.find(".amount").val());
    var $discount = parseFloat($form.find(".discount").val());
    var $total = parseFloat($form.find(".total").val());

    if ($billId && $productId && $productName && $price && $amount && $total) {
        // Find the bill that matches the billId
        $(".reactionary_wapper .reactionary-button").each(function () {
            var currentBillId = $(this).find(".saleBillId").val();
            if (currentBillId === $billId) {
                var $billDesc = $(this).next(".reactionary-decs");
                var $existingItem = $billDesc.find(".item").filter(function () {
                    return $(this).find(".productId").val() === $productId;
                });

                if ($existingItem.length > 0) {
                    var currentAmount = parseFloat($existingItem.find(".amount").val());
                    var newAmount = currentAmount + $amount;
                    $existingItem.find(".amount").val(newAmount);
                    var newTotal = newAmount * parseFloat($price) - parseFloat($discount);
                    $existingItem.find(".total").text(newTotal);
                } else {
                    // Create a new item element
                    var newItem = `
                            <div class="item">
                                <input type="hidden" value="${$categoryId}" class="categoryId">
                                <input type="hidden" value="${$productId}" class="productId">
                                <span class="productName" style="width:150px;height:auto;text-align:center;display: block;">${$productName}</span>
                                <span class="price" style="width:70px;height:auto;text-align:center;display: block;">${$price}</span>
                                <input type="text" class="amount" value="${$amount}" style="width:70px;height:30px;text-align:center">
                                <input type="text" class="discount" value="${$discount}" style="width:70px;height:30px;text-align:center">
                                <span class="total" style="width:70px;height:auto;text-align:center;display: block;">${$total}</span>
                                <span style="width:auto;height:auto;text-align:center; display: block;">
                                    <button class="delete-btn">
                                        <i class="fa-solid fa-trash"></i>
                                    </button>
                                </span>
                            </div>
                        `;
                    $billDesc.append(newItem);
                }

                // Reset/Close Form
                // Note: 'add_newItem' variable usage in SaleBill.js implies a global ID or variable.
                // In _EditBillModal.cshtml, the add button id might be 'new-reactionary'?
                // But in Cashier.js we rely on selectors.
                // Let's find the main "Add/Cancel" button to reset it.
                var $toggleBtn = $("#new-reactionary");
                $toggleBtn.text("اضافة");
                $toggleBtn.css("background", "var(--color-primary)");

                $(".form-reactionary").hide();
                $(".reactionary_wapper").css("display", "flex");

                if (!$(this).hasClass("active")) {
                    $(this).find("button").click();
                }
                return false;
            }
        });
    } else {
        alert("تأكد من ادخال كل الحقول");
    }
});

function filterBillsBySearch() {
    const searchValue = document.getElementById('billSearchInput').value.trim();
    const billButtons = document.querySelectorAll('.reactionary-button');

    billButtons.forEach(function (billButton) {
        const billIdInput = billButton.querySelector('.saleBillId');
        const billId = billIdInput ? billIdInput.value : '';

        // Get the corresponding bill details container
        const billDetails = billButton.nextElementSibling;

        if (searchValue === '' || billId.includes(searchValue)) {
            billButton.style.display = 'flex';
            if (billDetails && billDetails.classList.contains('reactionary-decs')) {
                billDetails.style.display = 'block';
            }
        } else {
            billButton.style.display = 'none';
            if (billDetails && billDetails.classList.contains('reactionary-decs')) {
                billDetails.style.display = 'none';
            }
        }
    });
}

function clearBillSearch() {
    document.getElementById('billSearchInput').value = '';
    filterBillsBySearch();
}

// Helper function to check hole amounts (required for bill update/print)
window.checkHolesAmount = function (billDetailRegisterVM) {
    return $.ajax({
        url: '/Cashier/BillSafary/CheckHolesAmount',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(billDetailRegisterVM)
    }).then(
        () => true,
        () => {
            if (typeof toastr !== 'undefined') toastr.error("الكمية الموجودة في الفاتورة غير متوفرة");
            return false;
        }
    );
};