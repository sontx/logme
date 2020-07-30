package dev.sontx.logme.worker;

interface Constants {
    String COMMAND_GET_SYSTEM_INFO = "GET_SYSTEM_INFO";

    String TOPIC_LOGS = "%s/logs";
    String TOPIC_EXCEPTIONS = "%s/exceptions";
    String TOPIC_CONTROLS = "%s/controls";
    String TOPIC_CONTROLS_RESPONSE = "%s/controls/response";
}
