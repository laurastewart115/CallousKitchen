/* Authors: Kevin Gadelha, Laura Stewart */
package com.example.callouskitchenandroid

import android.app.DatePickerDialog
import android.content.Intent
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.widget.*
import com.android.volley.Response
import kotlinx.android.synthetic.main.activity_edit_food.*
import kotlinx.android.synthetic.main.activity_kitchen_list.*
import kotlinx.android.synthetic.main.activity_kitchen_list.bottomNav
import org.json.JSONObject
import java.text.DecimalFormat
import java.text.SimpleDateFormat
import java.time.LocalDate
import java.time.format.DateTimeFormatter
import java.time.format.DateTimeParseException
import java.util.*

/**
 * Activity for editting a food (name, quantity, expiry date, quantity classifier, etc.).
 * Intended to be used for correcting errors made when adding food.
 *
 * @author Kevin Gadelha, Laura Stewart
 */
class EditFoodActivity : AppCompatActivity() {

    /**
     * Called when the activity is created. Gets references to UI elements and sets
     * listeners for them.
     *
     * @param savedInstanceState Can be used to save application state
     * @author Kevin Gadelha (backend), Laura Stewart (UI)
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_edit_food)

        // set up bottom nav bar
        setNavigation()

        val txtFoodName = findViewById<EditText>(R.id.editFoodName)
        val txtFoodQuantity = findViewById<EditText>(R.id.editFoodQuantity)
        val txtFoodExpiry = findViewById<EditText>(R.id.editFoodExpiry)
        val btnEditFood = findViewById<Button>(R.id.btnEditFoodItem)
        val btnCancel = findViewById<Button>(R.id.btnCancelEditFood)
        val spinnerUnits = findViewById<Spinner>(R.id.spinnerUnits)
        val spinnerCategories = findViewById<Spinner>(R.id.spinnerCategory)

        // populate the categories spinner
        var categories = listOf("Fridge", "Freezer", "Pantry", "Cupboard", "Cellar", "Other")
        val categoryAdapter = ArrayAdapter(this, R.layout.custom_spinner_item, categories)
        spinnerCategories.adapter = categoryAdapter

        // Populate the Units spinner
        val unitsArray = resources.getStringArray(R.array.units)
        val adapter = ArrayAdapter(this, R.layout.custom_spinner_item, unitsArray)
        spinnerUnits.adapter = adapter

        // populate the fields
        val food = intent.getSerializableExtra("FOOD") as Food

        txtFoodName.setText(food.name)
        val dec = DecimalFormat("#,###.##")
        val formattedQuantity = dec.format(food.quantity)
        txtFoodQuantity.setText(formattedQuantity)

        spinnerUnits.setSelection(unitsArray.indexOf(food.quantityClassifier))
        spinnerCategories.setSelection(categories.indexOf(food.storage))

        var cal = Calendar.getInstance()
        if (food.expiryDate != null){
            // Set date this way to make sure the month index is right
            cal.set(Calendar.YEAR, food.expiryDate?.year!!)
            cal.set(Calendar.MONTH, food.expiryDate?.monthValue!! - 1)
            cal.set(Calendar.DAY_OF_MONTH, food.expiryDate?.dayOfMonth!!)
            val myFormat = "MM/dd/yyyy"
            val sdf = SimpleDateFormat(myFormat, Locale.US)
            txtFoodExpiry.setText(sdf.format(cal.time))
        }

        // When the date is changed in the datepicker, update the expiry date field
        val dateSetListener = object : DatePickerDialog.OnDateSetListener {
            override fun onDateSet(view: DatePicker, year: Int, monthOfYear: Int,
                                   dayOfMonth: Int) {
                cal.set(Calendar.YEAR, year)
                cal.set(Calendar.MONTH, monthOfYear)
                cal.set(Calendar.DAY_OF_MONTH, dayOfMonth)
                val myFormat = "MM/dd/yyyy" // mention the format you need
                val sdf = SimpleDateFormat(myFormat, Locale.US)
                txtFoodExpiry.setText(sdf.format(cal.getTime()))
            }
        }

        // The edit text must not be focusable (see xml file) for this to work
        editFoodExpiry.setOnClickListener{
            DatePickerDialog(this@EditFoodActivity, dateSetListener,
                cal.get(Calendar.YEAR),
                cal.get(Calendar.MONTH),
                cal.get(Calendar.DAY_OF_MONTH)).show()
        }

        // Update the food in the database
        btnEditFood.setOnClickListener{
            val foodName = txtFoodName.text.toString()

            if (foodName.isNullOrEmpty())
            {
                Toast.makeText(applicationContext,"Please enter the food name", Toast.LENGTH_LONG).show()
            }
            else {
                val quantityString = txtFoodQuantity.text.toString()
                var quantity: Double = 1.0
                if (!quantityString.isNullOrEmpty())
                    quantity = quantityString.toDouble()

                var expiryDate : LocalDate? = null
                if (!txtFoodExpiry.text.isNullOrEmpty()){
                    try{
                        expiryDate = LocalDate.parse(txtFoodExpiry.text.toString(), DateTimeFormatter.ofPattern("MM/dd/yyyy"))
                    }
                    catch(e : DateTimeParseException){
                        Toast.makeText(applicationContext,"Please enter a valid date", Toast.LENGTH_LONG).show()
                        return@setOnClickListener
                    }
                }

                ServiceHandler.callAccountService(
                    "EditFood",hashMapOf("id" to food.id, "name" to foodName, "quantity" to quantity, "quantityClassifier" to spinnerUnits.selectedItem.toString(), "storage" to spinnerCategories.selectedItem.toString(), "expiryDate" to ServiceHandler.serializeDate(expiryDate)),this,
                    Response.Listener { response ->

                        val json = JSONObject(response.toString())
                        val success = json.getBoolean("EditFoodResult")
                        if (!success){
                            Toast.makeText(applicationContext,"Failed :(", Toast.LENGTH_LONG).show()
                        }

                        val intent = Intent(this@EditFoodActivity, InventoryActivity::class.java)
                        startActivity(intent)
                    })
            }
        }

        // Return to the inventory list
        btnCancel.setOnClickListener{
            val intent = Intent(this@EditFoodActivity, InventoryActivity::class.java)
            startActivity(intent)
        }

        // Open the date picker to the correct month, day, and year when the expiry date field is clicked
        txtFoodExpiry.setOnFocusChangeListener { v, hasFocus ->
            if (hasFocus){
                val year = food.expiryDate?.year
                val month = food.expiryDate?.monthValue
                //No idea why but the datepicker's month is off by one
                val day = food.expiryDate?.dayOfMonth
                DatePickerDialog(this@EditFoodActivity,
                    dateSetListener,
                    // set DatePickerDialog to point to today's date when it loads up
                    year ?: cal.get(Calendar.YEAR),
                    month ?: cal.get(Calendar.MONTH),
                    day ?: cal.get(Calendar.DAY_OF_MONTH)).show()
            }
        }
    }

    /**
     * Override the default back button press so that it always goes to the inventory
     *
     * @author Laura Stewart
     */
    override fun onBackPressed() {
        // do nothing for now
        val intent = Intent(this@EditFoodActivity, InventoryActivity::class.java)
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
                    val intent = Intent(this@EditFoodActivity, RecipeSearchActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_inventory -> {
                    // go to the categories list
                    val intent = Intent(this@EditFoodActivity, CategoryListActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_settings -> {
                    // go to settings
                    val intent = Intent(this@EditFoodActivity, SettingsActivity::class.java)
                    startActivity(intent)
                    true
                }
                else -> false
            }
        }
    }
}
