/* Authors: Kevin Gadelha, Laura Stewart */
package com.example.callouskitchenandroid

import android.content.Intent
import androidx.appcompat.app.AppCompatActivity
import android.os.Bundle
import android.widget.Button
import android.widget.EditText
import android.widget.Toast
import com.android.volley.Response
import org.json.JSONObject

/**
 * Activity for creating an account
 *
 * @author Kevin Gadelha, Laura Stewart
 */
class CreateAccountActivity : AppCompatActivity() {

    /**
     * Called when the activity is created. Gets references to UI elements and sets
     * listeners for them.
     *
     * @param savedInstanceState Can be used to save application state
     * @author Kevin Gadelha (backend), Laura Stewart (UI)
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_create_account)

        // references to the UI
        val btnCreate = findViewById<Button>(R.id.btnConfirmCreateAccount)
        val btnCancel = findViewById<Button>(R.id.btnCancelCreateAccount)
        val txtUsername = findViewById<EditText>(R.id.editTextUsernameCreate)
        val txtPassword = findViewById<EditText>(R.id.editTextCreatePassword)
        val txtConfirmPassword  = findViewById<EditText>(R.id.editTextConfirmPassword)

        // Validates input and calls the service to create an account
        btnCreate.setOnClickListener{
            // client side validation
            // all fields full?
            if (!(txtUsername.text.isNullOrEmpty() || txtPassword.text.isNullOrEmpty() || txtConfirmPassword.text.isNullOrEmpty()))
            {
                // passwords match?
                if (txtPassword.text.toString() == txtConfirmPassword.text.toString())
                {

                    val username = txtUsername.text.toString()
                    val password = txtPassword.text.toString()

                    //Couldn't find a good way to hash passwords the same in C# as kotlin
                    ServiceHandler.callAccountService(
                        "CreateAccount",hashMapOf("email" to username,"pass" to password),this,
                        Response.Listener { response ->
                            val json = JSONObject(response.toString())
                            val user = json.getJSONObject("CreateAccountResult")
                            val userId = user.getInt("Id")


                            if (userId != -1){
                                ServiceHandler.userId = userId
                                ServiceHandler.email = user.getString("Email")
                                //Some of this stuff will be the default since new user
                                //But better safe than sorry
                                ServiceHandler.vegan = user.getBoolean("Vegan")
                                ServiceHandler.vegetarian = user.getBoolean("Vegetarian")
                                var allergies = user.getJSONArray("Allergies")
                                ServiceHandler.allergies = ArrayList<String>()
                                for (i in 0 until allergies.length()) {
                                    ServiceHandler.allergies!!.add(allergies.getString(i))
                                }
                                //Default kitchen
                                var kitchens = user.getJSONArray("Kitchens")
                                var kitchen = kitchens.getJSONObject(0)
                                ServiceHandler.primaryKitchenId = kitchen.getInt("Id")
                                //Remind the user
                                Toast.makeText(applicationContext,"Please confirm your email before adding food!", Toast.LENGTH_LONG).show()
                                val intent = Intent(this@CreateAccountActivity, CategoryListActivity::class.java)
                                startActivity(intent)
                            }
                            else if (userId == -1){
                                Toast.makeText(applicationContext,"Email exists", Toast.LENGTH_LONG).show()
                            }
                            else if (userId == -2){
                                Toast.makeText(applicationContext,"Email is invalid", Toast.LENGTH_LONG).show()
                            }

                        })

                }
                else
                {
                    Toast.makeText(applicationContext,"Passwords don't match", Toast.LENGTH_LONG).show()
                }
            }
            else
            {
                Toast.makeText(applicationContext,"Please fill all fields", Toast.LENGTH_LONG).show()
            }
        }

        btnCancel.setOnClickListener{
            // return to login screen
            val intent = Intent(this@CreateAccountActivity, MainActivity::class.java)
            startActivity(intent)
        }

    }
}
