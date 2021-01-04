
$(document).ready(function () {
    console.log("Activated");
    $(".addFoodBtn").click(function () {
        var KitchenId = $(this).data("kitchenId");
        $.ajax({
            type: 'GET',
            url: "AddEditFoodView",
            data: {
                "kId": KitchenId,
                "fId": 0
            },
            success: function (result) {
                $("#AddEditFoodBody").html(result);
            }

        });
    });
    $(".editFoodBtn").click(function () {
        var KitchenId = $(this).data("kitchenId");
        var FoodId = $(this).data("foodId");
        console.log("Edit food");
        $.ajax({
            type: 'GET',
            url: "AddEditFoodView",
            data: {
                "kId": KitchenId,
                "fId": FoodId
            },
            success: function (result) {
                $("#AddEditFoodBody").html(result);
            }

        });
    });
    $(".eatFoodBtn").click(function () {
        var FoodId = $(this).data("foodId");
        $.ajax({
            type: 'GET',
            url: "EatFoodView",
            data: {
                "fId": FoodId
            },
            success: function (result) {
                $("#EatFoodBody").html(result);
            }

        });
    });
    $(".deleteFoodBtn").click(function () {
        var FoodId = $(this).data("foodId");
        console.log("Delete: " + FoodId);
        $.ajax({
            type: 'Delete',
            url: "DeleteFood",
            data: {
                "fId": FoodId
            },
            success: function (result) {
                console.log(result);
                $("#Kitchens").html(result);
                console.log("Refresh");
            }

        });
    });

});

$("#AddFood").on("click", "#btnBarcode", function () {
    var nameTb = $("#Food_Name");
    var barcode = $("#Food_Barcode");

    console.log("barcode: " + barcode.val());
    console.log("barcode: " + $("#Food_Barcode").val());


    $.ajax({
        type: 'Get',
        url: "GetBarcodeData",
        data: {
            "barcode": barcode.val()
        },
        success: function (result) {
            console.log(result);
            nameTb.val(result);
        }
    });

});
