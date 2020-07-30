package dev.sontx.logme.worker;

import android.annotation.SuppressLint;
import android.util.Log;

import dev.sontx.logme.worker.uce.UCEHandler;

public final class Logger {
    private static final String TAG = Logger.class.getName();
    @SuppressLint("StaticFieldLeak")
    private static LogMe logMe;

    static void setLogMe(LogMe logMe) {
        Logger.logMe = logMe;
    }

    public static void i(String tag, String message) {
        Log.i(tag, message);
        sendToSupervisor("INFO", message);
    }

    public static void d(String tag, String message) {
        Log.d(tag, message);
        sendToSupervisor("DEBUG", message);
    }

    public static void e(String tag, String message, Throwable throwable) {
        Log.e(tag, message, throwable);
        String stackTrace = "";
        if (throwable != null)
            stackTrace = "\n" + UCEHandler.getStackTrace(throwable);
        sendToSupervisor("ERROR", message + stackTrace);
    }

    private static void sendToSupervisor(String type, String message) {
        LogMe logMe = Logger.logMe;
        if (logMe != null) {
            logMe.sendLog(String.format("%s: %s", type, message));
        }
    }

    public static void i(String message) {
        i(TAG, message);
    }

    public static void d(String message) {
        d(TAG, message);
    }

    public static void e(String message, Throwable throwable) {
        e(TAG, message, throwable);
    }
}
