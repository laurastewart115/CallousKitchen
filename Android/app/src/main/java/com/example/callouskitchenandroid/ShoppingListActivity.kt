/* Authors: Kevin Gadelha, Laura Stewart */
package com.example.callouskitchenandroid

import android.content.Intent
import android.content.SharedPreferences
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.text.Editable
import android.text.TextWatcher
import android.view.View
import android.view.ViewGroup
import android.widget.*
import com.android.volley.Response
import kotlinx.android.synthetic.main.activity_inventory.*
import kotlinx.android.synthetic.main.activity_recipe_search.*
import kotlinx.android.synthetic.main.activity_shopping_list.*
import org.json.JSONObject

/**
 * Activity that displays the shopping list
 * Similar to inventory but with UI
 *
 * @author Kevin Gadelha (backend), Laura Stewart (UI)
 */
class ShoppingListActivity : AppCompatActivity() {

    // local storage
    private val sharedPref: SharedPreferences = ServiceHandler.sharedPref

    // The user's foods
    var foods: ArrayList<Food> = arrayListOf<Food>()

    /**
     * Called when the activity is created. Gets references to UI elements and sets
     * listeners for them.
     *
     * @param savedInstanceState Can be used to save application state
     * @author Kevin Gadelha (backend), Laura Stewart (UI)
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        // set the title of the activity
        title = ServiceHandler.lastCategory
        setContentView(R.layout.activity_shopping_list)
        // set up bottom nav bar
        setNavigation()

        // set the footer for the list of food
        val footerView = layoutInflater.inflate(R.layout.footer_view, listViewShoppingList, false) as ViewGroup
        listViewShoppingList.addFooterView(footerView)

        // get all of the food from the inventory
        ServiceHandler.callAccountService(
            "GetInventory",hashMapOf("kitchenId" to ServiceHandler.primaryKitchenId),this,
            Response.Listener { response ->

                val json = JSONObject(response.toString())
                val foodsJson = json.getJSONArray("GetInventoryResult")
                for (i in 0 until foodsJson.length()) {
                    var foodJson: JSONObject = foodsJson.getJSONObject(i)
                        var food = Food(foodJson.getInt("Id"),foodJson.getString("Name"))
                        food.quantity = foodJson.getDouble("Quantity")
                        food.quantityClassifier = foodJson.getString("QuantityClassifier")
                        food.expiryDate = ServiceHandler.deSerializeDate(foodJson.getString("ExpiryDate"))
                        food.favourite = foodJson.getBoolean("Favourite")
                        food.onShoppingList = foodJson.getBoolean("OnShoppingList")
                        foods.add(food)

                }

                updateSortedAndFilteredList()

            })

        // Clear all the checkboxes
        val btnClear = findViewById<Button>(R.id.btnClearChecked)

        btnClear.setOnClickListener {
            ServiceHandler.callAccountService(
                "ClearShoppingList",hashMapOf("kitchenId" to ServiceHandler.primaryKitchenId),this,
                Response.Listener { response ->

                    val json = JSONObject(response.toString())
                    val success = json.getBoolean("ClearShoppingListResult")
                    if (success){
                        // reset variables locally because calling the service was sometimes crashing the app
                        for (food in foods) {
                            food.onShoppingList = false
                        }
                        updateSortedAndFilteredList()
                    }
                    else {
                        Toast.makeText(applicationContext,"Could not clear list", Toast.LENGTH_LONG).show()
                    }
                })
        }

        // Searching and sorting UI elements
        val txtSearchShopping = findViewById<EditText>(R.id.searchShoppingList)
        val spinnerSort = findViewById<Spinner>(R.id.spinnerSortShopping)

        // Populate the Sorting spinner
        val sortingArray = resources.getStringArray(R.array.sortingOptionsShopping)
        val adapter = ArrayAdapter(this, R.layout.custom_spinner_item, sortingArray)
        spinnerSort.adapter = adapter
        spinnerSort.setSelection(sharedPref.getInt("lastIndexShopping", 0))
        txtSearchShopping.setText(sharedPref.getString("lastSearchShopping", ""))

        // detect value change in the search bar and update the filtering
        txtSearchShopping.addTextChangedListener(object : TextWatcher {

            override fun afterTextChanged(s: Editable) {}

            override fun beforeTextChanged(s: CharSequence, start: Int,
                                           count: Int, after: Int) {
            }

            override fun onTextChanged(s: CharSequence, start: Int,
                                       before: Int, count: Int) {
                with(sharedPref.edit()) {
                    putString("lastSearchShopping", s.toString())
                    apply()
                }
                updateSortedAndFilteredList()
            }
        })

        // detect selection change in the spinner and sort the list
        spinnerSort.onItemSelectedListener = object : AdapterView.OnItemSelectedListener {
            override fun onItemSelected(
                parent: AdapterView<*>,
                view: View,
                position: Int,
                id: Long
            ) {
                with(sharedPref.edit()) {
                    putInt("lastIndexShopping", position)
                    apply()
                }
                updateSortedAndFilteredList()
            }

            override fun onNothingSelected(parent: AdapterView<*>?) {
                // another interface callback
            }
        }

    }

    /**
     * Update the filtering and sorting on the list of food depending on the values in the
     * search bar and the sort dropdown.
     *
     * @author Kevin Gadelha, modified by Laura Stewart
     */
    private fun updateSortedAndFilteredList(){
        // get references to UI
        val txtSearchShopping = findViewById<EditText>(R.id.searchShoppingList)
        val spinnerSort = findViewById<Spinner>(R.id.spinnerSortShopping)

        // update the sorting
        var sort = spinnerSort.selectedItem.toString()
        when (sort) {
            //Default sort for shopping list is favorited items first sorted by lowest quantity
            "Default" -> {
                //Sort by lowest quantity first
                foods = ArrayList(foods.sortedWith(compareBy({ it.quantity })))
                //Then make sure to show all the favourited foods first
                foods = ArrayList(foods.sortedWith(compareByDescending({ it.favourite })))
            }
            "Checked" -> foods = ArrayList(foods.sortedWith(compareByDescending({ it.onShoppingList })))
            "Recently Added" -> foods = ArrayList(foods.sortedWith(compareByDescending ({ it.id })))
            "Expiring Soon" -> foods = ArrayList(foods.sortedWith(Comparator<Food>{ a, b ->
                when {
                    a.expiryDate == null && b.expiryDate != null -> 1
                    a.expiryDate != null && b.expiryDate == null -> -1
                    a.expiryDate == null && b.expiryDate == null -> 0
                    a.expiryDate!! > b.expiryDate!! -> 1
                    a.expiryDate!! < b.expiryDate!! -> -1
                    else -> 0
                }
            }))
            "Running Low" -> foods = ArrayList(foods.sortedWith(compareBy({ it.quantity })))
            "Favourited" -> foods = ArrayList(foods.sortedWith(compareByDescending({ it.favourite })))
            "Oldest Added" -> foods = ArrayList(foods.sortedWith(compareBy({ it.id })))
            "Alphabetical" -> foods = ArrayList(foods.sortedWith(compareBy({ it.name })))
            "Greatest Quantity" -> foods = ArrayList(foods.sortedWith(compareByDescending ({ it.quantity })))
            "Quantity Type" -> foods = ArrayList(foods.sortedWith(compareBy({ it.quantityClassifier })))
        }

        // Update the filtering based on the search
        if (txtSearchShopping.text.isNotEmpty()){
            val filteredFoods = foods.filter { food -> food.name.contains(txtSearchShopping.text)  }
            val shoppingListAdapter = ShoppingListAdapter(this@ShoppingListActivity, filteredFoods)
            listViewShoppingList.adapter = shoppingListAdapter
        }
        else{
            val shoppingListAdapter = ShoppingListAdapter(this@ShoppingListActivity, foods)
            listViewShoppingList.adapter = shoppingListAdapter
        }
    }

    /**
     * Override the default back button press so that it goes back to the inventory
     *
     * @author Laura Stewart
     */
    override fun onBackPressed() {
        // go back to the category list
        val intent = Intent(this@ShoppingListActivity, CategoryListActivity::class.java)
        startActivity(intent)
    }

    /**
     * Sets the Activities the buttons on the bottom navigation bar will go to
     *
     * @author Laura Stewart
     */
    private fun setNavigation() {
        bottomNavShoppingList.setOnNavigationItemSelectedListener {
            when (it.itemId){
                R.id.navigation_recipes -> {
                    // go to recipes
                    val intent = Intent(this@ShoppingListActivity, RecipeSearchActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_inventory -> {
                    // go to the categories list
                    val intent = Intent(this@ShoppingListActivity, CategoryListActivity::class.java)
                    startActivity(intent)
                    true
                }
                R.id.navigation_settings -> {
                    // go to settings
                    val intent = Intent(this@ShoppingListActivity, SettingsActivity::class.java)
                    startActivity(intent)
                    true
                }
                else -> false
            }
        }
    }
}