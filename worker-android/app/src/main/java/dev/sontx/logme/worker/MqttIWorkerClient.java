package dev.sontx.logme.worker;

import org.eclipse.paho.client.mqttv3.MqttClient;
import org.eclipse.paho.client.mqttv3.MqttException;
import org.eclipse.paho.client.mqttv3.persist.MemoryPersistence;

import java.nio.charset.StandardCharsets;

class MqttIWorkerClient implements IWorkerClient {
    private final String name;
    private final String url;
    private final String topic;
    private MqttClient client;

    public MqttIWorkerClient(String name, String url, String topic) {
        this.name = name;
        this.url = url;
        this.topic = topic;
    }

    @Override
    public void start() throws LogMeException {
        stop();
        try {
            MqttClient client = new MqttClient(url, name, new MemoryPersistence());
            client.connect();
            this.client = client;
        } catch (Exception e) {
            throw new LogMeException("Error while starting " + getClass().getName(), e);
        }
    }

    @Override
    public void stop() throws LogMeException {
        MqttClient client = this.client;
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
        MqttClient client = this.client;
        if (client == null || !client.isConnected()) {
            throw new LogMeException(getClass().getName() + " is not started yet");
        }
        byte[] payload = message.getBytes(StandardCharsets.UTF_8);
        try {
            client.publish(topic, payload, 0, false);
        } catch (MqttException e) {
            throw new LogMeException("Error while sending '" + message + "' to " + topic, e);
        }
    }
}
