package dev.sontx.logme.worker;

interface IWorkerClient {
    void start() throws LogMeException;
    void stop() throws LogMeException;
    void send(String message) throws LogMeException;
}
