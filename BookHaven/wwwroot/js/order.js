//var js = jQuery.noConflict(true);
var dataTable;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else {
        if (url.includes("completed")) {
            loadDataTable("completed");
        }
        else {
            if (url.includes("pending")) {
                loadDataTable("pending");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else
                {
                    loadDataTable("all");
                }
            }
        }
    }
});

function loadDataTable(status) {
    var prodData = [];
    $.ajax({
        type: "GET",
        url: '/admin/order/getall?status=' + status,
        async: false,
        success: function (data) {
            $.each(data, function (key, value) {
                var orderStatus = getOrderStatusString(value.orderStatus);
                prodData.push([value.id, value.name, value.phoneNumber, value.applicationUser.email, orderStatus, value.orderTotal, value.id]);
            })
        },
        failure: function (err) {

        }
    });
    dataTable = $('#tableData').DataTable({
        data: prodData,
        columns: [
            { title: "id", "width": "5%" },
            { title: "name", "width": "15%", },
            { title: "phoneNumber", "width": "20%" },
            { title: "applicationUser.email", "width": "15%" },
            { title: "orderStatus", "width": "10%" },
            { title: "orderTotal", "width": "5%" },
            {
                title: "Action",
                value: "id",
                "render": function (value) {
                    return `<div class="w-75 btn-group justify-content-center" role="group">
                                <a href="/admin/order/details?id=${value}"><i class="fa-regular fa-pen-to-square" style="margin-right: 10px;" ></i></a>
                            </div>`;
                },
                "width": "15%"
            }
        ]
    });
}

function getOrderStatusString(enumValue) {
    switch (enumValue) {
        case 1:
            return "Pending";
        case 2:
            return "Approved";
        case 3:
            return "Processing";
        case 4:
            return "Shipped";
        case 5:
            return "Cancelled";
        case 6:
            return "Refunded";
        default:
            return "Idle"; 

    }
}


