/* Authors: Kevin Gadelha */
package com.example.callouskitchenandroid

import android.app.job.JobInfo
import android.app.job.JobScheduler
import android.content.ComponentName
import android.content.Context

/**
 *
 *
 * @author Kevin Gadelha
 */
object Util {

    /**
     * Schedule the expiry notification to run daily
     *
     * @param context
     * @author Kevin Gadelha
     */
    fun scheduleJob(context: Context) {
        val serviceComponent = ComponentName(context, TheJobService::class.java)
        val builder = JobInfo.Builder(0, serviceComponent)
        builder.setMinimumLatency(60 * 60 *24 * 1000.toLong()) // wait at least 1 day
        builder.setOverrideDeadline(3 * 60 * 60 *24 * 1000.toLong()) // maximum delay of 3 days
        //builder.setRequiredNetworkType(JobInfo.NETWORK_TYPE_UNMETERED); // require unmetered network
        //builder.setRequiresDeviceIdle(true); // device should be idle
        //builder.setRequiresCharging(false); // we don't care if the device is charging or not
        val jobScheduler = context.getSystemService(JobScheduler::class.java)
        jobScheduler.schedule(builder.build())
    }
}