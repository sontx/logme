package dev.sontx.logme;

public class LogMeException extends Exception {
    public LogMeException(String message, Throwable cause) {
        super(message, cause);
    }

    public LogMeException(String message) {
        super(message);
    }
}
