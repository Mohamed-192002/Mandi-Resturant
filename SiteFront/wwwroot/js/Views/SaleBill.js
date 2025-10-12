//Handle Delivery DropDown
$('#deliveryId').select2({
    templateResult: formatCompany,
    templateSelection: formatCompany
});
function formatCompany(state) {
    if (!state.id) {
        return state.text;
    }

    var logo = $(state.element).data('logo');
    var color = $(state.element).data('color');

    return $(
        `<div style="display: flex; align-items: center; gap: 10px; min-height: 40px;">
                 <img src="/SystemImages/Deliveries/${logo}" style="width: 40px; height: 40px; border-radius: 50%;" />
                 <span style="background-color: ${color}; padding: 6px 10px; border-radius: 4px; color: white; font-size: 16px;">
                ${state.text}
                </span>
            </div>`
    );
};

//Print All Day Sales
function printDaySales() {
    var result = confirm("هل ترغب في طباعة فواتير اليوم");
    if (result) {
        $.ajax({
            type: 'GET',
            url: '/Cashier/BillSafary/PrintDaySales',
            success: function () {

            },
            error: function () {

            }
        });
    }
}

//Get All Products
function getAllProducts() {
    $.ajax({
        type: 'GET',
        url: '/Cashier/SaleBill/GetAllProducts',
        success: function (data) {
            $("#allProducts").html(data);
        },
        error: function () {

        }
    });
};


//Add New Row
function addProduct(btn) {
    var productId = $(btn).data('id');
    var productName = $(btn).data('name');
    var productPrice = $(btn).data('price');

    // Check if the product already exists
    var existingProduct = $("#productList").find(`.product_item input[type='hidden'][value='${productId}']`).closest('.product_item');

    if (existingProduct.length > 0) {
        // Product already exists, update the quantity and total
        var quantityInput = existingProduct.find(".item input[type='text']").eq(2); // Quantity input
        var newQuantity = parseInt(quantityInput.val()) + 1;
        quantityInput.val(newQuantity);

        // Update product total
        calculateTotal(existingProduct);
    } else {
        // Product does not exist, add a new one
        $("#productList").append(`
            <div class="product_item">
                <input type="hidden" class="product-id" value="${productId}" />
                <div class="item" style="width: min-content;">
                    <h4>السعر</h4>
                    <input type="text" value="${productPrice}" style="width:70px;height:30px;text-align:center" id="price" readonly />
                </div>
                <div class="item" style="width: min-content;">
                    <h4>الخصم</h4>
                    <input type="text" value="0" style="width:70px;height:30px;text-align:center" id="discount" oninput="updateTotal(this)" />
                </div>
                <div class="item" style="width: min-content;">
                    <h4>الكمية</h4>
                    <input type="text" value="1" style="width:70px;height:30px;text-align:center" id="quantity" oninput="updateTotal(this)" />
                </div>
                <div class="item" style="width: min-content;">
                    <h4>الاجمالي</h4>
                    <p>${productPrice}</p>
                </div>
                <div class="item_name" style="width: min-content;">
                    <h4>${productName}</h4>
                    <button onclick="removeProduct(this)">
                        <i class="fa-solid fa-trash item_icon"></i>
                    </button>
                </div>
            </div>
        `);
    }

    calculateBillTotal();
}

// Function to calculate the total price of a single product
function calculateTotal(productItem) {
    var price = parseFloat(productItem.find("#price").val());
    var discount = parseFloat(productItem.find("#discount").val());
    var quantity = parseInt(productItem.find("#quantity").val());

    // Handle invalid inputs
    discount = isNaN(discount) ? 0 : discount;
    quantity = isNaN(quantity) ? 1 : quantity;

    // Calculate total
    var total = (price - discount) * quantity;
    total = total < 0 ? 0 : total;

    productItem.find(".item p").text(total.toFixed(2));
}

// Function to calculate the total bill
function calculateBillTotal() {
    var totalWithoutDiscount = 0;

    // Sum up the total for all products
    $("#productList .product_item").each(function () {
        var productTotal = parseFloat($(this).find(".item p").text());
        productTotal = isNaN(productTotal) ? 0 : productTotal;
        totalWithoutDiscount += productTotal;
    });

    // Add delivery price if exists
    var deliveryPrice = 0;
    var deliveryItem = document.querySelector('.delivery-item');
    if (deliveryItem) {
        var deliveryPriceText = deliveryItem.querySelector('.delivery-area-price').textContent;
        deliveryPrice = parseFloat(deliveryPriceText.replace(/[^\d.]/g, ''));
        deliveryPrice = isNaN(deliveryPrice) ? 0 : deliveryPrice;
    }

    // Get the discount and VAT from inputs
    var discount = parseFloat($(".total_item input").eq(0).val()); // Discount input
    var vatRate = parseFloat($(".total_item input").eq(1).val()); // VAT input

    // Handle invalid inputs
    discount = isNaN(discount) ? 0 : discount;
    vatRate = isNaN(vatRate) ? 0 : vatRate;

    // Calculate the net total
    var totalAfterDiscount = totalWithoutDiscount - discount;
    totalAfterDiscount = totalAfterDiscount < 0 ? 0 : totalAfterDiscount;

    var vatAmount = (totalAfterDiscount * vatRate) / 100;
    var netTotal = totalAfterDiscount + vatAmount + deliveryPrice;

    $("#finalTotal").text(netTotal.toFixed(2));
}

// Event listeners for discount and VAT changes
function setupDiscountVatListeners() {
    $(".total_item input[type='text']").on("input", function () {
        calculateBillTotal();
    });
}

// Initialize everything on document ready
$(document).ready(function () {
    setupDiscountVatListeners();
});

// Function to get delivery price
function getDeliveryPrice() {
    debugger;
    var deliveryItem = document.querySelector('.delivery-item');
    if (deliveryItem) {
        var deliveryPriceText = deliveryItem.querySelector('.delivery-area-price').textContent;
        var deliveryPrice = parseFloat(deliveryPriceText.replace(/[^\d.]/g, ''));
        return isNaN(deliveryPrice) ? 0 : deliveryPrice;
    }
    return 0;
}

