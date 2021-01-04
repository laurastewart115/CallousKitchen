/* Authors: Kevin Gadelha, Laura Stewart */
package com.example.callouskitchenandroid

import java.io.Serializable
import java.time.LocalDate

/**
 * Represents a food item
 *
 * @author Kevin Gadelha, Laura Stewart
 */
class Food: Serializable {

    var id: Int = 0;
    var name: String = ""
    var barcode: Int = 0
    var expiryDate: LocalDate? = LocalDate.parse("2022-01-01")
    var quantity: Double = 0.0
    var quantityClassifier: String = ""
    var storage: String = ""
    var favourite: Boolean = false
    var onShoppingList: Boolean = false
    //Default of unknown
    var vegan: Int? = -1
    var vegetarian: Int? = -1
    var ingredients = arrayOf<String>()
    var traces = arrayOf<String>()

    /**
     * Creates a food with no expiry date.
     *
     * @param id
     * @param name
     * @param quantity
     * @param barcode
     * @param quantityClassifier
     */
    constructor(id: Int, name: String, quantity: Double = 1.0, barcode: Int = 0, quantityClassifier: String = "")
    {
        this.id = id
        this.name = name
        this.quantity = quantity
        this.quantityClassifier = quantityClassifier
        this.barcode = barcode
    }

    /**
     * Creates a food with only the data needed to check for allergens or to check if it is vegan/vegetarian
     *
     * @param ingredients Array of all ingredients in the food
     * @param traces Array of all ingredients the food may contain traces of
     * @param vegan Whether the food is vegan (1) or not (0)
     * @param vegetarian Whether the food is vegetarian (1) or not (0)
     */
    constructor(ingredients : Array<String>,traces : Array<String>, vegan : Int?, vegetarian : Int?)
    {
        this.ingredients = ingredients
        this.traces = traces
        this.vegan = vegan
        this.vegetarian = vegetarian
    }

    /**
     * Creates a food with all data (except vegetarian/vegan/allergen data)
     *
     * @param id
     * @param name
     * @param quantity
     * @param barcode
     * @param expiry
     * @param quantityClassifier
     */
    constructor(id: Int, name: String, quantity: Double = 1.0, barcode: Int = 0, expiry: LocalDate, quantityClassifier: String = "")
    {
        this.id = id
        this.name = name
        this.quantity = quantity
        this.quantityClassifier = quantityClassifier
        this.barcode = barcode
        this.expiryDate = expiry
    }
}