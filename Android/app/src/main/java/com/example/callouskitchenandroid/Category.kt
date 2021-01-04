/* Authors: Laura Stewart */
package com.example.callouskitchenandroid

/**
 * Represents a category in the kitchen
 *
 * @author Laura Stewart
 */
class Category
{
    var id: Int = 0
    var name: String = ""

    /**
     * Creates a Category with an Id and name
     *
     * @param id The category's index
     * @param name The category title
     * @author Laura Stewart
     */
    constructor(id:Int, name:String)
    {
        this.id = id
        this.name = name
    }
}