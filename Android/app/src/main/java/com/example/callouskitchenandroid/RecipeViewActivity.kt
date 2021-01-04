/* Author: Laura Stewart */
package com.example.callouskitchenandroid

import android.content.Intent
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.webkit.WebView
import kotlinx.android.synthetic.main.activity_kitchen_list.*

/**
 * Activity that displays a recipe in a web view
 *
 * @author Laura Stewart
 */
class RecipeViewActivity : AppCompatActivity() {

    /**
     * Called when the activity is created. Gets references to UI elements and sets
     * listeners for them.
     *
     * @param savedInstanceState Can be used to save application state
     * @author Laura Stewart
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_recipe_view)

        setNavigation()

        // Load the URL into the webview
        val recipeDisplay = findViewById<WebView>(R.id.webviewRecipe)

        if (intent.extras != null)
        {
            val extra = intent.extras
            if (intent.extras?.containsKey("URL")!!)
            {
                recipeDisplay.loadUrl(intent.extras?.getString("URL"))
            }
        }
    }

    /**
     * Sets the Activities the buttons on the bottom navigation bar will go to
     *
     * @author Laura Stewart
     */
    private fun setNavigation() {
        bottomNav.setOnNavigationItemSelectedListener {
            when (it.itemId){
                R.id.navigation_recipes -> {
                    // go to recipes
                    val intent = Intent(this@RecipeViewActivity, RecipeSearchActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_inventory -> {
                    // go to the categories list
                    val intent = Intent(this@RecipeViewActivity, CategoryListActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_settings -> {
                    // go to settings
                    val intent = Intent(this@RecipeViewActivity, SettingsActivity::class.java)
                    startActivity(intent)
                    true
                }
                else -> false
            }
        }
    }
}