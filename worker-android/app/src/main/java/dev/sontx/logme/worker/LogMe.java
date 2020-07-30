package dev.sontx.logme.worker;

import android.annotation.SuppressLint;
import android.content.Context;
import android.provider.Settings;
import android.util.Log;

import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import dev.sontx.logme.worker.uce.UCEDefaultActivity;
import dev.sontx.logme.worker.uce.UCEHandler;

public final class LogMe implements CommandHandler {
    private static final String TAG = LogMe.class.getName();

    private final ExecutorService executorService = Executors.newCachedThreadPool();
    private final IWorkerClient workerClient;
    private final Context context;

    public LogMe(Context context, String url) {
        this.context = context.getApplicationContext();
        new UCEHandler.Builder(this.context)
                .setHandleIntent(value -> {
                    String report = UCEDefaultActivity.getAllErrorDetailsFromIntent(this.context, value);
                    String appName = UCEDefaultActivity.getApplicationName(this.context);
                    String wrap = String.format("%s\n%s", appName, report);
                    send(wrap, MessageType.Exception);
                })
                .build();

        String appName = UCEDefaultActivity.getApplicationName(this.context).replace(" ", "");
        String clientName = getClientId(appName);
        MqttIWorkerClient mqttIWorkerClient = new MqttIWorkerClient(this.context, clientName, url, appName);
        mqttIWorkerClient.setCommandHandler(this);
        workerClient = mqttIWorkerClient;
    }

    private String getClientId(String appName) {
        @SuppressLint("HardwareIds")
        String androidId = Settings.Secure.getString(context.getContentResolver(), Settings.Secure.ANDROID_ID);
        return String.format("%s@%s", appName, androidId);
    }

    public void start() {
        executorService.execute(() -> {
            try {
                workerClient.start();
            } catch (LogMeException e) {
                Log.e(TAG, "Error while starting " + getClass().getName(), e);
            }
        });
    }

    public void stop() {
        try {
            workerClient.stop();
        } catch (LogMeException ignored) {
        }
    }

    public void sendLog(String log) {
        send(log, MessageType.Log);
    }

    private void send(String msg, MessageType messageType) {
        executorService.execute(() -> {
            try {
                if (workerClient != null) {
                    workerClient.send(msg, messageType);
                }
            } catch (LogMeException e) {
                Log.e(TAG, "Error while sending crash logs", e);
            }
        });
    }

    @Override
    public void handleCommand(String command) {
        if (Constants.COMMAND_GET_SYSTEM_INFO.equals(command.toUpperCase())) {
            String info = SystemInfo.getInfo(context);
            String appName = UCEDefaultActivity.getApplicationName(context);
            String wrap = String.format("%s\n%s\n%s", command, appName, info);
            send(wrap, MessageType.ControlResponse);
        }
    }
}