// Event handler for recalculation
function updateTotal(input) {
    var productItem = $(input).closest('.product_item');
    calculateTotal(productItem);
    calculateBillTotal();
}

// Optional: Remove product function
function removeProduct(btn) {
    $(btn).closest('.product_item').remove();
    calculateBillTotal();
}

// Function to select the bill type
function selectBillType(button) {
    debugger
    var radio = $(button).siblings('input[type="radio"]');
    radio.prop('checked', true);
    var selectedType = $('input[name="billType"]:checked').val();
    debugger
    if (selectedType == 1) {
        $(".add-Tables").removeClass('open');
        //$(".choose-Delivery").addClass('open');
        $(".company-delivery").removeClass('open');
        $("#customerName").show();
        $("#orderDeliveredTime").removeClass('open');
        $("#customerDeliveryTimePop").hide();
        $("#gift").addClass('open');
        $(".choose-Delivery .addCustomer").show();
        $(".order-input-container").hide();
    }
    else if (selectedType == 2) {
        $(".add-Tables").addClass('open');
        //$(".choose-Delivery").addClass('open');
        $(".company-delivery").addClass('open');
        $("#customerName").hide();
        $("#gift").removeClass('open');
        $("#orderDeliveredTime").removeClass('open');
        $("#customerDeliveryTimePop").hide();
        $(".choose-Delivery .addCustomer").hide();
        $(".order-input-container").show();

    }
    else if (selectedType == 3) {
        $(".add-Tables").addClass('open');
        //$(".choose-Delivery").addClass('open');
        $(".company-delivery").removeClass('open');
        $("#gift").removeClass('open');
        $("#customerName").show();
        $("#orderDeliveredTime").addClass('open');
        $("#customerDeliveryTimePop").show();
        $(".choose-Delivery .addCustomer").show();
        $(".order-input-container").hide();


    }
    else if (selectedType == 4) {
        $(".add-Tables").addClass('open');
        //$(".choose-Delivery").addClass('open');
        $(".company-delivery").removeClass('open');
        $("#gift").removeClass('open');
        $("#customerName").show();
        $("#orderDeliveredTime").addClass('open');
        $("#customerDeliveryTimePop").show();
        $(".choose-Delivery .addCustomer").show();
        $(".order-input-container").show();


    }

};

function addSelectedHoles() {
    // Select the UL element
    const $selectedHoleList = $('#selectedHoles');
    $selectedHoleList.empty();

    // Get all checked checkboxes
    $('.Tables_content input[type="checkbox"]:checked').each(function () {
        const holeName = $(this).data('name');
        const holeId = $(this).data('id');
        const listItem = `<li id="${holeId}">${holeName}</li>`;
        $selectedHoleList.append(listItem);
        Modal_Table.classList.remove("open");
    });
};
//function addSelectedHoles() {
//    const $selectedHoleList = $('#selectedHoles');
//    $selectedHoleList.empty();

//    $.ajax({
//        url: '/Cashier/SaleBill/GetCurrentHoles',
//        method: 'GET',
//        dataType: 'json',
//        success: function (holes) {
//            holes.forEach(function (hole) {
//                const listItem = `<li id="${hole.id}">${hole.name}</li>`;
//                $selectedHoleList.append(listItem);
//            });

//            Modal_Table.classList.remove("open");
//        },
//        error: function (xhr, status, error) {
//            console.error('Error fetching selected holes:', error);
//        }
//    });
//}
function validatePhone() {
    const phone = document.getElementById("CustomerRegisterVM_Phone").value.trim();
    const phoneLamp = document.getElementById("phoneLamp");
    const phoneVal = document.getElementById("phoneVal");

    if (phone === "") {
        phoneLamp.style.color = "red";
        phoneLamp.classList.add("flicker");
        phoneVal.style.display = "inline";
    } else {
        phoneVal.style.display = "none";
    }
}
function validateName() {
    const name = document.getElementById("CustomerRegisterVM_Name").value.trim();
    const nameLamp = document.getElementById("nameLamp");
    const nameVal = document.getElementById("nameVal");

    if (name === "") {
        nameLamp.style.color = "red";
        nameLamp.classList.add("flicker");
        nameVal.style.display = "inline";
    } else {
        nameVal.style.display = "none";
    }
}
function validateAddress() {
    const address = document.getElementById("CustomerRegisterVM_Address").value.trim();
    const addressLamp = document.getElementById("addressLamp");
    const addressVal = document.getElementById("addressVal");

    if (address === "") {
        addressLamp.style.color = "red";
        addressLamp.classList.add("flicker");
        addressVal.style.display = "inline";
    } else {
        addressVal.style.display = "none";
    }
}
//Add New Customer
function addNewCustomer() {
    validatePhone();
    validateName();
    validateAddress();
    var data = new FormData(document.getElementById("customerForm"));
    var phone = data.get("CustomerRegisterVM.Phone");
    var name = data.get("CustomerRegisterVM.Name");
    var address = data.get("CustomerRegisterVM.Address");

    if (phone === "" || name === "" || address === "") {
        return;
    }
    let isValid = true;
    let isCheckboxChecked = false;
    // Validate checkboxes and their corresponding input fields
    $(".input-container input[type='checkbox']").each(function () {
        if ($(this).is(":checked")) {
            isCheckboxChecked = true;
            let textInput = $(this).prev("input[type='text']");

            if (textInput.val().trim() === "") {
                textInput.focus();
                isValid = false;
                return false;
            }

            checkedAddress = textInput.val().trim();
        }
    });

    // Ensure at least one checkbox is checked
    if (!isCheckboxChecked) {
        alert("يرجى تحديد عنوان واحد على الأقل!");
        return;
    }

    if (!isValid) {
        alert("يرجى إدخال العنوان بجوار الخيار المحدد!");
        return;
    }

    if (phone != "" && name != "" && address != "") {
        //console.log(data);
        $.ajax({
            type: 'POST',
            url: '/Cashier/SaleBill/AddNewCustomer',
            processData: false,
            contentType: false,
            data: data,
            success: function (data) {
                Modal_Customer.classList.remove("open");
                toastr.success("تم الاضافة بنجاح");
                debugger
                //console.log(data);
                const $total = $(".total_wapper");
                $total.find("#customerName span").text(name);
                $total.find("#customerName span").attr('id', data.customerId);
                $("#customerName input[type='hidden']").val(checkedAddress);
            },
            error: function () {

            }
        });
        return;
    }
    else {
        toastr.error("أكمل بيانات العميل");
    }
};

