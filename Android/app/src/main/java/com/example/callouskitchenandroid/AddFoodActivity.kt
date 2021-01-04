/* Authors: Kevin Gadelha, Laura Stewart */
package com.example.callouskitchenandroid

import android.app.DatePickerDialog
import android.content.Intent
import android.os.Bundle
import android.widget.*
import androidx.appcompat.app.AppCompatActivity
import com.android.volley.Response
import kotlinx.android.synthetic.main.activity_kitchen_list.*
import org.json.JSONArray
import org.json.JSONObject
import java.text.SimpleDateFormat
import java.time.LocalDate
import java.time.format.DateTimeFormatter
import java.time.format.DateTimeParseException
import java.util.*

/**
 * Activity for adding a food to the inventory.
 *
 * @author Kevin Gadelha, Laura Stewart
 */
class AddFoodActivity : AppCompatActivity() {

    /**
     * Stores a tag with this activity's name
     */
    companion object {
        private const val ADD_FOOD_TAG = "AddFoodActivity"
    }

    /**
     * Called when the activity is created. Gets references to UI elements and sets
     * listeners for them.
     *
     * @param savedInstanceState Can be used to save application state
     * @author Kevin Gadelha (backend), Laura Stewart (UI)
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_add_food)

        // set up bottom nav bar
        setNavigation()

        val txtFoodName = findViewById<EditText>(R.id.editAddFoodName)
        val txtFoodQuantity = findViewById<EditText>(R.id.editAddFoodQuantity)
        val txtFoodExpiry = findViewById<EditText>(R.id.editTextAddFoodExpiry)
        val btnAddFood = findViewById<Button>(R.id.btnAddFoodItem)
        val btnCancel = findViewById<Button>(R.id.btnCancelAddFood)
        val btnScanBarcode = findViewById<Button>(R.id.btnScanBarcode)
        val spinnerUnits = findViewById<Spinner>(R.id.spinnerUnits)
        val spinnerCategory = findViewById<Spinner>(R.id.spinnerCategory)

        // Populate the Units spinner with custom adapter
        val unitsArray = resources.getStringArray(R.array.units)
        val adapter = ArrayAdapter(this, R.layout.custom_spinner_item, unitsArray)
        spinnerUnits.adapter = adapter

        // Populate the category spinner
        var categories = listOf("Fridge", "Freezer", "Pantry", "Cupboard", "Cellar", "Other")
        val categoryAdapter = ArrayAdapter(this, R.layout.custom_spinner_item, categories)
        spinnerCategory.adapter = categoryAdapter

        //Default is fridge
        var category = "Fridge"
        //The category should be based on what the user's last selected category was
        //But all doesn't count of course, in which case use the default
        if (ServiceHandler.lastCategory != "All")
            category = ServiceHandler.lastCategory
        //Set the category in the UI
        spinnerCategory.setSelection(categories.indexOf(category))

        // variables for storing food data
        var cal = Calendar.getInstance()
        var foodName: String?
        var quantity: Double?
        var quantityClassifier: String?
        var expiryDate: String?
        var vegan: Int? = -1
        var vegetarian: Int? = -1
        var ingredients = arrayOf<String>()
        var traces = arrayOf<String>()

        // Get the food data passed in from the barcode scanner
        if (intent != null) {
            try {
                foodName = intent.getStringExtra("FOODNAME")
                txtFoodName.setText(foodName)
            } catch (exc: Exception) {
            }

            try {
                quantity = intent.getDoubleExtra("QUANTITY", 0.0)
                txtFoodQuantity.setText(quantity.toString())
            } catch (exc: Exception) {
            }

            try {
                quantityClassifier = intent.getStringExtra("QUANTITYCLASSIFIER")
                spinnerUnits.setSelection(
                    (spinnerUnits.getAdapter() as ArrayAdapter<String?>).getPosition(
                        quantityClassifier
                    )
                )
            } catch (exc: Exception) {
            }

            try {
                expiryDate = intent.getStringExtra("EXPIRY")
                txtFoodExpiry.setText(expiryDate)
            } catch (exc: Exception) {
            }

            try {
                //Default to unknown
                vegan = intent.getIntExtra("VEGAN", -1)
            } catch (exc: Exception) {
            }

            try {
                vegetarian = intent.getIntExtra("VEGETARIAN", -1)
            } catch (exc: Exception) {
            }

            try {
                ingredients = intent.getStringArrayExtra("INGREDIENTS")
            } catch (exc: Exception) {
            }

            try {
                traces = intent.getStringArrayExtra("TRACES")
            } catch (exc: Exception) {
            }

            // Show allergy and dietary restriction warnings
            var tempFood = Food(ingredients,traces,vegan,vegetarian)
            //Tell the user if the food has bad stuff or if it doesn't
            var warningMessage = ServiceHandler.generateWarningMessage(tempFood, false)
            if (!warningMessage.isNullOrEmpty())
                Toast.makeText(
                    applicationContext,
                    warningMessage,
                    Toast.LENGTH_LONG
                ).show()
        }

        // Detect when the expiry date is set in the calendar
        val dateSetListener = object : DatePickerDialog.OnDateSetListener {
            override fun onDateSet(
                view: DatePicker, year: Int, monthOfYear: Int,
                dayOfMonth: Int
            ) {
                cal.set(Calendar.YEAR, year)
                cal.set(Calendar.MONTH, monthOfYear)
                cal.set(Calendar.DAY_OF_MONTH, dayOfMonth)
                val myFormat = "MM/dd/yyyy" // mention the format you need
                val sdf = SimpleDateFormat(myFormat, Locale.US)
                txtFoodExpiry.setText(sdf.format(cal.getTime()))
            }
        }

        // Go to the barcode scanner
        btnScanBarcode.setOnClickListener {
            val intent = Intent(this@AddFoodActivity, activity_barcode_scan::class.java)
            startActivity(intent)
        }

        // Add the food (if all required variable are valid
        btnAddFood.setOnClickListener {
            val foodName = txtFoodName.text.toString()

            // Validate input to check that it's good
            if (foodName.isNullOrEmpty()) {
                Toast.makeText(applicationContext, "Please enter the food name", Toast.LENGTH_LONG)
                    .show()
            } else {
                val quantityString = txtFoodQuantity.text.toString()
                var quantity: Double = 1.0
                if (!quantityString.isNullOrEmpty())
                    quantity = quantityString.toDouble()
                var expiryDate: LocalDate?
                if (txtFoodExpiry.text.isNullOrEmpty()){
                    expiryDate = null
                }
                else{
                    try {
                        expiryDate = LocalDate.parse(
                            txtFoodExpiry.text.toString(),
                            DateTimeFormatter.ofPattern("MM/dd/yyyy")
                        )
                    } catch (e: DateTimeParseException) {
                        Toast.makeText(
                            applicationContext,
                            "Please enter a valid date",
                            Toast.LENGTH_LONG
                        ).show()
                        return@setOnClickListener
                    }
                }

                // Add the food to the database (will only be called if required fields have been filled)
                val quantityClassifier = spinnerUnits.selectedItem.toString()
                ServiceHandler.callAccountService(
                    "AddFoodComplete", hashMapOf(
                        "userId" to ServiceHandler.userId,
                        "kitchenId" to ServiceHandler.primaryKitchenId,
                        "name" to foodName,
                        "storage" to spinnerCategory.selectedItem.toString(),
                        "quantity" to quantity,
                        "quantityClassifier" to quantityClassifier,
                        "expiryDate" to ServiceHandler.serializeDate(expiryDate),
                        "vegan" to vegan,
                        "vegetarian" to vegetarian,
                        "calories" to -1,
                        "ingredients" to JSONArray(ingredients),
                        "traces" to JSONArray(traces),
                        "favourite" to false
                    ), this,
                    Response.Listener { response ->
                        val json = JSONObject(response.toString())

                        val success = json.getBoolean("AddFoodCompleteResult")
                        //If it failed, it's cause the user's not confirmed yet
                        if (!success){
                            Toast.makeText(applicationContext,"Please confirm your email before adding food!", Toast.LENGTH_LONG).show()
                        }
                        else{
                            // return to the food list
                            val intent = Intent(this@AddFoodActivity, InventoryActivity::class.java)
                            startActivity(intent)
                        }

                    })
            }
        }

        // Return to the inventory activity
        btnCancel.setOnClickListener {
            val intent = Intent(this@AddFoodActivity, InventoryActivity::class.java)
            startActivity(intent)
        }

        // Open a date picker to the correct month, day, and year
        txtFoodExpiry.setOnFocusChangeListener { v, hasFocus ->
            DatePickerDialog(
                this@AddFoodActivity,
                dateSetListener,
                // set DatePickerDialog to point to today's date when it loads up
                cal.get(Calendar.YEAR),
                cal.get(Calendar.MONTH),
                cal.get(Calendar.DAY_OF_MONTH)
            ).show()
        }

    }

    /**
     * Override the default back button press so that it always goes to the inventory
     *
     * @author Laura Stewart
     */
    override fun onBackPressed() {
        val intent = Intent(this@AddFoodActivity, InventoryActivity::class.java)
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
                    val intent = Intent(this@AddFoodActivity, RecipeSearchActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_inventory -> {
                    // go to the categories list
                    val intent = Intent(this@AddFoodActivity, CategoryListActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_settings -> {
                    // go to settings
                    val intent = Intent(this@AddFoodActivity, SettingsActivity::class.java)
                    startActivity(intent)
                    true
                }
                else -> false
            }
        }
    }
}
