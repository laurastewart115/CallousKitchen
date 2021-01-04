/* Author: Kevin Gadelha */

package com.example.callouskitchenandroid

import android.Manifest
import android.content.Intent
import android.content.pm.PackageManager
import android.os.Bundle
import android.util.Log
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.camera.core.CameraSelector
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.ImageProxy
import androidx.camera.core.Preview
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import com.android.volley.Response
import com.google.mlkit.vision.barcode.Barcode
import com.google.mlkit.vision.barcode.BarcodeScannerOptions
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.common.InputImage
import kotlinx.android.synthetic.main.activity_barcode_scan.*
import org.json.JSONObject
import java.time.LocalDate
import java.time.format.DateTimeFormatter
import java.util.concurrent.ExecutorService
import java.util.concurrent.Executors

typealias ResultListener = (result: String) -> Unit

/**
 * Activity that uses the camera to scan a barcode.
 *
 * @author Kevin Gadelha
 * Uses camerax and ml kit barcode identifier to get barcode and get info from openfoodfacts
 *
 * And then send it back to the add food
 * I copied some of this code off the internet
 */
class activity_barcode_scan : AppCompatActivity() {

    private lateinit var cameraExecutor: ExecutorService

    /**
     * Called when the activity is created. Requests camera permission (if needed)
     * and starts the camera.
     *
     * @param savedInstanceState Can be used to save application state
     */
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_barcode_scan)

        // Request camera permissions
        if (allPermissionsGranted()) {
            startCamera()
        } else {
            ActivityCompat.requestPermissions(
                this, REQUIRED_PERMISSIONS, REQUEST_CODE_PERMISSIONS
            )
        }

        cameraExecutor = Executors.newSingleThreadExecutor()
    }

    /**
     * Checks if the permissions are granted
     *
     */
    private fun allPermissionsGranted() = REQUIRED_PERMISSIONS.all {
        ContextCompat.checkSelfPermission(
            baseContext, it
        ) == PackageManager.PERMISSION_GRANTED
    }

    /**
     * Shutdown the camera executor when this activity closes.
     *
     */
    override fun onDestroy() {
        super.onDestroy()
        cameraExecutor.shutdown()
    }

    /**
     * Stores tag and permission information needed for working with the camera.
     *
     */
    companion object {
        private const val TAG = "CameraXBasic"
        private const val REQUEST_CODE_PERMISSIONS = 10
        private val REQUIRED_PERMISSIONS = arrayOf(Manifest.permission.CAMERA)
    }

    /**
     * Starts the camera if permissions are granted.
     *
     * @param requestCode Code being passed in
     * @param permissions All permissions being requested
     * @param grantResults Results for the permissions (granted or denied)
     */
    override fun onRequestPermissionsResult(
        requestCode: Int, permissions: Array<String>, grantResults:
        IntArray
    ) {
        if (requestCode == REQUEST_CODE_PERMISSIONS) {
            if (allPermissionsGranted()) {
                startCamera()
            } else {
                Toast.makeText(
                    this,
                    "Permissions not granted by the user.",
                    Toast.LENGTH_SHORT
                ).show()
                finish()
            }
        }
    }

    /**
     * Starts the camera, starts the barcode analysis and calls openfoodfacts with the barcode
     * Then sends it back to the add food activity
     *
     * @author Kevin Gadelha
     */
    private fun startCamera() {
        val cameraProviderFuture = ProcessCameraProvider.getInstance(this)

        cameraProviderFuture.addListener(Runnable {
            // Used to bind the lifecycle of cameras to the lifecycle owner
            val cameraProvider: ProcessCameraProvider = cameraProviderFuture.get()

            // Preview
            val preview = Preview.Builder()
                .build()
                .also {
                    it.setSurfaceProvider(viewFinder.createSurfaceProvider())
                }

            //This is my code where I get stuff done
            val imageAnalyzer = ImageAnalysis.Builder()
                .build()
                .also {
                    it.setAnalyzer(cameraExecutor, ImageAnalyzer { result -> //Result is the barcode
                        it.clearAnalyzer()
                        ServiceHandler.callOpenFoodFacts(
                            result, this,
                            Response.Listener { response ->
                                val json = JSONObject(response.toString())
                                val product = json.optJSONObject("product")
                                //If product is null these are all null
                                val foodName = product?.optString("product_name")
                                //Convert quantity
                                val sourceQuantity = product?.optString("quantity")
                                var quantity = ExtrapolateQuantity(sourceQuantity)
                                var quantityClassifer = ExtrapolateQuantityClassifier(
                                    sourceQuantity
                                )
                                val expirationDate =
                                    StringToDate(product?.optString("expiration_date"))
                                //Source info from which I get vegan/vegetarian info
                                val ingredientsAnalysis =
                                    product?.optJSONArray("ingredients_analysis_tags")
                                //1 means true, 0 means false, -1 means unknown
                                var vegan: Int? = -1
                                var vegetarian: Int? = -1
                                for (i in 0 until (ingredientsAnalysis?.length() ?: 0)) {
                                    //Get the real info
                                    val item = RemovePrefix(ingredientsAnalysis!![i].toString())
                                    //Decide the facts based on the info
                                    when (item) {
                                        "vegan" -> vegan = 1
                                        "non-vegan" -> vegan = 0
                                        "vegan-status-unknown" -> vegan = -1
                                        "vegetarian" -> vegetarian = 1
                                        "non-vegetarian" -> vegetarian = 0
                                        "vegetarian-status-unknown" -> vegetarian = -1
                                    }
                                }
                                //Traces are one long string
                                var traces = StringToArray(product?.optString("traces"))
                                //But the ingredients are an an actual array
                                var ingredientsTags = product?.optJSONArray("ingredients_tags")
                                //No easy way to convert jsonarray
                                var ingredients =
                                    Array<String>(ingredientsTags?.length() ?: 0) { "n = $it" }
                                //Go through each one
                                for (i in 0 until (ingredientsTags?.length() ?: 0)) {
                                    ingredients[i] =
                                        FormatString(ingredientsTags!![i].toString())
                                }

                                // Go back to the AddFoodActivity and send all the food data
                                val intent = Intent(this, AddFoodActivity::class.java)
                                intent.putExtra("FOODNAME", foodName)
                                intent.putExtra("QUANTITY", quantity)
                                intent.putExtra("QUANTITYCLASSIFIER", quantityClassifer)
                                //Format the date
                                val formatter = DateTimeFormatter.ofPattern("MM/dd/yyyy")
                                intent.putExtra("EXPIRY", expirationDate?.format(formatter))
                                intent.putExtra("VEGAN", vegan)
                                intent.putExtra("VEGETARIAN", vegetarian)
                                intent.putExtra("INGREDIENTS", ingredients)
                                intent.putExtra("TRACES", traces)


                                startActivity(intent)
                            })
                    })
                }

            // Select back camera as a default
            val cameraSelector = CameraSelector.DEFAULT_BACK_CAMERA

            try {
                // Unbind use cases before rebinding
                cameraProvider.unbindAll()

                // Bind use cases to camera
                cameraProvider.bindToLifecycle(
                    this, cameraSelector, preview, imageAnalyzer
                )

            } catch (exc: Exception) {
                Log.e(TAG, "Use case binding failed", exc)
            }

        }, ContextCompat.getMainExecutor(this))
    }

    /**
     * Turns a string quantity into a double.
     *
     * @param quantity The quantity as a String
     * @return The quantity as a double (will be 0.0 if it can't be cast as a double or it's null)
     * @author Kevin Gadelha
     */
    private fun ExtrapolateQuantity(quantity: String?) : Double{
        if (quantity.isNullOrEmpty())
            return 0.0
        //Only get the numbers from the string
        else
            return quantity.filter { it.isDigit() }.toDoubleOrNull() ?: 0.0
    }

    /**
     * Determine quantity classifier (ex: item, g, mL).
     *
     * @param quantity The quantity as a string
     * @return The quantity classifier
     * @author Kevin Gadelha
     */
    private fun ExtrapolateQuantityClassifier(quantity: String?) : String{
        //Default is item
        if (quantity.isNullOrEmpty())
            return "item"
        else
            {
                //Honestly, the quantity has always been in g or ml so this is probably overkill
                //And I can't use the source list because it needs to be in a different order
                //Starting with longer strings and going to shorter
                var orderedClassifiers = listOf<String>(
                    "kg",
                    "mg",
                    "mL",
                    "oz",
                    "gallon",
                    "lb",
                    "g",
                    "L"
                )
                //One custom search and return because fl oz is hard
                if (quantity!!.toLowerCase().contains("fl") && quantity!!.toLowerCase().contains("oz")){
                    return "fl. oz."
                }
                //If the source contains the classifier, return it
                orderedClassifiers.forEach{
                    if (quantity!!.toLowerCase().contains(it.toLowerCase())){
                        return it
                    }
                }
                //Return item by default
                return "item"
            }
    }

    /**
     * Convert the expiry date String to a LocalDate object.
     *
     * @param date The date as a String
     * @return The date as a localDate (will be null if it can't be converted or if it's null)
     * @author Kevin Gadelha
     */
    private fun StringToDate(date: String?) : LocalDate?{
        if (date.isNullOrEmpty())
            return null
        else{
            //Try the different possible date formats and hope that there aren't any more possibilities
            try{
                return LocalDate.parse(
                    date,
                    DateTimeFormatter.ofPattern("yyyy/MM/dd")
                )
            }
            catch (exc: Exception) {
                println("yyyy/MM/dd didn't work")
            }
            try{
                return LocalDate.parse(
                    date,
                    DateTimeFormatter.ofPattern("dd/MM/yyyy")
                )
            }
            catch (exc: Exception) {
                println("dd/MM/yyyy didn't work")
            }
            try{
                return LocalDate.parse(
                    date,
                    DateTimeFormatter.ofPattern("MM/d/yyyy")
                )
            }
            catch (exc: Exception) {
                println("MM/dd/yyyy didn't work")
            }
        }
        return null
    }

    /**
     * Trim whitespace from a string and replace underscores and dashes with spaces
     * And remove some unimportant stuff
     *
     * @param theString The original string
     * @return The formatted string
     * @author Kevin Gadelha
     */
    private fun FormatString(theString: String) : String{
        var formattedString = RemovePrefix(theString)
        formattedString = formattedString.trim()
        formattedString = formattedString.replace('-', ' ')
        formattedString = formattedString.replace('_', ' ')
        return formattedString
    }

    /**
     * Remove text before a colon.
     *
     * @param textWithPrefix Text with a colon indicating there is a prefix
     * @return String without the prefix
     * @author Kevin Gadelha
     */
    private fun RemovePrefix(textWithPrefix: String) : String{
        val colonLocation = textWithPrefix.indexOf(':')
        //Don't do anything to stuff without a colon
        if (colonLocation != -1)
            return textWithPrefix.substring(colonLocation + 1, textWithPrefix.length)
        return textWithPrefix
    }

    //Remove spaces and split
    /**
     * Remove spaces and split a comma delimited array
     *
     * @param commaDelimitedArray A string containing a comma delimited array
     * @return An Array of Strings that were in the comma delimited array
     * @author Kevin Gadelha
     */
    private fun StringToArray(commaDelimitedArray: String?) : Array<String>?{
        if (commaDelimitedArray.isNullOrEmpty())
            return null
        else
            return commaDelimitedArray.split(",").map { FormatString(it) }.toTypedArray()
    }

    /**
     * Image analyzer that looks for barcodes. This gets run whenever there's new camera info.
     * Takes a listener and then runs activates the listener with the barcode
     *
     * @author Kevin Gadelha
     */
    private class ImageAnalyzer(private val listener: ResultListener) : ImageAnalysis.Analyzer {

        /**
         *
         * @param imageProxy
         * @author Kevin Gadelha
         */
        @androidx.camera.core.ExperimentalGetImage
        override fun analyze(imageProxy: ImageProxy) {
                //Conversions
                val mediaImage = imageProxy.image
                if (mediaImage != null) {
                    val image = InputImage.fromMediaImage(
                        mediaImage,
                        imageProxy.imageInfo.rotationDegrees
                    )
                    //Where the magic happens
                    scanBarcodes(image, imageProxy);
                }
                //Close the imageproxy if it's not being used
                else
                    imageProxy.close()
        }

        /**
         * Processes the image to find barcodes.
         *
         * @param image
         * @param imageProxy
         * @author Kevin Gadelha
         */
        fun scanBarcodes(image: InputImage, imageProxy: ImageProxy) {
            // [START set_detector_options]
            //I though upc and product were the same thing but they're separate here so accept either
            val options = BarcodeScannerOptions.Builder()
                .setBarcodeFormats(
                    //Only take food barcodes
                    Barcode.TYPE_PRODUCT,
                    Barcode.FORMAT_UPC_A
                )
                .build()
            // [END set_detector_options]

            // [START get_detector]
            //val scanner = BarcodeScanning.getClient()
            // Or, to specify the formats to recognize:
            val scanner = BarcodeScanning.getClient(options)
            // [END get_detector]

            // [START run_detector]
            val result = scanner.process(image)
                .addOnSuccessListener { barcodes ->
                    // Task completed successfully
                    // [START_EXCLUDE]
                    // [START get_barcodes]
                    for (barcode in barcodes) {
                        //get the actual barcode
                        val rawValue = barcode.rawValue

                        val valueType = barcode.valueType
                        // Only proceed if it's what we're looking for
                        if (valueType == Barcode.TYPE_PRODUCT || valueType == Barcode.FORMAT_UPC_A) {
                            if (!rawValue.isNullOrEmpty()) { //Only proceed if barcode was found
                                //basically this returns the result in a listener
                                listener(rawValue)
                            }
                        }
                    }
                    // [END get_barcodes]
                    // [END_EXCLUDE]
                    //Important, only close the imageProxy once everything's been processed
                    imageProxy.close()
                }
                .addOnFailureListener {
                    //Important, only close the imageProxy once everything's been processed
                    imageProxy.close()
                }
            // [END run_detector]
        }
    }

}