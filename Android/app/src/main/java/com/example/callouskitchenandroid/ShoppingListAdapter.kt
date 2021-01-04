/* Author: Kevin Gadelha, Laura Stewart */
package com.example.callouskitchenandroid

import android.app.Activity
import android.content.SharedPreferences
import android.view.View
import android.view.ViewGroup
import android.widget.*
import com.android.volley.Response
import org.json.JSONObject
import java.text.DecimalFormat

/**
 * Extends the ArrayAdapter class so that the food items can be displayed in a shopping list
 * Similar to inventory but with onshoppinglist instead of favoriting
 *
 * @param context The current context of the activity
 * @param foods A list of Food objects to be displayed
 * @author Laura Stewart mostly with on shoppinglist selection by Kevin Gadelha
 */
class ShoppingListAdapter(private val context: Activity,
                          private val foods: List<Food>)
    : ArrayAdapter<Food>(context, R.layout.food_item_list, foods) {

    /**
     * Called when the list item is being created.
     *
     * @param position Index of the current element in the list
     * @param convertView
     * @param parent
     */
    override fun getView(position: Int, convertView: View?, parent: ViewGroup): View {
        val inflater = context.layoutInflater
        val rowView = inflater.inflate(R.layout.shopping_list_item, null, true)

        // change the text to match the food name
        val chkBxFoodName = rowView.findViewById<CheckBox>(R.id.checkBoxFood)
        chkBxFoodName.text = foods[position].name

        // Check the box based on food's info
        chkBxFoodName.isChecked = foods[position].onShoppingList

        // When the check box value is changed, update the database
        //Similar to the favoriting process
        chkBxFoodName.setOnCheckedChangeListener { buttonView, isChecked ->
            ServiceHandler.callAccountService(
                "ShoppingListFood", hashMapOf(
                    "foodId" to foods[position].id,
                    "onShoppingList" to isChecked
                ), context,
                Response.Listener { response ->
                    val json = JSONObject(response.toString())
                    val success = json.getBoolean("ShoppingListFoodResult")
                    // return to the food list
                    if (!success){
                        Toast.makeText(context,"Failed :(", Toast.LENGTH_LONG).show()
                        //Undo if fail
                        chkBxFoodName.isChecked = !isChecked
                    }
                    else{
                        //Update the entry in the original list
                        foods[position].onShoppingList = isChecked
                    }
                })
        }

        // Show the food quantity
        val txtQuantity = rowView.findViewById<TextView>(R.id.textViewShopQuantity)
        val dec = DecimalFormat("#,###.##")
        val formattedQuantity = dec.format(foods[position].quantity)
        txtQuantity.text = "Quantity remaining: ${formattedQuantity} ${foods[position].quantityClassifier}"

        return rowView
    }
}