//var js = jQuery.noConflict(true);
var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    var prodData = [];
    $.ajax({
        type: "GET",
        url: '/admin/product/getall',
        async: false,
        success: function (data) {
            $.each(data, function (key, value) {
                prodData.push([value.title, value.description, value.author, value.category.name, value.price, value.id]);
            })
        },
        failure: function (err) {

        }
    });
    dataTable=$('#tableData').DataTable({
        data: prodData,
        columns: [
            { title: "title", "width": "10%" },
            { title: "description", "width": "50%" },
            { title: "author", "width": "10%" },
            { title: "category", "width": "10%" },
            { title: "price", "width": "5%" },
            {
                title: "Action",
                value : "id",
                "render": function (value) {
                    return `<div class="w-75 btn-group" role="group">
                                <a href="/admin/product/addupdateproduct?id=${value}"><i class="fa-regular fa-pen-to-square" style="margin-right: 10px;" ></i></a>
                                <a onClick="Delete('/Admin/Product/DeleteProduct/${value}')"><i class="fa-solid fa-trash"></i></a>
                            </div>`;
                },
                "width": "15%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    let dataTable = $('#tableData').DataTable();
                    if (dataTable) {
                        dataTable.ajax.reload();
                        dataTable.clear().rows.add(prodData).draw();
                    } else {
                        console.error("DataTable instance not found.");
                    }
                    //$('#tableData').DataTable().ajax.reload();
                    toastr.success(data.message);
                },
                error: function (xhr, status, error) {
                    console.error(xhr.responseText);
                    toastr.error('Error deleting item');
                }
            });
        }
    });
}