//GetCustomerData
function GetCustomerData() {
    var phone = $("#CustomerRegisterVM_Phone").val();
    if (phone != "") {
        $.ajax({
            type: 'GET',
            url: '/Cashier/SaleBill/GetCustomerData?phone=' + phone,
            success: function (data) {
                //console.log(data);
                $("#CustomerRegisterVM_Name").val(data.name);
                $("#CustomerRegisterVM_AnotherPhone").val(data.anotherPhone);
                $("#CustomerRegisterVM_Address").val(data.address);
                $("#CustomerRegisterVM_Address2").val(data.address2);
                $("#CustomerRegisterVM_Address3").val(data.address3);
                $("#CustomerRegisterVM_Address4").val(data.address4);
                validatePhone();
                validateName();
                validateAddress();
            },
            error: function () {
                $("#CustomerRegisterVM_Name").val('');
                $("#CustomerRegisterVM_AnotherPhone").val('');
                $("#CustomerRegisterVM_Address").val('');
                $("#CustomerRegisterVM_Address2").val('');
                $("#CustomerRegisterVM_Address3").val('');
                $("#CustomerRegisterVM_Address4").val('');
            }
        });
    }
};

//handle Customer Address
$('#address1 button').on('click', function () {
    $("#address2").addClass('open');
});

$('#address2 button').on('click', function () {
    $("#address3").addClass('open');
});

$('#address3 button').on('click', function () {
    $("#address4").addClass('open');
});

//Check on only one Address
$(".input-container input[type='checkbox']").on("change", function () {
    if ($(this).is(":checked")) {
        $(".input-container input[type='checkbox']").not(this).prop("checked", false);
    }
});

//Handle Holes Real Time
var connection = new signalR.HubConnectionBuilder().withUrl("/dataHub").build();
connection.on("ReceiveData", function (dagagHoleDataVM, meatHoleDataVM) {
    //console.log(dagagHoleDataVM, meatHoleDataVM);
    $("#dagagHolesData").html('');
    for (var i = 0; i < dagagHoleDataVM.length; i++) {
        let endTimeText = dagagHoleDataVM[i].endTime
            ? ` الفتح : ${formatTime(dagagHoleDataVM[i].endTime)}`
            : " الفتح : ____";
        $("#dagagHolesData").append(`
                <div class="table_item_table">
                        <img src="/assets/images/hole2.jpg"
                        alt="table"
                        loading="lazy" />
                        <div>
                        <h3>${dagagHoleDataVM[i].name}</h3>
                        <h5>${endTimeText}</h5>
                        <h5>العدد : ${dagagHoleDataVM[i].amount}</h5>
                        <input type="checkbox" data-id="${dagagHoleDataVM[i].id}" data-name="${dagagHoleDataVM[i].name}" />
                    </div>
                 </div>
        `);
    }

    $("#meatHolesData").html('');
    for (var k = 0; k < meatHoleDataVM.length; k++) {
        let endTimeText = meatHoleDataVM[k].endTime
            ? ` الفتح : ${formatTime(meatHoleDataVM[k].endTime)}`
            : " الفتح : ____";
        $("#meatHolesData").append(`
                <div class="table_item_table">
                        <img src="/assets/images/hole2.jpg"
                        alt="table"
                        loading="lazy" />
                        <div>
                        <h3>${meatHoleDataVM[k].name}</h3>
                        <h5>${endTimeText}</h5>
                        <h5>عدد النفر : ${meatHoleDataVM[k].nafrAmount}</h5>
                        <h5>عدد نص النفر : ${meatHoleDataVM[k].halfNafrAmount}</h5>
                        <input type="checkbox" data-id="${meatHoleDataVM[k].id}" data-name="${meatHoleDataVM[k].name}" />
                    </div>
                 </div>
        `);
    }

    $("#dagagHolesSidebar").html('');
    $("#dagagHolesSidebar").append(`
                    <li style="text-align:center;">
                        <h2 style="color:red;">حفر الدجاج</h2>
                    </li>
    `);
    for (var j = 0; j < dagagHoleDataVM.length; j++) {
        let endTimeText = dagagHoleDataVM[j].endTime
            ? ` الفتح : ${formatTime(dagagHoleDataVM[j].endTime)}`
            : " الفتح : ____";
        $("#dagagHolesSidebar").append(`
               <li style="text-align:center;border-radius: 10px;background-color: white;">
                        <h3>${dagagHoleDataVM[j].name}</h3>
                        <div>
                            <h4>${endTimeText}</h4>
                            <h4 style="color:red;">العدد : ${dagagHoleDataVM[j].amount}</h4>
                        </div>
                </li>
        `);
    }
    $("#dagagHolesSidebar").append(`
                     <li style="text-align:center;border-radius: 10px;background-color: black;color:white;">
                        <h3>المجموع</h3>
                        <div>
                            <h4>العدد : ${dagagHoleDataVM.reduce((sum, item) => sum + item.amount, 0)}</h4>
                        </div>
                    </li>
    `);

    $("#meatHolesSidebar").html('');
    $("#meatHolesSidebar").append(`
                    <li style="text-align:center;">
                        <h2 style="color:red;">حفر اللحم</h2>
                    </li>
    `);
    for (var l = 0; l < meatHoleDataVM.length; l++) {
        let endTimeText = meatHoleDataVM[l].endTime
            ? ` الفتح : ${formatTime(meatHoleDataVM[l].endTime)}`
            : " الفتح : ____";
        $("#meatHolesSidebar").append(`
                <li style="text-align:center;border-radius: 10px;background-color: white;">
                        <h3>${meatHoleDataVM[l].name}</h3>
                        <div>
                            <h4>${endTimeText}</h4>
                            <h4 style="color:red;">عدد النفر : ${meatHoleDataVM[l].nafrAmount}</h4>
                            <h4 style="color:red;">عدد نص النفر : ${meatHoleDataVM[l].halfNafrAmount}</h4>
                        </div>
                </li>
        `);
    }
    $("#meatHolesSidebar").append(`
                   <li style="text-align:center;border-radius: 10px;background-color: black;color:white;">
                        <h3>المجموع</h3>
                        <div>
                            <h4>عدد النفر : ${meatHoleDataVM.reduce((sum, item) => sum + item.nafrAmount, 0)}</h4>
                            <h4>عدد نص النفر : ${meatHoleDataVM.reduce((sum, item) => sum + item.halfNafrAmount, 0)}</h4>
                        </div>
                    </li>
    `);
});

