/* Author: Kevin Gadelha */
package com.example.callouskitchenandroid

import android.app.*
import android.content.Context
import android.content.Intent
import android.graphics.Color
import android.os.Build
import android.os.Handler
import android.os.IBinder
import android.util.Log
import androidx.core.app.NotificationCompat
import androidx.core.app.NotificationManagerCompat
import com.android.volley.Response
import org.json.JSONObject
import java.time.LocalDate
import java.util.*

/**
 *
 * The part where I actually show the notification
 * @author Kevin Gadelha
 */
class NotificationService : Service() {

    lateinit var notificationManager: NotificationManager

    /**
     *
     *
     * @param arg0
     * @author Kevin Gadelha
     */
    override fun onBind(arg0: Intent): IBinder? {
        return null
    }

    /**
     *
     *
     * @param intent
     * @param flags
     * @param startId
     * @return
     * @author Kevin Gadleha
     */
    override fun onStartCommand(intent: Intent, flags: Int, startId: Int): Int {
        super.onStartCommand(intent, flags, startId)
        var context = applicationContext

        //Only do the notification if the user is logged in and this magical context I'm getting from somewhere isn't null
        if (ServiceHandler.userId != -1 && context != null) {

            ServiceHandler.callAccountService(
                "GetInventory", hashMapOf("kitchenId" to ServiceHandler.primaryKitchenId), context!!,
                Response.Listener { response ->
                    val json = JSONObject(response.toString())
                    val foodsJson = json.optJSONArray("GetInventoryResult")
                    if (foodsJson != null){

                        var foods: ArrayList<Food> = arrayListOf<Food>()
                        for (i in 0 until foodsJson.length()) {
                            var foodJson: JSONObject = foodsJson.getJSONObject(i)
                            //Only get the important info
                            var food = Food(foodJson.getInt("Id"), foodJson.getString("Name"))
                            food.expiryDate =
                                ServiceHandler.deSerializeDate(foodJson.getString("ExpiryDate"))
                            foods.add(food)
                        }
                        //Only the stuff expiring soon
                        var expiringFoods = foods.filter { food -> food.expiryDate != null && (food.expiryDate!! < LocalDate.now()
                            .plusDays(3)) }
                        //Show expiring soonest first
                        expiringFoods = expiringFoods.sortedWith(Comparator<Food>{ a, b ->
                            when {
                                a.expiryDate == null && b.expiryDate != null -> 1
                                a.expiryDate != null && b.expiryDate == null -> -1
                                a.expiryDate == null && b.expiryDate == null -> 0
                                a.expiryDate!! > b.expiryDate!! -> 1
                                a.expiryDate!! < b.expiryDate!! -> -1
                                else -> 0
                            }
                        })
                        //Only show notification if stuff is actually expiring
                        if (expiringFoods.size > 0) {
                            notificationManager =
                                context?.getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager
                            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                                val channel = NotificationChannel(
                                    "primary_notification_channel",
                                    "Messages",
                                    NotificationManager.IMPORTANCE_HIGH
                                )

                                channel.enableLights(true)
                                channel.lightColor = Color.RED
                                channel.enableVibration(true)
                                channel.description = "Messages Notification"
                                notificationManager.createNotificationChannel(channel)
                            }

                            // Go to the inventory
                            val intent = Intent(context, InventoryActivity::class.java).apply {
                                this.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
                            }
                            //And make sure to sort by expiring
                            intent.putExtra("Expiring Soon", true)
                            val pendingIntent: PendingIntent = PendingIntent.getActivity(
                                context,
                                0,
                                intent,
                                //Update with the extra
                                PendingIntent.FLAG_UPDATE_CURRENT
                            )

                            val builder =
                                NotificationCompat.Builder(context, "primary_notification_channel").apply {
                                    setSmallIcon(R.drawable.hippo)
                                    setContentTitle("Expiring")
                                    setContentText(expiringFoods.joinToString { it -> it.name })
                                    setPriority(NotificationCompat.PRIORITY_DEFAULT)
                                    setContentIntent(pendingIntent)
                                    setStyle(
                                        //If there's overflow
                                        //Then this sets the expandable text
                                        NotificationCompat.BigTextStyle()
                                            .bigText(expiringFoods.joinToString { it -> it.name })
                                    )
                                }

                            // displaying the notification with NotificationManagerCompat.
                            with(NotificationManagerCompat.from(context)) {
                                notify(112, builder.build())
                            }
                        }
                    }
                })
        }
        //Start sticky basically means to keep restarting this service when necessary
        return START_STICKY
    }
}