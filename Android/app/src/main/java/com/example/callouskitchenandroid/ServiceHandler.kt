/* Author: Kevin Gadelha */
package com.example.callouskitchenandroid

import android.content.Context
import android.content.SharedPreferences
import android.util.Log
import android.widget.Toast
import com.android.volley.Request
import com.android.volley.RequestQueue.RequestFinishedListener
import com.android.volley.Response
import com.android.volley.toolbox.JsonObjectRequest
import com.android.volley.toolbox.StringRequest
import com.android.volley.toolbox.Volley
import com.google.gson.Gson
import com.google.gson.GsonBuilder
import org.json.JSONObject
import java.time.Instant
import java.time.LocalDate
import java.time.LocalDateTime
import java.time.ZoneOffset
import java.time.format.DateTimeFormatter
import java.util.*
import kotlin.collections.HashMap

/**
 * Handles all the C# communication stuff
 * And also handles singlton stuff
 * Like methods I need across the app
 * And variables I want to use across the app
 *
 * @author Kevin Gadelha
 */
class ServiceHandler {
    /**
     * I need a companion object since kotlin doesn't have static stuff
     * @author Kevin Gadelha
     */
    companion object Static {
        //Some getters and setters might have been a good idea
        //I blame the fact we were never taught them in class
        //And it's too late now
        var userId = -1
        var email : String? = null
        var vegan : Boolean = false
        var vegetarian : Boolean = false
        var allergies : ArrayList<String>? = null
        var primaryKitchenId = -1
        //This will always be set on app start
        lateinit var sharedPref : SharedPreferences
        //this is better than passing stuff through intents all the time
        var lastCategory = ""
        //The service url
        val baseUrl =  "http://142.55.32.86:50241"

        /**
         * Calls a method in a service and uses a response listener to return the result
         *
         * @param service The name of the service with the file extension
         * @param method The method name being called
         * @param parameters The parameters for the method in a hashmap
         * @param context The current context
         * @param response The response listener
         * @author Kevin Gadelha
         */
        fun callService(service : String, method : String, parameters : HashMap<String,Any?>, context: Context, response : Response.Listener<JSONObject>)
        {
            val queue = Volley.newRequestQueue(context)
            val url = "$baseUrl/$service/$method"

            var jsonObject = JSONObject()
            for ((key, value) in parameters){
                jsonObject.put(key,value)
            }
            println("the url is $url")
            println("the json request is $jsonObject")
            val request = JsonObjectRequest(Request.Method.POST, url, jsonObject,
                response,
                Response.ErrorListener { println("request failed") })
            queue.add(request)
        }

        /**
         * Specifically calls the account service.
         * I would have loved to make a method for each accountservice method
         * But kotlin sucks and wouldn't let me
         * Because the results happen in a different thread or something
         * And as far as I can tell, I can't just wait for it and then return it here
         *
         * @param method The method name being called
         * @param parameters The parameters for the method in a hashmap
         * @param context The current context
         * @param response The response listener
         * @author Kevin Gadelha
         */
        fun callAccountService(method : String, parameters : HashMap<String,Any?>, context: Context, response : Response.Listener<JSONObject>)
        {
            callService("AccountService.svc",method,parameters,context, response)
        }

        /**
         * Calls OpenFoodFacts directly to get the result for a barcode (faster than going through
         * the C# service)
         *
         * @param barcode The barcode to check
         * @param context The current context
         * @param response The response listener
         * @author Kevin Gadelha
         */
        fun callOpenFoodFacts(barcode : String, context: Context, response : Response.Listener<JSONObject>){
            val queue = Volley.newRequestQueue(context)
            //Public url
            val url = "https://world.openfoodfacts.org/api/v0/product/$barcode.json"
            var jsonObject = JSONObject()
            val request = JsonObjectRequest(Request.Method.GET, url, jsonObject,
                response,
                Response.ErrorListener { println("request failed") })
            queue.add(request)
        }

        /**
         * Turns a LocalDate object into a C# String so it can be saved.
         *
         * @param date The date as a LocalDate
         * @return The date as a String
         * @author Kevin Gadelha
         */
        fun serializeDate(date : LocalDate?) : String?{
            if (date == null)
                return null
            var dateStart = date.atStartOfDay()
            //Something is off because I need to subtract this to get the correct date
            dateStart = dateStart.minusDays(166)
            //Adds 4 hours in seconds to make up for timezone differences
            //I don't know what the rest of the numbers and dashes mean but they might be important
            //The first three zeros are actually important though as they change it milliseconds
            //I really should have just changed the service to accept a string
            //Because C# date serialization is a nightmare
            return "/Date("+(dateStart.toEpochSecond(ZoneOffset.UTC)+14400000).toString()+"000-00-00T00:00:00.0-00:00)/"
        }

        /**
         * Turns a C# String into a LocalDate
         *
         * @param date The date as a String
         * @return The date as a LocalDate
         * @author Kevin Gadelha
         */
        fun deSerializeDate(date : String) : LocalDate?{
            if (date.isNullOrEmpty() || date == "null"){
                return null
            }
            //get the miliseconds since 1970 from the string
            //Don't blame me, blame C# serialization
            //I could probably get this number a better way
            //But there are a lot of things I probably could have and should have done better
            //At least this works
            var epoch = date.substring(6,date.length-10)
            //convert that to a date
            return LocalDateTime.ofEpochSecond(epoch.toLong(),0,ZoneOffset.UTC).toLocalDate()
        }

        /**
         * Create a warning message if a food does not match the user's dietary requirements.
         *
         * @param food The food to check
         * @param onlyWarning If true, messages will not be generated if the food is safe
         * @return The warning message
         */
        fun generateWarningMessage(food : Food, onlyWarning : Boolean) : String?{

            var warningMessage = ""
            //Only show warning messages if applicable
            if (vegan && food.vegan == 0) {
                warningMessage = addToWarningMessage(warningMessage, "is not vegan")
            } else if (vegan && food.vegan == 1 && !onlyWarning) {
                warningMessage = addToWarningMessage(warningMessage, "is vegan")
            } else if (vegetarian && food.vegetarian == 0) {
                warningMessage = addToWarningMessage(warningMessage, "is not vegetarian")
            } else if (vegetarian && food.vegetarian == 1 && !onlyWarning) {
                warningMessage = addToWarningMessage(warningMessage, "is vegetarian")
            }

            var containedAllergens =
                getElementsOfArrayThatAreContainedInAnotherArray(
                    ServiceHandler.allergies!!,
                    food.ingredients?.toList()
                )

            containedAllergens.forEach() {
                warningMessage = addToWarningMessage(warningMessage, "contains $it")
            }

            var containedTraces =
                getElementsOfArrayThatAreContainedInAnotherArray(
                    ServiceHandler.allergies!!,
                    food.traces?.toList()
                )

            containedTraces.forEach() {
                warningMessage = addToWarningMessage(warningMessage, "contains traces of $it")
            }

            //Interpret empty ingredients or traces  as unknown
            //Only shows these if the user has allergies
            //Look at what warning message is generated and then backtrack to find out why the condition makes sense
            if (ServiceHandler.allergies != null && ServiceHandler.allergies!!.count() > 0 && food.ingredients.size > 0 || food.traces.size > 0){
                if (containedAllergens.size == 0 && food.traces.size == 0) {
                    warningMessage = addToWarningMessage(
                        warningMessage,
                        "does not contain allergens but traces are unknown"
                    )
                } else if (containedTraces.size == 0 && food.ingredients.size == 0) {
                    warningMessage = addToWarningMessage(
                        warningMessage,
                        "does not have traces allergens but ingredients are unknown"
                    )
                } else if (containedAllergens.size == 0 && containedTraces.size == 0 && !onlyWarning) {
                    warningMessage = addToWarningMessage(
                        warningMessage,
                        "does not contain allergens or traces of allergens"
                    )
                }
            }

            return warningMessage
        }

        /**
         * Compares two arrays to see if elements from one are in the other. Used for checking if
         * the user's allergens are present in the food.
         *
         * @param sourceArray The source array to check
         * @param checkingArray The array to check the source against
         * @return And ArrayList of the elements that are in both arrays
         * @author Kevin Gadelha
         */
        fun getElementsOfArrayThatAreContainedInAnotherArray(
            sourceArray: List<String>?, checkingArray: List<String>?
        ): ArrayList<String> {
            var containedElements = ArrayList<String>()
            sourceArray?.forEach() {
                checkingArray?.forEach() { it2: String ->
                    if (it2.contains(it)) {
                        containedElements.add(it2)
                    }
                }
            }
            return containedElements
        }

        /**
         * Format warning message properly when more warnings are being added to it.
         *
         * @param warningMessage The original warning message
         * @param addition The String being added to the message
         * @return The new warning message
         * @author Kevin Gadelha
         */
        fun addToWarningMessage(warningMessage: String, addition: String): String {
            var newWarningMessage = warningMessage
            //Start the warning message with food and then follow with whatever the message is
            if (newWarningMessage.isNullOrEmpty()) {
                newWarningMessage = "Food "
            //Add "and" if it's an adendum to an existing message
            //There can be multiple ands, I'm not a stickler for grammar
            } else {
                newWarningMessage += " and "
            }
            //Add the actual message
            newWarningMessage += addition
            return newWarningMessage
        }
    }

}