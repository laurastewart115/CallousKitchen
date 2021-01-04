/* Author: Laura Stewart */
package com.example.callouskitchenandroid

/**
 * Represents a recipe received from the Edamam API
 *
 * @author Laura Stewart
 */
class Recipe {
    var name: String = ""
    var url: String = ""
    var source: String = ""
    var image: String = ""
    var recipeYield: Double = 0.0

    /**
     * Main recipe constructor
     *
     * @param name The recipe title
     * @param url The link to the recipe
     * @param source What website the recipe is from
     * @param image Image associated with the recipe
     * @param recipeYield The number of servings the recipe makes
     */
    constructor(name: String, url: String, source: String, image: String, recipeYield: Double)
    {
        this.name = name
        this.url = url
        this.source = source
        this.image = image
        this.recipeYield = recipeYield
    }
}