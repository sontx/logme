package dev.sontx.logme;

interface IWorkerClient {
    void start() throws LogMeException;
    void stop() throws LogMeException;
    void send(String message, MessageType messageType) throws LogMeException;
}
