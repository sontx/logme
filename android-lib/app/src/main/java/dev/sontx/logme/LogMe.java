package dev.sontx.logme;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.provider.Settings;
import android.text.TextUtils;
import android.util.Base64;
import android.util.Log;

import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

import dev.sontx.logme.uce.UCEDefaultActivity;
import dev.sontx.logme.uce.UCEHandler;

public final class LogMe implements CommandHandler {
    private static final String TAG = LogMe.class.getName();

    public static LogMe startNew(Context context, String url) {
        LogMe logMe = new LogMe(context, url);
        logMe.start();
        return logMe;
    }

    private final ExecutorService executorService = Executors.newCachedThreadPool();
    private final Context context;
    private final String url;
    private WorkerClient workerClient;

    public LogMe(Context context, String url) {
        this.context = context;
        this.url = url;
    }

    private String getClientId(String appName) {
        @SuppressLint("HardwareIds")
        String androidId = Settings.Secure.getString(context.getContentResolver(), Settings.Secure.ANDROID_ID);
        return String.format("%s@%s", appName, androidId);
    }

    public void start() {
        Logger.setLogMe(this);
        Thread thread = new Thread(() -> {
            executorService.execute(() -> {
                try {
                    new UCEHandler.Builder(context)
                            .setHandleIntent(value -> {
                                String report = UCEDefaultActivity.getAllErrorDetailsFromIntent(context, value);
                                String appName = UCEDefaultActivity.getApplicationName(context);
                                String wrap = String.format("%s\n%s", appName, report);
                                send(wrap, MessageType.Exception);
                            })
                            .build();

                    String appName = UCEDefaultActivity.getApplicationName(context).replace(" ", "");
                    String clientName = getClientId(appName);
                    MqttIWorkerClient mqttIWorkerClient = new MqttIWorkerClient(context, clientName, url, appName);
                    mqttIWorkerClient.setCommandHandler(this);
                    workerClient = mqttIWorkerClient;
                    workerClient.start();
                } catch (LogMeException e) {
                    Log.e(TAG, "Error while starting " + getClass().getName(), e);
                }
            });
        });
        thread.setDaemon(true);
        thread.start();
    }

    public void stop() {
        Logger.setLogMe(null);
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
        String data = null;
        switch (command.toUpperCase()) {
            case Constants.COMMAND_GET_SYSTEM_INFO:
                data = SystemInfo.getInfo(context);
                break;
            case Constants.COMMAND_TAKE_SCREENSHOT:
                if (context instanceof Activity) {
                    byte[] screenshot = ScreenshotHelper.takeScreenshot((Activity) context);
                    data = Base64.encodeToString(screenshot, Base64.DEFAULT);
                }
                break;
        }

        if (!TextUtils.isEmpty(data)) {
            String appName = UCEDefaultActivity.getApplicationName(context);
            String wrap = String.format("%s\n%s\n%s", command, appName, data);
            send(wrap, MessageType.ControlResponse);
        }
    }
}
