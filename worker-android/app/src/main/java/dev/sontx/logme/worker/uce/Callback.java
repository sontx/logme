package dev.sontx.logme.worker.uce;

public interface Callback<T> {
    void run(T value);
}
