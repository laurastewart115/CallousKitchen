// Author: Peter
// Script used on the layout page (Which makes it used on all pages) to open the two modals on the navbar
$("#btnRecipeModal").on("click", function () {
    console.log("recipe");
    $.ajax({
        type: 'Get',
        url: "RecipeSearchView",
        success: function (result) {
            $("#recipeSearchBody").html(result);
        }
    });
});
$("#btnShoppingListModal").on("click", function () {
    $.ajax({
        type: 'Post',
        url: "ShoppingList",
        success: function (result) {
            $("#shoppingListBody").html(result);
        }
    });
});
