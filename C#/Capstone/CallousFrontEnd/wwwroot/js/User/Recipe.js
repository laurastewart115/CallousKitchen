// Author: Peter
// Script file used for the recipe modal

// Loading indicator, recipe api might take a while
const loading = $("#loading");
loading.hide();


// Function to call the recipe search
function recipeSearch() {
    if ($("#tbRecipeSearch").val()) {
        $.ajax({
            type: 'Post',
            url: "SearchRecipes",
            data: {
                "search": $("#tbRecipeSearch").val()
            },
            beforeSend: function () {
                loading.show();
            },
            success: function (result) {
                $("#recipeContainer").html(result);
                loading.hide();
            }
        });
    }
}

// Function for reciple search button
$("#btnSearchRecipe").on("click", function () {
    recipeSearch();
});


// Function for the "feeling lucky" button
$("#btnFeelingLucky").on("click", function () {
    $.ajax({
        type: 'Post',
        url: "FeelingLucky",
        beforeSend: function () {
            loading.show();
        },
        success: function (result) {
            $("#recipeContainer").html(result);
            loading.hide();
        }
    });

});


// Function to search for recipes when enter is pressed on the textbox
$("#tbRecipeSearch").on("keydown", function (e) {
    if (e.keyCode == 13) { // if pressed enter
        recipeSearch();
    }
});
