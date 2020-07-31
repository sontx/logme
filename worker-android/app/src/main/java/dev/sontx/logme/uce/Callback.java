package dev.sontx.logme.uce;

public interface Callback<T> {
    void run(T value);
}
