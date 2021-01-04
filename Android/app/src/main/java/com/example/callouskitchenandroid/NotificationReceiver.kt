/* Author: Kevin Gadelha */
package com.example.callouskitchenandroid

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent

/**
 * Calls the scheduler
 *
 * @author Kevin Gadelha
 */
class NotificationReceiver : BroadcastReceiver() {

    /**
     *
     *
     * @param context
     * @param intent
     * @author Kevin Gadelha
     */
    override fun onReceive(context: Context, intent: Intent?) {
        Util.scheduleJob(context);
    }
}
