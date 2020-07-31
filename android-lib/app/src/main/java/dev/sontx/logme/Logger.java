package dev.sontx.logme;

import android.annotation.SuppressLint;
import android.util.Log;

import java.text.SimpleDateFormat;
import java.util.Date;

import dev.sontx.logme.uce.UCEHandler;

public final class Logger {
    private static final String TAG = Logger.class.getName();
    @SuppressLint("StaticFieldLeak")
    private static LogMe logMe;

    private static final SimpleDateFormat simpleDateFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss,SSS");

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
            String now = simpleDateFormat.format(new Date());
            logMe.sendLog(String.format("%s %s: %s", now, type, message));
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