connection.start();

function formatTime(timeString) {
    // Convert the time string into a JavaScript Date object
    const [hours, minutes, seconds] = timeString.split(':');
    const date = new Date();
    date.setHours(parseInt(hours), parseInt(minutes), parseInt(seconds));

    // Format to 12-hour time with AM/PM
    return date.toLocaleTimeString('en-US', {
        hour: 'numeric',
        minute: 'numeric',
        hour12: true,
    });
};

//Alert Sound
function playErrorSound() {
    const audio = document.getElementById("errorSound");
    if (audio) {
        audio.play();
    }
}

//Save Bill
async function CreateBill() {
    debugger
    var selectedType = $('input[name="billType"]:checked').val();
    var orderNumber = $("#orderNumber").val();

    if (selectedType == 1) {
        //Customer
        var customerId = $("#customerName span").attr('id');
        if (customerId == undefined || customerId == null) {
            return toastr.error("يجب إدخال بيانات العميل");
        }
        const productsData = [];
        $("#productList .product_item").each(function () {
            const $product = $(this);
            const productId = parseInt($product.find("input.product-id").val(), 10);
            const price = $product.find("#price").val();
            const discount = $product.find("#discount").val();
            const quantity = $product.find("#quantity").val();
            const total = $product.find(".item:nth-of-type(4) p").text();
            const name = $product.find(".item_name h4").text();

            productsData.push({
                productId: productId,
                pName: name.trim(),
                price: parseFloat(price),
                discount: parseFloat(discount),
                Amount: parseInt(quantity, 10),
                totalPrice: parseFloat(total)
            });
        });

        const $total = $(".total_wapper");

        const discount = $total.find(".total_item:eq(0) input").val();
        const vat = $total.find(".total_item:eq(1) input").val();
        const netTotal = $total.find("#finalTotal").text();
        const gift = $total.find("#gift input").prop("checked");
        const note = $total.find("#notes").val();
        const customerAddress = $("#customerName input[type='hidden']").val();
        debugger;
        const deliveryPrice = getDeliveryPrice();

        const billSafaryRegisterVM = {
            billDetailRegisterVM: productsData,
            customerId: customerId,
            discount: parseFloat(discount),
            vat: parseFloat(vat),
            finalTotal: parseFloat(netTotal),
            deliveryPrice: deliveryPrice,
            gift: gift,
            notes: note,
            orderNumber: orderNumber,
            customerAddress: customerAddress
        };
        console.log(orderNumber);
        console.log(billSafaryRegisterVM);

        if (billSafaryRegisterVM.billDetailRegisterVM.length == 0) {
            return toastr.error("يجب إدخال علي الاقل صنف واحد في الفاتورة");
        }
        //console.log(billSafaryRegisterVM);
        const result = await checkHolesAmount(productsData);
        if (!result) {
            return;
        }
        //return;
        var button = $("#btn_save");
        button.prop("disabled", true);
        $.ajax({
            url: '/Cashier/BillSafary/SaveSaleSafary',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(billSafaryRegisterVM),
            success: function (response) {
                toastr.success("تم حفظ الفاتورة بنجاح");

                //Update Data Real Time
                connection.invoke("SendData");
                setTimeout(function () {
                    window.location.href = "/Cashier/SaleBill/Index";
                }, 1000);
            },
            error: function (xhr) {
                playErrorSound();
                if (xhr.responseJSON?.error === "PRINT_ERROR") {
                    toastr.info("تم الحفظ ولكن حدث خطأ في الطباعة");
                } else if (xhr.responseJSON?.error === "TRANSACTION_ERROR") {
                    toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
                }
                setTimeout(function () {
                    window.location.href = "/Cashier/SaleBill/Index";
                }, 1000);
            }

        });

    }
    else if (selectedType == 2) {
        if (orderNumber.length == 0) {
            return toastr.error("ادخل رقم الطلب");
        }
        //Customer
        var customerId = $("#customerName span").attr('id');
        if (customerId == undefined) {
            customerId = null;
        }
        //Delivery
        var deliveryId = $("#deliveryId").val();
        if (deliveryId == 0) {
            return toastr.error("اختر شركة التوصيل");
        }
        //Holes
        const listItems = document.querySelectorAll('#selectedHoles li');
        //if (listItems.length === 0) {
        //    return toastr.error("يجب اختيار الحفرة");
        //}
        const holeIds = [];
        listItems.forEach((item) => {
            holeIds.push(item.id);
        });
        //ProductData
        const productsData = [];
        $("#productList .product_item").each(function () {
            const $product = $(this);
            const productId = parseInt($product.find("input.product-id").val(), 10);
            const price = $product.find("#price").val();
            const discount = $product.find("#discount").val();
            const quantity = $product.find("#quantity").val();
            const total = $product.find(".item:nth-of-type(4) p").text();
            const name = $product.find(".item_name h4").text();

            productsData.push({
                productId: productId,
                pName: name.trim(),
                price: parseFloat(price),
                discount: parseFloat(discount),
                Amount: parseInt(quantity, 10),
                totalPrice: parseFloat(total)
            });
        });

        const $total = $(".total_wapper");

        const discount = $total.find(".total_item:eq(0) input").val();
        const vat = $total.find(".total_item:eq(1) input").val();
        const netTotal = $total.find("#finalTotal").text();
        const time = $total.find("#orderDeliveredTime input").val();
        const note = $total.find("#notes").val();
        const customerAddress = $("#customerName input[type='hidden']").val();
        debugger;
        const deliveryPrice = getDeliveryPrice();

        const billDeliveryRegisterVM = {
            billDetailRegisterVM: productsData,
            deliveryId: deliveryId,
            customerId: customerId,
            holeIds: holeIds,
            discount: parseFloat(discount),
            vat: parseFloat(vat),
            finalTotal: parseFloat(netTotal),
            deliveryPrice: deliveryPrice,
            orderDeliveredTime: time,
            notes: note,
            orderNumber: orderNumber,
            customerAddress: customerAddress
        };
        console.log(orderNumber);
        console.log(billDeliveryRegisterVM);

        if (billDeliveryRegisterVM.billDetailRegisterVM.length == 0) {
            return toastr.error("يجب إدخال علي الاقل صنف واحد في الفاتورة");
        }
        //if (billDeliveryRegisterVM.orderDeliveredTime == "" && listItems.length === 0) {
        //    return toastr.error("يجب إدخال وقت الاستلام أو اختيار الحفر");
        //}
        //console.log(billDeliveryRegisterVM, billDeliveryRegisterVM.time);
        //return;
        debugger
        if (listItems.length != 0) {
            //CheckHoleAmount
            const checkDeliveryHoleAmountVM = {
                billDetailRegisterVM: productsData,
                holeIds: holeIds,
            };
            const result = await checkDeliveryHolesAmount(checkDeliveryHoleAmountVM);
            if (!result) {
                return;
            }
            var button = $("#btn_save");
            button.prop("disabled", true);
            //Send Data To Save
            $.ajax({
                url: '/Cashier/BillDelivery/SaveSaleDelivery',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(billDeliveryRegisterVM),
                success: function (response) {
                    toastr.success("تم حفظ الفاتورة بنجاح");

                    //Update Data Real Time
                    connection.invoke("SendData");
                    setTimeout(function () {
                        window.location.href = "/Cashier/SaleBill/Index";
                    }, 1000);
                },
                error: function (xhr) {
                    playErrorSound();
                    if (xhr.responseJSON?.error === "PRINT_ERROR") {
                        toastr.info("تم الحفظ ولكن حدث خطأ في الطباعة");
                    } else if (xhr.responseJSON?.error === "TRANSACTION_ERROR") {
                        toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
                    }
                    setTimeout(function () {
                        window.location.href = "/Cashier/SaleBill/Index";
                    }, 1000);
                }
            });
        }
        //else if (billDeliveryRegisterVM.orderDeliveredTime != "") {
        //CheckHoleAmount

        const checkDeliveryHoleAmountVM = {
            billDetailRegisterVM: productsData,
            orderDeliveredTime: time,
        };
        console.log(typeof (checkDeliveryHoleAmountVM.orderDeliveredTime));
        //const result = await checkDeliveryHolesAmountByTime(checkDeliveryHoleAmountVM);
        //if (!result) {
        //    return;
        //}
        var button = $("#btn_save");
        button.prop("disabled", true);
        //Send Data To Save
        $.ajax({
            url: '/Cashier/BillDelivery/SaveSaleDeliveryByTime',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(billDeliveryRegisterVM),
            success: function (response) {
                toastr.success("تم حفظ الفاتورة بنجاح");

                //Update Data Real Time
                connection.invoke("SendData");
                setTimeout(function () {
                    window.location.href = "/Cashier/SaleBill/Index";
                }, 1000);
            },
            error: function (xhr) {
                playErrorSound();
                if (xhr.responseJSON?.error === "PRINT_ERROR") {
                    toastr.info("تم الحفظ ولكن حدث خطأ في الطباعة");
                } else if (xhr.responseJSON?.error === "TRANSACTION_ERROR") {
                    toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
                }
                setTimeout(function () {
                    window.location.href = "/Cashier/SaleBill/Index";
                }, 1000);
            }
        });
        //}
    }
    else if (selectedType == 3) {
        //Customer
        var customerId = $("#customerName span").attr('id');
        if (customerId == undefined || customerId == null) {
            return toastr.error("يجب إدخال بيانات العميل");
        }
        //Holes
        const listItems = document.querySelectorAll('#selectedHoles li');
        const holeIds = [];
        listItems.forEach((item) => {
            holeIds.push(item.id);
        });
        //ProductData
        const productsData = [];
        $("#productList .product_item").each(function () {
            const $product = $(this);
            const productId = parseInt($product.find("input.product-id").val(), 10);
            const price = $product.find("#price").val();
            const discount = $product.find("#discount").val();
            const quantity = $product.find("#quantity").val();
            const total = $product.find(".item:nth-of-type(4) p").text();
            const name = $product.find(".item_name h4").text();

            productsData.push({
                productId: productId,
                pName: name.trim(),
                price: parseFloat(price),
                discount: parseFloat(discount),
                Amount: parseInt(quantity, 10),
                totalPrice: parseFloat(total)
            });
        });

        const $total = $(".total_wapper");

        const discount = $total.find(".total_item:eq(0) input").val();
        const vat = $total.find(".total_item:eq(1) input").val();
        const netTotal = $total.find("#finalTotal").text();
        const time = $total.find("#orderDeliveredTime input").val();
        //const customerReceive = $total.find("#customerReceive input").prop("checked");
        const note = $total.find("#notes").val();
        const customerAddress = $("#customerName input[type='hidden']").val();
        const deliveryPrice = getDeliveryPrice();
        const billDeliveryRegisterVM = {
            billDetailRegisterVM: productsData,
            deliveryId: deliveryId,
            customerId: customerId,
            holeIds: holeIds,
            discount: parseFloat(discount),
            vat: parseFloat(vat),
            finalTotal: parseFloat(netTotal),
            orderDeliveredTime: time,
            notes: note,
            deliveryPrice: deliveryPrice,
            orderNumber: orderNumber,
            customerAddress: customerAddress
        };
        console.log(orderNumber);
        console.log(billDeliveryRegisterVM);

        if (billDeliveryRegisterVM.billDetailRegisterVM.length == 0) {
            return toastr.error("يجب إدخال علي الاقل صنف واحد في الفاتورة");
        }

        if (billDeliveryRegisterVM.orderDeliveredTime == "" && listItems.length === 0) {
            return toastr.error("يجب إدخال وقت الاستلام أو اختيار الحفر");
        }
        //console.log(billDeliveryRegisterVM);
        //return;

        if (listItems.length != 0) {
            //CheckHoleAmount
            const checkDeliveryHoleAmountVM = {
                billDetailRegisterVM: productsData,
                holeIds: holeIds,
            };
            const result = await checkDeliveryHolesAmount(checkDeliveryHoleAmountVM);
            if (!result) {
                return;
            }
            var button = $("#btn_save");
            button.prop("disabled", true);
            //Send Data To Save
            $.ajax({
                url: '/Cashier/BillDriver/SaveSaleDelivery',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(billDeliveryRegisterVM),
                success: function (response) {
                    toastr.success("تم حفظ الفاتورة بنجاح");

                    //Update Data Real Time
                    connection.invoke("SendData");
                    setTimeout(function () {
                        window.location.href = "/Cashier/SaleBill/Index";
                    }, 1000);
                },
                error: function (xhr) {
                    playErrorSound();
                    if (xhr.responseJSON?.error === "PRINT_ERROR") {
                        toastr.info("تم الحفظ ولكن حدث خطأ في الطباعة");
                    } else if (xhr.responseJSON?.error === "TRANSACTION_ERROR") {
                        toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
                    }
                    setTimeout(function () {
                        window.location.href = "/Cashier/SaleBill/Index";
                    }, 1000);
                }
            });
        }
        else if (billDeliveryRegisterVM.orderDeliveredTime != "") {
            //CheckHoleAmount
            const checkDeliveryHoleAmountVM = {
                billDetailRegisterVM: productsData,
                orderDeliveredTime: time,
            };
            const result = await checkDeliveryHolesAmountByTime(checkDeliveryHoleAmountVM);
            if (!result) {
                return;
            }
            var button = $("#btn_save");
            button.prop("disabled", true);
            //Send Data To Save
            $.ajax({
                url: '/Cashier/BillDriver/SaveSaleDeliveryByTime',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(billDeliveryRegisterVM),
                success: function (response) {
                    toastr.success("تم حفظ الفاتورة بنجاح");

                    //Update Data Real Time
                    connection.invoke("SendData");
                    setTimeout(function () {
                        window.location.href = "/Cashier/SaleBill/Index";
                    }, 1000);
                },
                error: function (xhr) {
                    playErrorSound();
                    if (xhr.responseJSON?.error === "PRINT_ERROR") {
                        toastr.info("تم الحفظ ولكن حدث خطأ في الطباعة");
                    } else if (xhr.responseJSON?.error === "TRANSACTION_ERROR") {
                        toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
                    }
                    setTimeout(function () {
                        window.location.href = "/Cashier/SaleBill/Index";
                    }, 1000);
                }
            });
        }
    }
    else if (selectedType == 4) {
        //Customer
        var customerId = $("#customerName span").attr('id');
        if (customerId == undefined || customerId == null) {
            return toastr.error("يجب إدخال بيانات العميل");
        }
        //Holes
        const listItems = document.querySelectorAll('#selectedHoles li');
        const holeIds = [];
        listItems.forEach((item) => {
            holeIds.push(item.id);
        });
        //ProductData
        const productsData = [];
        $("#productList .product_item").each(function () {
            const $product = $(this);
            const productId = parseInt($product.find("input.product-id").val(), 10);
            const price = $product.find("#price").val();
            const discount = $product.find("#discount").val();
            const quantity = $product.find("#quantity").val();
            const total = $product.find(".item:nth-of-type(4) p").text();
            const name = $product.find(".item_name h4").text();

            productsData.push({
                productId: productId,
                pName: name.trim(),
                price: parseFloat(price),
                discount: parseFloat(discount),
                Amount: parseInt(quantity, 10),
                totalPrice: parseFloat(total)
            });
        });

        const $total = $(".total_wapper");

        const discount = $total.find(".total_item:eq(0) input").val();
        const vat = $total.find(".total_item:eq(1) input").val();
        const netTotal = $total.find("#finalTotal").text();
        const time = $total.find("#orderDeliveredTime input").val();
        //const customerReceive = $total.find("#customerReceive input").prop("checked");
        const note = $total.find("#notes").val();
        const customerAddress = $("#customerName input[type='hidden']").val();
        debugger;
        const deliveryPrice = getDeliveryPrice();

        const billDeliveryRegisterVM = {
            billDetailRegisterVM: productsData,
            deliveryId: deliveryId,
            customerId: customerId,
            holeIds: holeIds,
            discount: parseFloat(discount),
            vat: parseFloat(vat),
            finalTotal: parseFloat(netTotal),
            deliveryPrice: deliveryPrice,
            orderDeliveredTime: time,
            notes: note,
            orderNumber: orderNumber,
            customerAddress: customerAddress
        };
        console.log(orderNumber);
        console.log(billDeliveryRegisterVM);

        if (billDeliveryRegisterVM.billDetailRegisterVM.length == 0) {
            return toastr.error("يجب إدخال علي الاقل صنف واحد في الفاتورة");
        }
        if (billDeliveryRegisterVM.orderDeliveredTime == "" && listItems.length === 0) {
            return toastr.error("يجب إدخال وقت الاستلام أو اختيار الحفر");
        }
        //console.log(billDeliveryRegisterVM);
        //return;

        if (listItems.length != 0) {
            //CheckHoleAmount
            const checkDeliveryHoleAmountVM = {
                billDetailRegisterVM: productsData,
                holeIds: holeIds,
            };
            const result = await checkDeliveryHolesAmount(checkDeliveryHoleAmountVM);
            if (!result) {
                return;
            }
            var button = $("#btn_save");
            button.prop("disabled", true);
            //Send Data To Save
            $.ajax({
                url: '/Cashier/BillReservation/SaveSaleDelivery',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(billDeliveryRegisterVM),
                success: function (response) {
                    toastr.success("تم حفظ الفاتورة بنجاح");

                    //Update Data Real Time
                    connection.invoke("SendData");
                    setTimeout(function () {
                        window.location.href = "/Cashier/SaleBill/Index";
                    }, 1000);
                },
                error: function (xhr) {
                    playErrorSound();
                    if (xhr.responseJSON?.error === "PRINT_ERROR") {
                        toastr.info("تم الحفظ ولكن حدث خطأ في الطباعة");
                    } else if (xhr.responseJSON?.error === "TRANSACTION_ERROR") {
                        toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
                    }
                    setTimeout(function () {
                        window.location.href = "/Cashier/SaleBill/Index";
                    }, 1000);
                }
            });
        }
        else if (billDeliveryRegisterVM.orderDeliveredTime != "") {
            //CheckHoleAmount
            const checkDeliveryHoleAmountVM = {
                billDetailRegisterVM: productsData,
                orderDeliveredTime: time,
            };
            const result = await checkDeliveryHolesAmountByTime(checkDeliveryHoleAmountVM);
            if (!result) {
                return;
            }
            var button = $("#btn_save");
            button.prop("disabled", true);
            //Send Data To Save
            $.ajax({
                url: '/Cashier/BillReservation/SaveSaleDeliveryByTime',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(billDeliveryRegisterVM),
                success: function (response) {
                    toastr.success("تم حفظ الفاتورة بنجاح");

                    //Update Data Real Time
                    connection.invoke("SendData");
                    setTimeout(function () {
                        window.location.href = "/Cashier/SaleBill/Index";
                    }, 1000);
                },
                error: function (xhr) {
                    playErrorSound();
                    if (xhr.responseJSON?.error === "PRINT_ERROR") {
                        toastr.info("تم الحفظ ولكن حدث خطأ في الطباعة");
                    } else if (xhr.responseJSON?.error === "TRANSACTION_ERROR") {
                        toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
                    }
                    setTimeout(function () {
                        window.location.href = "/Cashier/SaleBill/Index";
                    }, 1000);
                }
            });
        }
    }

};

