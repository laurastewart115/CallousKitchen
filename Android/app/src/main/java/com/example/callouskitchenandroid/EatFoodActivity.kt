/* Authors: Kevin Gadelha, Laura Stewart */
package com.example.callouskitchenandroid

import android.content.Intent
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.widget.*
import com.android.volley.Response
import kotlinx.android.synthetic.main.activity_kitchen_list.*
import org.json.JSONObject
import java.text.DecimalFormat
import java.util.*

/**
 * Activity for eating a food item. The quantity eaten will vary.
 *
 * @author Kevin Gadelha, Laura Stewart
 */
class EatFoodActivity : AppCompatActivity() {

    // The number of steps in the food quantity slider
    private val SLIDER_MAX = 10

    /**
     * Called when the activity is created. Gets references to UI elements and sets
     * listeners for them.
     *
     * @param savedInstanceState Can be used to save application state
     * @author Kevin Gadelha (backend), Laura Stewart (UI)
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_eat_food)

        // set up bottom nav bar
        setNavigation()

        val txtFoodName = findViewById<TextView>(R.id.textViewFoodTitleEat)
        val btnEatFood = findViewById<Button>(R.id.btnEatFoodItem)
        val btnCancel = findViewById<Button>(R.id.btnCancelEatFood)

        // Slider and text representation
        val seekBarQuantity = findViewById<SeekBar>(R.id.seekBarEatFood)
        val txtViewQuantity = findViewById<TextView>(R.id.textViewQuantity)

        // populate the fields
        val food = intent.getSerializableExtra("FOOD") as Food
        //Only warn the user if it's something important
        var warningMessage = ServiceHandler.generateWarningMessage(food,true)
        if (!warningMessage.isNullOrEmpty())
            Toast.makeText(
                applicationContext,
                warningMessage,
                Toast.LENGTH_LONG
            ).show()
        txtFoodName.text = food.name

        var units = food.quantityClassifier

        // Set the seekbar max to 10 so there will be 10 "steps" in the bar
        // food quantity will be calculated using percentages
        seekBarQuantity.max = SLIDER_MAX
        seekBarQuantity.progress = SLIDER_MAX

        val dec = DecimalFormat("#,###.##")
        val formattedQuantity = dec.format(food.quantity)

        txtViewQuantity.text = "${formattedQuantity} $units"

        // detect changes in seek bar value
        seekBarQuantity.setOnSeekBarChangeListener(object : SeekBar.OnSeekBarChangeListener {
            override fun onProgressChanged(seekBar: SeekBar, progress: Int, fromUser: Boolean) {
                // convert seekbar's current value to a percent
                val percent = progress.toDouble() / SLIDER_MAX.toDouble()

                // calculate the amount of food remaining
                val remainingQuantity = food.quantity * percent
                val formattedRemQuantity = dec.format(remainingQuantity)

                txtViewQuantity.text = "$formattedRemQuantity $units"
            }
            override fun onStartTrackingTouch(seekBar: SeekBar) {}
            override fun onStopTrackingTouch(seekBar: SeekBar) {}
        })

        btnEatFood.setOnClickListener{

            // calculate the amount of food remaining
            val remainingQuantity = food.quantity * (seekBarQuantity.progress.toDouble() / SLIDER_MAX.toDouble())

            val quantityString = remainingQuantity.toString()
            if (!quantityString.isNullOrEmpty())
            {
                val quantity = quantityString.toDouble()

                ServiceHandler.callAccountService(
                    "EatFood",hashMapOf("id" to food.id, "quantity" to quantity),this,
                    Response.Listener { response ->

                        val json = JSONObject(response.toString())
                        val success = json.getBoolean("EatFoodResult")
                        if (!success){
                            Toast.makeText(applicationContext,"Failed :(", Toast.LENGTH_LONG).show()
                        }

                        val intent = Intent(this@EatFoodActivity, InventoryActivity::class.java)
                        startActivity(intent)
                    })
            }
            else
            {
                Toast.makeText(applicationContext,"Please enter a quantity", Toast.LENGTH_LONG).show()
            }

        }

        btnCancel.setOnClickListener{
            val intent = Intent(this@EatFoodActivity, InventoryActivity::class.java)
            startActivity(intent)
        }
    }

    /**
     * Override the default back button press so that it always goes to the inventory
     *
     * @author Laura Stewart
     */
    override fun onBackPressed() {
        // do nothing for now
        val intent = Intent(this@EatFoodActivity, InventoryActivity::class.java)
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
                    val intent = Intent(this@EatFoodActivity, RecipeSearchActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_inventory -> {
                    // go to the categories list
                    val intent = Intent(this@EatFoodActivity, CategoryListActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_settings -> {
                    // go to settings
                    val intent = Intent(this@EatFoodActivity, SettingsActivity::class.java)
                    startActivity(intent)
                    true
                }
                else -> false
            }
        }
    }
}
