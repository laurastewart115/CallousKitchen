/* Authors: Kevin Gadelha, Laura Stewart */
package com.example.callouskitchenandroid

import android.app.NotificationChannel
import android.content.Intent
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.view.ViewGroup
import kotlinx.android.synthetic.main.activity_inventory.*
import kotlinx.android.synthetic.main.activity_kitchen_list.*
import android.app.NotificationManager
import android.content.Context
import android.graphics.Color
import android.os.Build

/**
 * Activity for displaying a list of all categories that can be clicked to go to the inventory activity.
 *
 * @author Kevin Gadelha, Laura Stewart
 */
class CategoryListActivity : AppCompatActivity() {

    //declaring variables
    lateinit var notificationManager : NotificationManager

    /**
     * Called when the activity is created. Gets references to UI elements and sets
     * listeners for them.
     *
     * @param savedInstanceState Can be used to save application state
     * @author Kevin Gadelha (backend), Laura Stewart (UI)
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_kitchen_list)

        // set the bottom nav bar
        setNavigation()

        // make the hard coded categories cause some of them are unique
        var categories: ArrayList<Category> = arrayListOf<Category>()
        categories.add(Category(0,"All"))
        categories.add(Category(1,"Fridge"))
        categories.add(Category(2,"Freezer"))
        categories.add(Category(3,"Pantry"))
        categories.add(Category(4,"Cupboard"))
        categories.add(Category(5,"Cellar"))
        categories.add(Category(6,"Other"))
        categories.add(Category(7, "Shopping List"))

        // display the category list
        val categoryListAdapter = CategoryListAdapter(this, categories)
        val footerView = layoutInflater.inflate(R.layout.footer_view, listView, false) as ViewGroup
        listView.addFooterView(footerView)
        listView.adapter = categoryListAdapter
    }


    /**
     * Override the default back button press so that it does nothing
     *
     * @author Laura Stewart
     */
    override fun onBackPressed() {
        // stay on the same activity
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
                    val intent = Intent(this@CategoryListActivity, RecipeSearchActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_inventory -> {
                    // stay in the inventory
                    true
                }
                R.id.navigation_settings -> {
                    // go to settings
                    val intent = Intent(this@CategoryListActivity, SettingsActivity::class.java)
                    startActivity(intent)
                    true
                }
                else -> false
            }
        }
    }
}
