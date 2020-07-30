package dev.sontx.logme.worker;

import android.content.Context;
import android.util.Log;

import org.eclipse.paho.android.service.MqttAndroidClient;
import org.eclipse.paho.client.mqttv3.IMqttActionListener;
import org.eclipse.paho.client.mqttv3.IMqttMessageListener;
import org.eclipse.paho.client.mqttv3.IMqttToken;
import org.eclipse.paho.client.mqttv3.MqttConnectOptions;
import org.eclipse.paho.client.mqttv3.MqttException;
import org.eclipse.paho.client.mqttv3.MqttMessage;

import java.nio.charset.StandardCharsets;
import java.util.ArrayDeque;
import java.util.Queue;

import lombok.Setter;

class MqttIWorkerClient implements IWorkerClient, IMqttMessageListener, IMqttActionListener {
    private static final String TAG = IWorkerClient.class.getName();

    private final Context context;
    private final String clientName;
    private final String url;
    private final String pushLogTopic;
    private final String exceptionTopic;
    private final String controlTopic;
    private final String controlResponseTopic;
    private final Object syncRoot = new Object();
    private final Queue<PendingMessage> pendingMessageQueue = new ArrayDeque<>();
    private MqttAndroidClient client;
    @Setter
    private CommandHandler commandHandler;

    public MqttIWorkerClient(Context context, String clientName, String url, String appName) {
        this.context = context;
        this.clientName = clientName;
        this.url = url;

        pushLogTopic = String.format(Constants.TOPIC_LOGS, appName);
        exceptionTopic = String.format(Constants.TOPIC_EXCEPTIONS, appName);
        controlTopic = String.format(Constants.TOPIC_CONTROLS, appName);
        controlResponseTopic = String.format(Constants.TOPIC_CONTROLS_RESPONSE, appName);
    }

    @Override
    public void start() throws LogMeException {
        stop();
        try {
            MqttAndroidClient client = new MqttAndroidClient(context, url, clientName);
            MqttConnectOptions options = new MqttConnectOptions();
            options.setAutomaticReconnect(true);
            client.connect(options, null, new IMqttActionListener() {
                @Override
                public void onSuccess(IMqttToken asyncActionToken) {
                    Log.i(TAG, "connect succeed");
                    try {
                        client.subscribe(controlTopic, 0, MqttIWorkerClient.this);
                    } catch (MqttException e) {
                        Log.e(TAG, "Error while subscribe " + controlTopic, e);
                    }
                }

                @Override
                public void onFailure(IMqttToken asyncActionToken, Throwable exception) {
                    Log.e(TAG, "connect failed", exception);
                }
            });
            synchronized (syncRoot) {
                this.client = client;
            }
        } catch (Exception e) {
            throw new LogMeException("Error while starting " + getClass().getName(), e);
        }
    }

    @Override
    public void stop() throws LogMeException {
        synchronized (syncRoot) {
            MqttAndroidClient client = this.client;
            if (client != null) {
                try {
                    client.disconnect();
                    client.close();
                    this.client = null;
                } catch (MqttException e) {
                    throw new LogMeException("Error while stopping " + getClass().getName(), e);
                }
            }
        }
    }

    @Override
    public void send(String message, MessageType messageType) throws LogMeException {
        synchronized (syncRoot) {
            MqttAndroidClient client = this.client;

            try {
                if (prepareSending(message, messageType, client)) {
                    sendPendingMessages(client);
                    sendImpl(message, messageType, client);
                }
            } catch (MqttException e) {
                throw new LogMeException("Error while sending '" + message + "' to " + pushLogTopic, e);
            }
        }
    }

    private boolean prepareSending(String message, MessageType messageType, MqttAndroidClient client) throws MqttException {
        if (client != null && !client.isConnected()) {
            client.connect().waitForCompletion();
        }

        boolean ready = client != null && client.isConnected();

        if (!ready) {
            pendingMessageQueue.add(new PendingMessage(message, messageType));
        }
        return ready;
    }

    private void sendPendingMessages(MqttAndroidClient client) throws MqttException {
        while (pendingMessageQueue.size() > 0) {
            PendingMessage pendingMessage = pendingMessageQueue.remove();
            sendImpl(pendingMessage.message, pendingMessage.messageType, client);
        }
    }

    private void sendImpl(String message, MessageType messageType, MqttAndroidClient client) throws MqttException {
        byte[] payload = message.getBytes(StandardCharsets.UTF_8);
        String topic = getTopicByMessageType(messageType);
        client.publish(topic, payload, 0, false).setActionCallback(this);
    }

    private String getTopicByMessageType(MessageType messageType) {
        switch (messageType) {
            case Log:
                return pushLogTopic;
            case Exception:
                return exceptionTopic;
            case ControlResponse:
                return controlResponseTopic;
        }
        return null;
    }

    @Override
    public void messageArrived(String topic, MqttMessage message) {
        byte[] payload = message.getPayload();
        Log.i(TAG, "topic: " + topic + ", msg: " + new String(payload));
        String command = new String(payload, StandardCharsets.UTF_8);
        handleControl(command);
    }

    private void handleControl(String command) {
        if (commandHandler != null) {
            commandHandler.handleCommand(command);
        }
    }

    @Override
    public void onSuccess(IMqttToken asyncActionToken) {
        Log.i(TAG, "onSuccess: send ok");
    }

    @Override
    public void onFailure(IMqttToken asyncActionToken, Throwable exception) {
        Log.e(TAG, "onFailure: sand failed", exception);
    }

    private static class PendingMessage {
        private final String message;
        private final MessageType messageType;

        PendingMessage(String message, MessageType messageType) {
            this.message = message;
            this.messageType = messageType;
        }
    }
}