function checkHolesAmount(billDetailRegisterVM) {
    return $.ajax({
        url: '/Cashier/BillSafary/CheckHolesAmount',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(billDetailRegisterVM)
    }).then(
        () => true,
        () => {
            toastr.error("الكمية الموجودة في الفاتورة غير متوفرة");
            return false;
        }
    );
}

function checkDeliveryHolesAmount(checkDeliveryHoleAmountVM) {
    return $.ajax({
        url: '/Cashier/BillDelivery/CheckDeliveryHolesAmount',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(checkDeliveryHoleAmountVM)
    }).then(
        () => true,
        () => {
            toastr.error("الكمية الموجودة في الفاتورة غير متوفرة");
            return false;
        }
    );
}

function checkDeliveryHolesAmountByTime(checkDeliveryHoleAmountVM) {
    return $.ajax({
        url: '/Cashier/BillDelivery/CheckDeliveryHolesAmountByTime',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(checkDeliveryHoleAmountVM)
    }).then(
        () => true,
        () => {
            toastr.error("الكمية الموجودة في الفاتورة غير متوفرة");
            return false;
        }
    );
}


//SaleBillRefund

//Delete Row


$(".reactionary-decs").on("click", ".delete-btn", function () {
    $(this).closest(".item").remove();
});


$(".reactionary-decs").on("input", ".amount, .discount", function () {
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
        $total.text(total.toFixed(2)); // Update total with 2 decimal places
    };

    updateTotal(); // Update total immediately after input

    // Attach the event for input changes
    $amount.on("input", updateTotal);
    $discount.on("input", updateTotal);
});

