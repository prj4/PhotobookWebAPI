const uri = "api/Account";
let accounts = null;
let globKey = null;
function getCount(data) {
    const el = $("#counter");
    let name = "account";
    if (data) {
        if (data > 1) {
            name = "accounts";
        }
        el.text(data + " " + name);
    } else {
        el.text("No " + name);
    }
}

$(document).ready(function () {
    getData();
    getHosts();
});

function getData() {
    $.ajax({
        type: "GET",
        url: uri,
        cache: false,
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Something went wrong in getdata!");
        },
        success: function (data) {
            const tBody = $("#accounts");

            $(tBody).empty();

            getCount(data.length);

            $.each(data, function (key, item) {
                const tr = $("<tr></tr>")
                    .append($("<td></td>").text(item.email))
                    .append(
                        $("<td></td>").append(
                            $("<button>Edit</button>").on("click", function () {
                                editItem(key);
                            })
                        )
                    )
                    .append(
                        $("<td></td>").append(
                            $("<button>Delete</button>").on("click", function () {
                                deleteItem(key);
                            })
                        )
                    );

                tr.appendTo(tBody);
            });

            accounts = data;
        }
    });
}



function addItem() {
    const item = {
        Email: $("#AccountEmail").val(),
        Name: $("#AccountName").val(),
        Password: $("#AccountPassword").val(),
        ConfirmPassword: $("#AccountConfirmPassword").val(),
        Role: "Host"
    };

    $.ajax({
        type: "POST",
        accepts: "application/json",
        url: "api/Account/RegisterHost",
        contentType: "application/json",
        data: JSON.stringify(item),
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Something went wrong in additem!");
        },
        success: function (result) {
            getData();
            $("#AccountName").val("");
            $("#AccountEmail").val("");
            $("#AccountPassword").val("");
            $("#AccountConfirmPassword").val("");
        }
    });
}



function deleteItem(key) {

    const tBody = $("#accounts");
    var rows = $('tr', tBody);
    var Email = rows.eq(key).closest('tr').find('td:eq(0)').text();


    $.ajax({
        url: uri + "/" + Email,
        type: "DELETE",
        
        error: function (jqXHR, textStatus, errorThrown) {
            alert("Something went wrong in deleteitem!");
        },
        success: function (result) {
            getData();
        }
    });
}

function editItem(key) {
    globKey = key;
    const tBody = $("#accounts");
    var rows = $('tr', tBody);
    var Email = rows.eq(key).closest('tr').find('td:eq(0)').text();

    $.each(accounts, function (count, item) {
        if (item.email === Email) {
            $("#edit-username").val(item.userName);
            $("#edit-email").val(item.email);
        }
    });
    $("#spoiler").css({ display: "block" });
}




$(".my-form").on("submit", function () {

    const tBody = $("#accounts");
    var rows = $('tr', tBody);
    var Email = rows.eq(globKey).closest('tr').find('td:eq(0)').text();

    NewEmail = $("#edit-email").val()

    const item = {
        UserName: $("#edit-username").val(),
        Email: NewEmail
    };

    $.ajax({
        url: uri + "/" + Email,
        type: "PUT",
        accepts: "application/json",
        contentType: "application/json",
        data: JSON.stringify(item),
        success: function (result) {
            getData();
        }
    });

    closeInput();
    return false;
});

function closeInput() {
    $("#spoiler").css({ display: "none" });
}
