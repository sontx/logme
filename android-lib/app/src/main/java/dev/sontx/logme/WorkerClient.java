package dev.sontx.logme;

interface WorkerClient {
    void start() throws LogMeException;
    void stop() throws LogMeException;
    void send(String message, MessageType messageType) throws LogMeException;
}
