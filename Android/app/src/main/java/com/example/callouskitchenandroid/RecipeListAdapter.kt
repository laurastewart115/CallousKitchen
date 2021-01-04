/* Author: Laura Stewart */
package com.example.callouskitchenandroid

import android.app.Activity
import android.content.Intent
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import android.widget.ImageView
import android.widget.TextView
import com.squareup.picasso.Picasso

/**
 * Extends the ArrayAdapter class so that the recipes can be displayed in a custom ListView.
 *
 * @param context The current context of the activity
 * @param recipes A list of Recipe objects to be displayed
 * @author Laura Stewart
 */
class RecipeListAdapter (private val context: Activity,
                         private val recipes: List<Recipe>)
    : ArrayAdapter<Recipe>(context, R.layout.recipe_item_list, recipes) {

    /**
     * Called when the list item is being created.
     *
     * @param position Index of the current element in the list
     * @param convertView
     * @param parent
     */
    override fun getView(position: Int, convertView: View?, parent: ViewGroup): View {
        val inflater = context.layoutInflater
        val rowView = inflater.inflate(R.layout.recipe_item_list, null, true)

        // get UI elements
        val txtRecipeTitle = rowView.findViewById<TextView>(R.id.textViewRecipeTitle)
        val txtRecipeYield = rowView.findViewById<TextView>(R.id.textViewRecipeYield)
        val txtRecipeSource = rowView.findViewById<TextView>(R.id.textViewRecipeSource)
        val imgRecipe  = rowView.findViewById<ImageView>(R.id.imageViewRecipe)

        // Set the content of the UI elements
        txtRecipeTitle.text = recipes[position].name
        txtRecipeYield.text = "Servings: ${recipes[position].recipeYield}"
        txtRecipeSource.text = recipes[position].source

        // use Picasso to load the image into the imageview
        val url = recipes[position].image
        if (!url.isNullOrEmpty()) {
            Picasso.get().load(url).into(imgRecipe)
        }

        // When the list item is clicked, go the the recipe
        rowView.setOnClickListener {
            // open recipe activity
            val host = rowView.context as Activity
            val intent = Intent(host, RecipeViewActivity::class.java)
            intent.putExtra("URL", recipes[position].url)
            host.startActivity(intent)

        }

        return rowView
    }
}