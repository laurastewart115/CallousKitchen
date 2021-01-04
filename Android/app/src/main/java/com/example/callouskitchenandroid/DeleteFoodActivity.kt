/* Authors: Kevin Gadelha, Laura Stewart */

package com.example.callouskitchenandroid

import android.content.Intent
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.widget.*
import com.android.volley.Response
import kotlinx.android.synthetic.main.activity_kitchen_list.*
import org.json.JSONObject

/**
 * Activity for deleting a food item. This deletes the food item
 *
 * @author Kevin Gadelha, Laura Stewart
 */
class DeleteFoodActivity : AppCompatActivity() {

    /**
     * Called when the activity is created. Gets references to UI elements and sets
     * listeners for them.
     *
     * @param savedInstanceState Can be used to save application state
     * @author Kevin Gadelha (backend), Laura Stewart (UI)
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_delete_food)

        // set up bottom nav bar
        setNavigation()

        val txtFoodName = findViewById<TextView>(R.id.textViewFoodTitleDelete)
        val btnDeleteFood = findViewById<Button>(R.id.btnDeleteFoodItem)
        val btnCancel = findViewById<Button>(R.id.btnCancelDeleteFood)

        // populate the field
        val food = intent.getSerializableExtra("FOOD") as Food

        txtFoodName.text = food.name

        // Delete the food from the database
        btnDeleteFood.setOnClickListener{
                ServiceHandler.callAccountService(
                    "RemoveItem",hashMapOf("id" to food.id),this,
                    Response.Listener { response ->

                        val json = JSONObject(response.toString())
                        val success = json.getBoolean("RemoveItemResult")
                        if (!success){
                            Toast.makeText(applicationContext,"Failed :(", Toast.LENGTH_LONG).show()
                        }

                        val intent = Intent(this@DeleteFoodActivity, InventoryActivity::class.java)
                        startActivity(intent)
                    })

        }

        // Return to the inventory
        btnCancel.setOnClickListener{
            val intent = Intent(this@DeleteFoodActivity, InventoryActivity::class.java)
            startActivity(intent)
        }
    }

    /**
     * Override the default back button press so that it always goes to the inventory
     *
     * @author Laura Stewart
     */
    override fun onBackPressed() {
        val intent = Intent(this@DeleteFoodActivity, InventoryActivity::class.java)
        startActivity(intent)
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
                    val intent = Intent(this@DeleteFoodActivity, RecipeSearchActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_inventory -> {
                    // go to the categories list
                    val intent = Intent(this@DeleteFoodActivity, CategoryListActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_settings -> {
                    // go to settings
                    val intent = Intent(this@DeleteFoodActivity, SettingsActivity::class.java)
                    startActivity(intent)
                    true
                }
                else -> false
            }
        }
    }
}