//Add New Item To Bill
$(".form-reactionary form .productId").on("change", function () {
    var productId = $(this).val();
    var $form = $(this).closest("form");
    var $price = $form.find(".price");
    var $categoryId = $form.find(".categoryId");
    var $total = $form.find(".total");
    $.ajax({
        type: 'GET',
        url: '/Cashier/SaleBill/getProductPrice?productId=' + productId,
        success: function (data) {
            console.log(data);
            $price.val(data.price);
            $total.val(data.price);
            $categoryId.val(data.categoryId);
        },
        error: function () {

        }
    });
});

$(".form-reactionary form").on("input", ".amount, .discount", function () {
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

$(".form-reactionary form button").on("click", function () {
    var $form = $(this).closest("form");
    var $billId = $form.find(".billId").val();
    var $productId = $form.find(".productId").val();
    var $productName = $form.find(".productId option:selected").text();
    var $categoryId = $form.find(".categoryId").val();
    var $price = $form.find(".price").val();
    var $amount = parseFloat($form.find(".amount").val());
    var $discount = parseFloat($form.find(".discount").val());
    var $total = parseFloat($form.find(".total").val());

    // console.log('$billId : ' + $billId, ' , $productId : ' + $productId, ' , $productName : ' + $productName, ' , $price : ' + $price, ' , $amount : ' + $amount, ' , $discount :' + $discount, ' , $total : ' + $total);
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
                    // Create a new item element if the product doesn't exist
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

                // Convert Modal
                add_newItem.innerHTML = "اضافة";
                add_newItem.style = "background: var(--color-primary)";
                document.querySelector("header #modal.add-reactionary .form-reactionary").style = `display: none`;
                document.querySelector("header #modal.add-reactionary .reactionary_wapper").style = `display: flex`;
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

//Update Bill
$(".fa-print").on("click", async function () {
    const $reactionaryButton = $(this).closest(".reactionary-button");
    const saleBillId = $reactionaryButton.find(".saleBillId").val();
    const refundBillType = $reactionaryButton.find(".refundBillType").val();

    const $reactionaryDecs = $reactionaryButton.next(".reactionary-decs");
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

    const result = await checkHolesAmount(items);
    if (!result) {
        return;
    }

    $.ajax({
        url: '/Cashier/BillSafary/UpdateSaleSafary',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(SaleBillRefundVM),
        success: function (response) {
            toastr.success("تم تعديل الفاتورة بنجاح");
            setTimeout(function () {
                window.location.href = "/Cashier/SaleBill/Index";
            }, 1000);
        },
        error: function (xhr, status, error) {
            toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
            setTimeout(function () {
                window.location.href = "/Cashier/SaleBill/Index";
            }, 1000);
        }
    });
});

async function pasteClipboardContent() {
    try {
        const response = await fetch('/api/number');
        if (!response.ok) {
            throw new Error('No phone number found.');
        }

        const data = await response.json();
        $("#CustomerRegisterVM_Phone").val(data.phoneNumber);
        GetCustomerData();
    } catch (error) {
        console.error("Error fetching phone number:", error);
    }
}


//async function pasteClipboardContent() {
//    try {
//        if (navigator.clipboard) {
//            const text = await navigator.clipboard.readText();
//            $("#CustomerRegisterVM_Phone").val(text);
//            GetCustomerData();
//        } else {
//            //alert("Clipboard API not supported in your browser.");
//        }
//    } catch (err) {
//        console.error("Failed to read clipboard content: ", err);
//    }
//};

//Save Temp Bill
function CreateTempBill() {
    var selectedType = $('input[name="billType"]:checked').val();
    var orderNumber = $("#orderNumber").val();


    if (selectedType == 2) {
        const productsData = [];
        $("#productList .product_item").each(function () {
            const $product = $(this);
            const catId = parseInt($product.find("input.category-id").val(), 10);
            const productId = parseInt($product.find("input.product-id").val(), 10);
            const price = $product.find("#price").val();
            const discount = $product.find("#discount").val();
            const quantity = $product.find("#quantity").val();
            const total = $product.find(".item:nth-of-type(4) p").text();
            const name = $product.find(".item_name h4").text();

            productsData.push({
                categoryId: catId,
                productId: productId,
                pName: name.trim(),
                price: parseFloat(price),
                discount: parseFloat(discount),
                Amount: parseInt(quantity, 10),
                totalPrice: parseFloat(total)
            });
        });

        const $total = $(".total_wapper");

        const discount = $total.find(".total_item:eq(0) input").val();
        const vat = $total.find(".total_item:eq(1) input").val();
        const netTotal = $total.find("#finalTotal").text();
        const note = $total.find("#notes").val();

        const billSafaryRegisterVM = {
            billDetailRegisterVM: productsData,
            discount: parseFloat(discount),
            vat: parseFloat(vat),
            finalTotal: parseFloat(netTotal),
            orderNumber: orderNumber,
            notes: note
        };

        if (billSafaryRegisterVM.billDetailRegisterVM.length == 0) {
            return toastr.error("يجب إدخال علي الاقل صنف واحد في الفاتورة");
        }

        $.ajax({
            url: '/Cashier/BillSafary/SaveTempSafary',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(billSafaryRegisterVM),
            success: function (response) {
                toastr.success("تم حفظ الفاتورة بنجاح");
                setTimeout(function () {
                    window.location.href = "/Cashier/SaleBill/Index";
                }, 1000);
            },
            error: function (xhr, status, error) {
                toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
                setTimeout(function () {
                    window.location.href = "/Cashier/SaleBill/Index";
                }, 1000);
            }
        });
    }
    else {
        return toastr.error("الفواتير المؤقتة للسفري فقط");
    }

};

//Edit TempBill
function EditTempBill() {
    var orderNumber = $("#orderNumber").val();
    const productsData = [];
    $("#productList .product_item").each(function () {
        const $product = $(this);
        const catId = parseInt($product.find("input.category-id").val(), 10);
        const productId = parseInt($product.find("input.product-id").val(), 10);
        const price = $product.find("#price").val();
        const discount = $product.find("#discount").val();
        const quantity = $product.find("#quantity").val();
        const total = $product.find(".item:nth-of-type(4) p").text();
        const name = $product.find(".item_name h4").text();

        productsData.push({
            categoryId: catId,
            productId: productId,
            pName: name.trim(),
            price: parseFloat(price),
            discount: parseFloat(discount),
            Amount: parseInt(quantity, 10),
            totalPrice: parseFloat(total)
        });
    });

    const $total = $(".total_wapper");

    const discount = $total.find(".total_item:eq(0) input").val();
    const vat = $total.find(".total_item:eq(1) input").val();
    const netTotal = $total.find("#finalTotal").text();
    const note = $total.find("#notes").val();
    const saleBillId = $total.find("#saleBillId").val();

    const billSafaryRegisterVM = {
        billDetailRegisterVM: productsData,
        discount: parseFloat(discount),
        vat: parseFloat(vat),
        finalTotal: parseFloat(netTotal),
        notes: note,
        orderNumber: orderNumber,
        id: saleBillId
    };

    if (billSafaryRegisterVM.billDetailRegisterVM.length == 0) {
        return toastr.error("يجب إدخال علي الاقل صنف واحد في الفاتورة");
    }

    $.ajax({
        url: '/Cashier/BillSafary/EditTempSafary',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(billSafaryRegisterVM),
        success: function (response) {
            toastr.success("تم حفظ الفاتورة بنجاح");
            setTimeout(function () {
                window.location.href = "/Cashier/SaleBill/Index";
            }, 1000);
        },
        error: function (xhr, status, error) {
            toastr.error("لم يتم حفظ الفاتورة بشكل صحيح برجاء المحاولة مرة أخري");
            setTimeout(function () {
                window.location.href = "/Cashier/SaleBill/Index";
            }, 1000);
        }
    });

};


