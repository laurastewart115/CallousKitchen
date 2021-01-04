/* Author: Laura Stewart */
package com.example.callouskitchenandroid

import android.app.Activity
import android.content.Intent
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import android.widget.Button

/**
 * Extends the ArrayAdapter class so that the categories can be displayed in a custom ListView.
 *
 * @param context The current context of the activity
 * @param categories A list of Category objects to be displayed
 * @author Laura Stewart
 */
class CategoryListAdapter (private val context: Activity,
                           private val categories: List<Category>)
    : ArrayAdapter<Category>(context, R.layout.category_item_list, categories)
{
    /**
     * Called when the list item is being created.
     *
     * @param position Index of the current element in the list
     * @param convertView
     * @param parent
     */
    override fun getView(position: Int, convertView: View?, parent: ViewGroup): View {
        val inflater = context.layoutInflater
        val rowView = inflater.inflate(R.layout.category_item_list, null, true)

        // change the button text to match the kitchen name
        val kitchenBtn = rowView.findViewById<Button>(R.id.btnKitchen)
        kitchenBtn.text = categories[position].name

        // set on click listener for the button to go to the inventory view
        kitchenBtn.setOnClickListener(){
            val host = kitchenBtn.context as Activity
            if (categories[position].name != "Shopping List")
            {
                // go to inventory
                val intent = Intent(host, InventoryActivity::class.java)
                ServiceHandler.lastCategory = categories[position].name
                host.startActivity(intent)
            }
            else
            {
                // go to the shopping list
                val intent = Intent(host, ShoppingListActivity::class.java)
                ServiceHandler.lastCategory = categories[position].name
                host.startActivity(intent)
            }

        }

        return rowView
    }
}