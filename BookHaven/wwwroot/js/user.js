var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    var userData = [];
    $.ajax({
        type: "GET",
        url: '/admin/user/getall',
        async: false,
        success: function (data) {
            $.each(data, function (key, value) {
                userData.push([value.name, value.email, value.phoneNumber, value.company.name, value.role,value.id ,value.lockoutEnd]);
            })
        },
        failure: function (err) {

        }
    });
    dataTable = $('#tableData').DataTable({
        data: userData,
        columns: [
            { title: "name", "width": "15%" },
            { title: "email", "width": "15%" },
            { title: "phoneNumber", "width": "15%" },
            { title: "company.name", "width": "15%" },
            { title: "role", "width": "15%" },
            {
                title: "Action",
                value: { id: "id", lockoutEnd: "lockoutEnd" },
                "render": function (data,type,row,meta) {
                    var lockoutEnd = row[6];
                    var today = new Date().getTime();
                    var lockout = new Date(lockoutEnd).getTime();

                    if (lockout > today) {
                        return `
                        <div class="row text-center">
                            <div class="d-flex justify-content-center">
                                <a onclick=LockUnlock('${data}') class = "btn btn-success text-white rounded" style="cursor:pointer; width:auto; padding: 5px 10px; margin-right: 10px;">
                                    <i class="fa-solid fa-unlock"></i> Unlock
                                </a>
                                <a onclick=RoleManagement('${data}') class = "btn btn-success text-white rounded" style="cursor:pointer; width:auto; padding: 5px 10px; margin-right: 10px;">
                                //<a href="/admin/user/RoleManagement?userId="${data}" class = "btn btn-success text-white rounded" style="cursor:pointer; width:auto; padding: 5px 10px; margin-right: 10px;">
                                    <i class="fa-solid fa-square-pen"></i> Permission
                                </a>
                                </div>
                        </div>`
                    }
                    else {
                        return `
                        <div class="row text-center">
                            <div class="d-flex justify-content-center">
                                <a onclick=LockUnlock('${data}') class = "btn btn-danger text-white rounded" style="cursor:pointer; width:auto; padding: 5px 10px; margin-right: 10px;">
                                    <i class="fa-solid fa-lock"></i> lock
                                </a>
                                <a href="/admin/user/RoleManagement?userId="${data}" class = "btn btn-success text-white rounded" style="cursor:pointer; width:auto; padding: 5px 10px; margin-right: 10px;">
                                    <i class="fa-solid fa-square-pen"></i> Permission
                                </a>
                            </div>
                        </div>`

                    }

                },
                "width": "15%"
            }
        ]
    });
}

function LockUnlock(id)
{
    $.ajax({
        type: "POST",
        url: '/admin/user/LockUnlock',
        data: JSON.stringify(id),
        contentType : "application/json",
        success: function (data) {
            if (data.success)
            {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        },
        failure: function (err) {

        }

    })

}


