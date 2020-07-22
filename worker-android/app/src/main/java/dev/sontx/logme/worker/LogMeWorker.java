package dev.sontx.logme.worker;

import android.content.Context;
import android.content.Intent;
import android.util.Log;

import dev.sontx.logme.worker.uce.Callback;
import dev.sontx.logme.worker.uce.UCEDefaultActivity;
import dev.sontx.logme.worker.uce.UCEHandler;

public final class LogMeWorker implements Callback<Intent> {
    private static final String TAG = LogMeWorker.class.getName();

    private final IWorkerClient workerClient;
    private final Context context;

    public LogMeWorker(Context context, String url) {
        this.context = context;
        String appName = UCEDefaultActivity.getApplicationName(context).replace(" ", "");
        workerClient = new MqttIWorkerClient(appName, url, String.format("%s/logs", appName));
        new UCEHandler.Builder(context)
                .setHandleIntent(this)
                .build();
    }

    public void start() {
        try {
            workerClient.start();
        } catch (LogMeException e) {
            Log.e(TAG, "Error while starting " + getClass().getName(), e);
        }
    }

    public void stop() {
        try {
            workerClient.stop();
        } catch (LogMeException ignored) {
        }
    }

    public void send(String message) {
        try {
            workerClient.send(message);
        } catch (LogMeException e) {
            Log.e(TAG, "Error while sending crash logs", e);
        }
    }

    @Override
    public void run(Intent value) {
        String report = UCEDefaultActivity.getAllErrorDetailsFromIntent(context, value);
        send(report);
    }
}
