package dev.sontx.logme.worker;

import android.content.Context;
import android.util.Log;

import org.eclipse.paho.android.service.MqttAndroidClient;
import org.eclipse.paho.client.mqttv3.IMqttActionListener;
import org.eclipse.paho.client.mqttv3.IMqttDeliveryToken;
import org.eclipse.paho.client.mqttv3.IMqttToken;
import org.eclipse.paho.client.mqttv3.MqttCallback;
import org.eclipse.paho.client.mqttv3.MqttConnectOptions;
import org.eclipse.paho.client.mqttv3.MqttException;
import org.eclipse.paho.client.mqttv3.MqttMessage;

import java.nio.charset.StandardCharsets;

class MqttIWorkerClient implements IWorkerClient, MqttCallback {
    private static final String TAG = IWorkerClient.class.getName();

    private final Context context;
    private final String name;
    private final String url;
    private final String topic;
    private MqttAndroidClient client;

    public MqttIWorkerClient(Context context, String name, String url, String topic) {
        this.context = context;
        this.name = name;
        this.url = url;
        this.topic = topic;
    }

    @Override
    public void start() throws LogMeException {
        stop();
        try {
            MqttAndroidClient client = new MqttAndroidClient(context, url, name);
            client.setCallback(this);
            client.connect(new MqttConnectOptions(), null, new IMqttActionListener() {
                @Override
                public void onSuccess(IMqttToken asyncActionToken) {
                    Log.i(TAG, "connect succeed");
                }

                @Override
                public void onFailure(IMqttToken asyncActionToken, Throwable exception) {
                    Log.e(TAG, "connect failed", exception);
                }
            });
            this.client = client;
        } catch (Exception e) {
            throw new LogMeException("Error while starting " + getClass().getName(), e);
        }
    }

    @Override
    public void stop() throws LogMeException {
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

    @Override
    public void send(String message) throws LogMeException{
        MqttAndroidClient client = this.client;
        if (client == null) {
            return;
        }

        try {
            if (!client.isConnected()) {
                client.connect();
            }

            byte[] payload = message.getBytes(StandardCharsets.UTF_8);
            client.publish(topic, payload, 0, false);
        } catch (MqttException e) {
            throw new LogMeException("Error while sending '" + message + "' to " + topic, e);
        }
    }

    @Override
    public void connectionLost(Throwable cause) {
        Log.i(TAG, "connectionLost", cause);
    }

    @Override
    public void messageArrived(String topic, MqttMessage message) throws Exception {
        Log.i(TAG, "topic: " + topic + ", msg: " + new String(message.getPayload()));
    }

    @Override
    public void deliveryComplete(IMqttDeliveryToken token) {
        Log.i(TAG, "msg delivered");
    }
}
