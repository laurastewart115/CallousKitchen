/* Authors: Kevin Gadelha, Laura Stewart */

package com.example.callouskitchenandroid

import android.content.Intent
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.view.ViewGroup
import android.widget.Button
import android.widget.SearchView
import com.android.volley.Response
import kotlinx.android.synthetic.main.activity_kitchen_list.bottomNav
import kotlinx.android.synthetic.main.activity_recipe_search.*
import org.json.JSONObject

/**
 * Activity that allows the user to search for a recipe and generate recipes based on expired food.
 *
 * @author Kevin Gadelha, Laura Stewart
 */
class RecipeSearchActivity : AppCompatActivity() {

    /**
     * Called when the activity is created. Gets references to UI elements and sets
     * listeners for them.
     *
     * @param savedInstanceState Can be used to save application state
     * @author Kevin Gadelha (backend), Laura Stewart (UI)
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_recipe_search)

        // set up bottom nav bar
        setNavigation()

        val footerView = layoutInflater.inflate(R.layout.recipe_footer_view, listViewRecipe, false) as ViewGroup
        listViewRecipe.addFooterView(footerView)

        // Search for recipes by name
        val searchBar = findViewById<SearchView>(R.id.searchViewRecipes)

        searchBar.setOnQueryTextListener(object : SearchView.OnQueryTextListener {
            override fun onQueryTextSubmit(query: String): Boolean {
                //Searches when the user clicks enter
                //Kind of finicky, just like all of android
                searchRecipes(query)
                return false
            }
            override fun onQueryTextChange(newText: String): Boolean {
                return false
            }
        })

        // Get recipe suggestions based on expiring food
        val btnGetSuggestions = findViewById<Button>(R.id.btnGetRecipeSuggestions)

        btnGetSuggestions.setOnClickListener {
            ServiceHandler.callAccountService(
                "FeelingLuckyUser", hashMapOf(
                    //Call more recipes for better results
                    "count" to 100,
                    "userId" to ServiceHandler.userId
                ), this,
                Response.Listener { response ->
                    val json = JSONObject(response.toString())
                    val recipesArray = json.optJSONArray("FeelingLuckyUserResult")
                    var recipes: ArrayList<Recipe> = arrayListOf<Recipe>()
                    for (i in 0 until (recipesArray?.length() ?: 0)) {
                        val recipe = recipesArray[i]!! as JSONObject
                        val name = recipe.optString("Name")
                        val url = recipe.optString("Url")
                        val source = recipe.optString("Source")
                        val image = recipe.optString("Image")
                        val recipeYield = recipe.optDouble("Yield")
                        recipes.add(Recipe(name,url,source,image,recipeYield))
                    }
                    val recipeListAdapter = RecipeListAdapter(this, recipes)
                    listViewRecipe.adapter = recipeListAdapter
                })
        }
    }

    /**
     * Search for recipes
     *
     * @param query The query to search for recipes with
     * @author Kevin Gadelha
     */
    private fun searchRecipes(query: String)
    {
        ServiceHandler.callAccountService(
            "SearchRecipesUser", hashMapOf(
                "search" to query,
                "count" to 100,
                "userId" to ServiceHandler.userId
            ), this,
            Response.Listener { response ->
                val json = JSONObject(response.toString())
                val recipesArray = json.optJSONArray("SearchRecipesUserResult")
                var recipes: ArrayList<Recipe> = arrayListOf<Recipe>()
                for (i in 0 until (recipesArray?.length() ?: 0)) {
                    val recipe = recipesArray[i]!! as JSONObject
                    val name = recipe.optString("Name")
                    val url = recipe.optString("Url")
                    val source = recipe.optString("Source")
                    val image = recipe.optString("Image")
                    val recipeYield = recipe.optDouble("Yield")
                    recipes.add(Recipe(name,url,source,image,recipeYield))
                }
                val recipeListAdapter = RecipeListAdapter(this, recipes)
                listViewRecipe.adapter = recipeListAdapter
            })
    }

    /**
     * Override the default back button press so that it does nothing
     *
     * @author Laura Stewart
     */
    override fun onBackPressed() {
        // do nothing for now
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
                    // stay in recipes
                    true
                }
                R.id.navigation_inventory -> {
                    // go to the categories list
                    val intent = Intent(this@RecipeSearchActivity, CategoryListActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_settings -> {
                    // go to settings
                    val intent = Intent(this@RecipeSearchActivity, SettingsActivity::class.java)
                    startActivity(intent)
                    true
                }
                else -> false
            }
        }
    }
}